

open System
open System.Text
open System.Collections
open System.Collections.Generic

#I @"C:\Users\davidemorelli\Documents\GitHub\Energon\DataLayer"
#I @"C:\Users\davidemorelli\Documents\GitHub\Energon\DataLayer\bin\Debug"
#I @"C:\Users\davidemorelli\Documents\GitHub\Energon\DataLayer\bin\Debug\x86"
#r @"DataLayer.dll"
#r @"linq2db.dll"
open DataLayer

let dbfile = @"C:\Users\davidemorelli\Documents\GitHub\Energon\DataLayer\SPECCPU.sqlite"

let helper = new SPECCPUHelper()
    
// -------------- GET DATA FROM www.spec.org ----------------

Console.WriteLine("downloading index...")
let baseaddr = "http://www.spec.org/cpu2006/results/cint2006.html" 
//let baseaddr = "http://www.spec.org/cpu2006/results/cfp2006.html" 
let wc = new System.Net.WebClient()
let content = wc.DownloadString(baseaddr)
Console.WriteLine("...index downloaded")

open System.Text.RegularExpressions

let urls = seq {
  let urlsL = Regex.Matches(content, @"href=""(.*\.csv)""")
  for i in 0..(urlsL.Count-1) do
    yield urlsL.Item(i).Groups.Item(1).Value
  }

//let firstCsv = urls |> Seq.head

let processUrl firstCsv =
    let addr = "http://www.spec.org/cpu2006/results/" + firstCsv
    printfn "fetching %s" addr
    System.Threading.Thread.Sleep(500)
    let wc = new System.Net.WebClient()
    let content = wc.DownloadString(addr)
    if content.StartsWith("valid,1") then
        let righe = content.Split([| '\n' |])
        let stuff = Regex.Matches(content, @"Selected.Results.Table([\s\S]*)HARDWARE([\s\S]*)SOFTWARE([\s\S]*)Base.Compiler.Invocation")
        let results = stuff.Item(0).Groups.Item(1).Value
        let hw = stuff.Item(0).Groups.Item(2).Value
        let sw = stuff.Item(0).Groups.Item(3).Value
        let resultsLines = results.Split([|'\n'|])
        let foldProg (progs: (string*double*double) list) (line:string) =
            let tags = line.Split([|','|])
            let progName = tags.[0]
            if (Regex.Match(line, @"^\d\d\d[\S\s]*").Captures.Count > 0) then
                let tagT1 = tags.[1]
                let tagT2 = tags.[2]
                System.Console.WriteLine(@"{0} {1} {2}", tags.[0], tagT1, tagT2)
                (progName,double(tagT1),double(tagT2))::progs
            else
                progs
        let progs = resultsLines |>  Seq.fold foldProg (List.empty<string*double*double>)
        let getItem (l:string) (item:string) =
            let filterForItem (line:string) =        
                let tags = line.Split([|','|])
                let name = tags.[0].Replace("\"","")
                if (name.Equals(item)) then
                    true
                else
                    false
            let res = l.Split([|'\n'|]) |> Seq.find filterForItem
            res.Substring(res.IndexOf(",")+1).Replace("\"","")
        let createExp (n:string,v1:double,v2:double) = helper.SaveNewExperiment(dbfile, firstCsv, getItem hw "CPU Name", getItem hw "CPU MHz", getItem hw "FPU", getItem hw "Memory", getItem sw "Operating System", getItem sw "Compiler", n, new decimal(v1), new decimal(v2));
        progs |> Seq.iter createExp

// only process the first 2
//urls |> Seq.take 2 |> Seq.iter processUrl

// do all of them
urls |> Seq.iter processUrl

// don't process anything before a certain url (useful in case network connection is lost, to resume the job) 
urls |> Seq.fold (fun s u -> if s then processUrl u; true else if u.Equals(@"res2012q4/cpu2006-20121119-25188.csv") then true else false) false  

urls |> Seq.length
urls |> Seq.skip 4000






// ------------------ USE DATA --------------------

let datafolder = @"C:\Users\davidemorelli\Documents\GitHub\Energon\data"

#I @"C:\Users\davidemorelli\Documents\GitHub\Energon\bin\Debug"
#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#I @"C:\Users\davide\Documents\GitHub\Energon\bin\Debug"

#r @"Energon.Measuring.dll"
#r @"Energon.Storage.dll"
#r "System.Data.Linq.dll"

#r "FSharp.Data.TypeProviders.dll"

#r "System.Data.DataSetExtensions.dll"
#r "System.Core.dll"

open Energon.Storage
open System
//open Microsoft.FSharp.Data.TypeProviders
open System.Data.Linq.SqlClient
open System.Linq
open Microsoft.FSharp.Linq
open System.Data.Linq
open Energon.Measuring
open Energon.Measuring.Database
open System.Text
open System.Collections.Generic

// TODO: 1 dictionary for programs and one for the testbed
let programNames = new Dictionary<int,string>()
let programPriority = new List<float>(29);
for i in 0..28 do
    programPriority.Add(0.0)

// ------------------------ SIMILARITY ----------------------

open System

let norm1 (a:float[]) =
  Seq.fold (+) 0.0 a

let norm2 (a:float[]) =
  Math.Sqrt(Seq.fold (fun s e -> s + e * e) 0.0 a)

let cosineSimilarity (a:float[]) (b:float[]) =
  if a.Length <> b.Length then
    raise (new System.Exception("Can not compute cosine similarity of two vectors if they have different dimensionality"))
  let crossProduct = Seq.fold (fun s (ai,bi) -> s + ai * bi) 0.0 (Seq.zip a b)
  let norm_a = norm2 a
  let norm_b = norm2 b
  crossProduct / (norm_a * norm_b)

let angleBetweenVectors a b =
  Math.Acos(cosineSimilarity a b)

// given 2 arrays on N values, it builds N vectors of 2 dimesions, checks the angle between each, and return the larges angle, with indices
// so, given the measures of 2 programs, it will tell you which couple of resources should be used to get the testbed later on
let resourceSimilarity (a:float[]) (b:float[]) =
  let zipped = Array.zip a b
  let mutable maxAngle = 0.0
  let mutable besti = 0
  let mutable bestj = 0
  for i in 0..(zipped.Length - 1) do
    for j in i..(zipped.Length - 1) do
      let a1, b1 = zipped.[i]
      let a2, b2 = zipped.[j]
      let a = [| a1; a2|]
      let b = [| b1; b2|]
      let alpha = angleBetweenVectors (a) (b)
      if alpha > maxAngle then
        maxAngle <- alpha
        besti <- i
        bestj <- j
  (maxAngle, besti, bestj)


#r "GlpkProxy.dll"
open GlpkProxy

let cone = false


// ---------- global variables about the experiment -----------------
let r = new Random()

let normaliseMeasures (measures:float[][]) =
    let normRow (r:float[]) =
        let max = r |> Array.max
        r |> Array.map (fun m -> m/max)
    measures |> Array.map normRow

// --------------- load from sqlite ------------ 

open DataLayer

let db = helper.getDB(dbfile)
let envs = db.Environments

// envs |> Seq.length
//let e = envs |> Seq.skip 100 |> Seq.head  // first env

// preparing a dictionary of all programs
let progsDict = new Dictionary<int64, DataModel.Program>()
let progs = helper.getPrograms(db);
progs |> Seq.iter (fun p -> progsDict.Add(p.ID, p))

let orderedProgs = progs.OrderBy(fun p -> p.ID)
for i in 0..(orderedProgs.Count() - 1) do
    programNames.Add(i, orderedProgs.ElementAt(i).Name)

//programNames
//progs |> Seq.length

// all experiments of an environment
//let exps = helper.getExperimentsOfEnvironment(db, e.ID)
//let printExp (e:DataModel.Experiment) =
//    let p = progsDict.[(int64)e.ProgID.Value]
//    System.Console.WriteLine("{0} {1} {2}", p.Name, e.BaseRefTime, e.BaseRunTime)

let getExps (e:DataModel.Environment) = helper.getExperimentsOfEnvironment(db, e.ID)
let envsArray = envs |> Seq.toArray
let allexps = envsArray |> Array.map getExps
let chosenExps = allexps |> Array.filter (fun exps -> Array.length exps = 29) |> Seq.cache

//allexps  |> Seq.length
//chosenExps |> Seq.length
//chosenExps |> Seq.head

let expDict = new Dictionary<int64,Dictionary<int64,decimal>>()
let handleEnv (e:DataModel.Environment) =
    if not (expDict.ContainsKey(e.ID)) then
        expDict.Add(e.ID, new Dictionary<int64,decimal>())
    let dict = expDict.[e.ID]
    let exps = helper.getExperimentsOfEnvironment(db, e.ID)
    let handleExp (exp:DataModel.Experiment) =
        if not (dict.ContainsKey(exp.ID)) then
            dict.Add(exp.ID,exp.BaseRunTime.Value)
    exps |> Seq.iter handleExp 

envsArray |> Seq.iter handleEnv
let validEnvs = new List<int64>()

for e in expDict.Keys do
    let count = expDict.[e].Count
    if count = 29 then
        validEnvs.Add(e)

validEnvs.Count

let measuresSeq = 
        let doEnv envID =
            let exps = expDict.[envID]
            let sortedExps = exps.Keys.ToArray() |> Array.sort
            let retSeq = sortedExps |> Array.map (fun i -> float(exps.[i]))
            retSeq
        validEnvs |> Seq.map doEnv
let measures = measuresSeq |> Seq.toArray

db.Close()


let values_array =  measures

let values_array = [|
                        [| 3.0; 0.5; 1.5; 0.25 |]
                        [| 0.5; 2.0; 0.25; 1.0 |]
                        [| 3.0; 1.0; 1.5; 0.51 |]
                        [| 2.0; 1.0; 1.0; 0.52 |]
                    |]

// split the measures into separate sets for model and test
let modelSize = 2
let values_model, values_test = 
    let chunkSize = values_array.Length / modelSize
    let indices = Array.init (modelSize) (fun i -> i*chunkSize + int(r.NextDouble()*float(chunkSize)))
    let wIndices = values_array |> Array.mapi (fun i r -> i,r)
    let model = wIndices |> Array.filter (fun (i,r) -> indices.Contains(i)) |> Array.map snd
    let test = wIndices |> Array.filter (fun (i,r) -> not (indices.Contains(i))) |> Array.map snd
    normaliseMeasures model, test

let measuresOfProg (measures:float[][]) i =
    measures |> Seq.map (fun row -> row.[i]) |> Seq.toArray

// the error of a program on a resource
let programError (measured:float) (testbed:float[]) (splitup:float[]) =
    let zip = Array.zip testbed splitup
    let predicted = zip |> Array.fold (fun s (v1,v2) -> s + v1*v2) 0.0
    let error = (predicted - measured)/measured
    error

// the error of a program on all resources
let programErrors (measured:float[]) (testbed:float[][]) (splitup:float[]) =
    let zip = Array.zip measured testbed
    let errors = zip |>  Array.map (fun (m,t) -> programError m t splitup)
    let average = errors |> Array.average
    let MSE = (Array.fold (fun s v -> s+v*v) 0.0 errors) / float(errors.Length)
    average, MSE, errors

let allProgramsErrors (measures_model:float[][]) (measures_test:float[][]) (basis:int[]) =
    let programsIndices = Array.init (measures_test.[0].Length) (id) |> Array.filter (fun i -> not (basis.Contains(i)))
    let getRow (measures:float[][]) i =
        measures |> Array.map (fun r -> r.[i])
    let filterRows (measures:float[][]) (indices:int[]) =
        let filterRow (r:float[]) =
            r |> Array.mapi (fun i v -> i,v) |> Array.filter (fun (i,v) -> indices.Contains(i)) |> Array.map snd
        measures |> Array.map filterRow
    let testbed_model = filterRows measures_model basis
    let testbed_test =  filterRows measures_test basis
    let programs_model =   filterRows measures_model programsIndices
    let programs_test =   filterRows measures_test programsIndices
    let s = new SplitupFinder()
    let programs = Array.init (basis.Length) (id) |> Array.map (getRow testbed_model) |> Array.map (fun r -> 
            let p = new Program()
            p.Measures <- r
            p
        )
    s.Testbed <- programs
    let handleProg (i:int) =
        let model = getRow programs_model i
        let test = getRow programs_test i
        let name = programNames.[programsIndices.[i]]
        let idx = programsIndices.[i]
        let p = new Program()
        p.Measures <- model
        s.Target <- p
        if (s.FindSplitup(cone)) then
            let splitup = s.Splitup
            let a,d, e = programErrors test testbed_test splitup
            idx, name, a, d, e
        else
            System.Console.WriteLine("could not find a splitup for program {0} {1}", i, name)
            idx, name, 0.0, 0.0, [| |]
    // prepare array with index, model and test data for each program
    let res = Array.init (programsIndices.Length) (id) |> Array.map (handleProg)
    res


//let res = allProgramsErrors values_model values_test [| 0; 1 |]

// computing the similarity matrix of programs
let nOfPrograms = Seq.length values_array.[0]
let listOfIndices = List.init nOfPrograms (fun i -> i)
let similarityMatrix = Array2D.create nOfPrograms nOfPrograms 0.0
for i in 0..(nOfPrograms - 1) do
    let a = measuresOfProg values_array i
    for j in 0..(nOfPrograms - 1) do
        let b = measuresOfProg values_array j
        similarityMatrix.[i,j] <- cosineSimilarity a b

// print the simialrity matrix as a latex table
(*
let printLatexTable =
    let sb = new StringBuilder()
    sb.Append("\\begin{tabular}{r ")
    Array.init nOfPrograms (fun a -> a) |> Array.map (fun _ -> sb.Append("| c "))
    sb.AppendLine("}")
    sb.Append(" ")
    Array.init nOfPrograms (fun a -> a) |> Array.map (fun a -> sb.AppendFormat("& {0}", programNames.[a].Substring(0, 3)))
    sb.AppendLine(" \\\\ \\hline")
    sb.ToString()
    for i in 0..(nOfPrograms - 1) do
        sb.Append(programNames.[i].Substring(0, 3)) |> ignore
        for j in 0..(nOfPrograms - 1) do
            let sim = similarityMatrix.[i,j]
            sb.AppendFormat("&{0:G2}", sim)  |> ignore
        sb.AppendLine("\\\\")  |> ignore
    sb.Append("\\end{tabular}")
*)
// print the similarity matrix as a ascii table for gnuplot
let printAsciiTable =
    let sb = new StringBuilder()
    for i in 0..(nOfPrograms - 1) do
        for j in 0..(nOfPrograms - 1) do
            let sim = similarityMatrix.[i,j]
            sb.AppendFormat("{0} ", sim)  |> ignore
        sb.AppendLine("")  |> ignore
    System.IO.File.WriteAllText(@"Z:\Projects\thesis\energon\articoli\CPUSPEC\data\correlation.txt", sb.ToString())    


let rec measureSimilarity (attempt:int[]) =
    if attempt.Length = 2 then
        similarityMatrix.[attempt.[0], attempt.[1]]
    else
        let a = attempt.[0]
        let sum = attempt |> Seq.skip 1 |> Seq.sumBy (fun b -> similarityMatrix.[a,b])
        sum + measureSimilarity (attempt |> Seq.skip 1 |> Seq.toArray)


// normalize 
let normalise f a =
    let n:float = f a
    a |> Array.map (fun i -> i / n)
let normalise2 = normalise norm2
let normalise1 = normalise norm1

let normalised_measure_model measure_model = 
    let getIth i = measure_model |> Array.map (fun (l:float[]) -> l.[i])
    let indices = Array.init (measure_model.[0].Length) (id)
    indices |> Array.map getIth |> Array.map normalise1 

let normalised_points = normalised_measure_model values_model
let points = normalised_points

// convex hull
#I @"C:\Users\davidemorelli\AppData\Roaming\Local Libraries\Local Documents\GitHub\Energon\Debug"
#r "MIConvexHullPlugin.dll"

let hull = MIConvexHull.ConvexHull.Create(points)
let hull_points = hull.Points

hull_points |> Seq.length

let findProg r =
    let same a =
        Seq.forall2 (fun v w -> v = w) a r
    normalised_points |> Seq.findIndex same

let basis = hull_points |> Seq.map (fun p -> p.Position) |>  Seq.map findProg |> Seq.toArray |> Array.sort

let res = allProgramsErrors values_model values_test basis
let averages = 
    let a = res |> Array.map (fun (i,n,a,d,e) -> a) |> Array.average
    let d = res |> Array.map (fun (i,n,a,d,e) -> d) |> Array.average
    let mse = res |> Array.map (fun (i,n,a,d,e) -> e) |> Array.concat |> Array.map (fun v -> v*v) |> Array.sum
    let count = res |> Array.map (fun (i,n,a,d,e) -> e) |> Array.concat |> Array.length
    a,d,mse/(float(count))

let saveAveragesOnFile (averages:float*float*float) =
    let sb = new System.Text.StringBuilder()
    sb.AppendLine("Testbed;")
    basis |> Array.map (fun i -> sb.AppendFormat("{0};", programNames.[i]); sb.AppendLine())
    sb.AppendLine()
    sb.AppendLine("Program;average error; MSE;")
    res |> Array.map (fun (i,n,a,d,e) -> sb.AppendFormat("{0};{1};{2}", programNames.[i], a, d); sb.AppendLine())
    sb.AppendLine()
    sb.AppendLine()
    let a,d,mse = averages
    sb.AppendFormat("average; {0};", a)
    sb.AppendLine()
    sb.AppendFormat("MSE; {0};", mse)
    sb.AppendLine()
    System.IO.File.WriteAllText(@"C:\Users\davidemorelli\AppData\Roaming\Local Libraries\Local Documents\GitHub\Energon\data\MSE.csv", sb.ToString())

saveAveragesOnFile averages

// k-means or mesh semplification

// http://www.cs.uu.nl/research/techreps/repo/CS-2007/2007-038.pdf

// http://webdocs.cs.ualberta.ca/~anup/Courses/604_3DTV/Presentation_files/Polygon_Simplification/7.pdf



