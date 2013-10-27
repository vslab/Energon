#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#I @"C:\Users\davide\Documents\GitHub\Energon\bin\Debug"

#r @"Energon.Measuring.dll"
#r @"Energon.Storage.dll"
#r "System.Data.Linq.dll"
#r "System.Linq.dll"

#r "FSharp.PowerPack.Linq.dll"
//#r "FSharp.Data.TypeProviders.dll"

#r "System.Data.DataSetExtensions.dll"
#r "System.Core.dll"
#r "SQLExpress.dll"

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
open SQLExpress

let server = "MANDARINO\MISURATORE"
let dbname = "Measures"



// ------------------------ SAVE TO DB ----------------------

let system = "ImportedFromSpecWebsite_FP2006"

// db helper
//let server = "HPLAB\SQLEXPRESS"

let sensors = [| new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]

let createExp (prog:string) (system:string) =
  new Experiment(System.String.Format("{0}_{1}", prog, system), sensors, 0, [| "size" |], [||], fun _ -> ())

let createAndStartExp prog sys env =
  let e = createExp prog sys
  e.Note <- env
  let saver = new Energon.Storage.ExperimentRuntimeSaverExpress(e, server, dbname )
  new Energon.Measuring.Helper.ExperimentHelper(e)

let saveCpuSpecRun sys env prog time =
  let test = createAndStartExp prog sys env
  test.caseCallback([| "1.0" |])
  test.start()
  test.stop([| "ignore"; time |])


Console.WriteLine("---- CPU SPEC results import tool ----")
Console.WriteLine("this tool will import every CPU SPEC result from the CPU SPEC website, and save them in a local database. It will take several minutes to complete (to avoid DOS attacking CPU SPEC), so please be patient.")

    
let fetchFile (rel_addr:string) =
    let addr = "http://www.spec.org/cpu2006/results/" + rel_addr
    printfn "fetching %s" addr
    System.Threading.Thread.Sleep(1000)
    let wc = new System.Net.WebClient()
    let content = wc.DownloadString(addr)
    if content.StartsWith("valid,0") then
        ([], "", addr)
    else
        let makerInizio = "\"Selected Results Table\""
        let datiECoda = content.Substring(content.IndexOf(makerInizio) + makerInizio.Length)
        let soloDati = datiECoda.Substring(0, datiECoda.IndexOf("SPECfp"))
        let righe = soloDati.Split([| '\n' |])

        let readLine (line:string) =
            let tags = line.Split([| ',' |], System.StringSplitOptions.RemoveEmptyEntries)
            if (tags.Length = 0) then
                (false, "", "")
            else
                if tags.[0] = "Benchmark" then
                    (false, "", "")
                else
                    (true, tags.[0], tags.[2])
        let valoriDaFiltrare = Seq.map readLine righe
        let misure = Seq.filter (fun (b:bool,_,_) -> b) valoriDaFiltrare |> Seq.map (fun (_,a,b) -> a,b) |> Seq.toList
        (misure, "", addr)

//    let addr2 = "res2001q4/cpu2000-20011023-01096.asc"
//
//    let misure, setup = fetchFile addr2


Console.WriteLine("downloading index...")
let baseaddr = "http://www.spec.org/cpu2006/results/cfp2006.html" 
let wc = new System.Net.WebClient()
let content = wc.DownloadString(baseaddr)
Console.WriteLine("...index downloaded")

let split (text:string) (splitter:string) =
    let idx = text.IndexOf(splitter)
    if idx < 0 then
        text, ""
    else
        let pre = text.Substring(0, idx + splitter.Length)
        let post = text.Substring(idx + splitter.Length)
        pre, post

let rec analyse (urls:string list) (text:string) =
    let pre, post = split text ".csv"
    if post.Length = 0 then
        urls
    else
        let urlstart = pre.LastIndexOf("\"")
        let thisurl = pre.Substring(urlstart+1)::[]
        let newurls = List.append urls thisurl 
        analyse newurls post

let urls = analyse [] content

System.Console.WriteLine("index contains " + urls.Length.ToString() + " elements" )

//let urls2 = Seq.skip 2352 urls
//let runs = Seq.map fetchFile urls2
//urls.Length
//let m,s,a =fetchFile (urls.[0])
 

let runs = Seq.map fetchFile urls
let run_clean = Seq.filter (fun (l:(string*string) list,d:string,e:string) -> l.Length > 0) runs
//let runs_array = Seq.toArray run_clean

runs.Count()
run_clean.Count()

let save prog time system note  =
  saveCpuSpecRun system note prog time

let savePrograms ((l:(string*string) list),systemDiscard,note) =
  Seq.iter (fun (prog,time) -> save prog time system note ) l
  System.Console.WriteLine(note + " saved ")
  
      
Seq.iter savePrograms run_clean


// ------------------------ SAVE TO DB ----------------------




// ------------------------ SIMILARITY ----------------------

open System

let norm (a:float[]) =
  Math.Sqrt(Seq.fold (fun s e -> s + e * e) 0.0 a)

let cosineSimilarity (a:float[]) (b:float[]) =
  if a.Length <> b.Length then
    raise (new System.Exception("Can not compute cosine similarity of two vectors if they have different dimensionality"))
  let crossProduct = Seq.fold (fun s (ai,bi) -> s + ai * bi) 0.0 (Seq.zip a b)
  let norm_a = norm a
  let norm_b = norm b
  crossProduct / (norm_a * norm_b)

let angleBetweenVectors a b =
  Math.Acos(cosineSimilarity a b)

//cosineSimilarity [| 1.0; 1.0 |] [| -1.0; 1.0 |]
//angleBetweenVectors [| 1.0; 1.0 |] [| 2.0; 1.0 |]

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

// given 2 resources and 2 programs (the current basis), returns the program that maximizes the angle in the 3d space
let resourceSimilarity3 (a:float[]) (b:float[]) (c:float[]) (i1:int) (i2:int) =
  let zipped = Array.zip3 a b c
  let mutable maxAngle = 0.0
  let mutable besti = 0
  for i in 0..(a.Length - 1) do
      let a1, b1, c1 = zipped.[i]
      let a2, b2, c2 = zipped.[i1]
      let a3, b3, c3 = zipped.[i2]
      let a = [| a1; a2; a3 |]
      let b = [| b1; b2; b3 |]
      let c = [| c1; c2; c3 |]
      let alpha = angleBetweenVectors (a) (b) + angleBetweenVectors (a) (c) + angleBetweenVectors (b) (c)
      if alpha > maxAngle then
        maxAngle <- alpha
        besti <- i
  (maxAngle, besti)

// given the measures of N programs, it tells you which resources (first 2 indices) and which programs (last 2 indices) 
// should be used to build the splitup
let findMax2 (programs:float[][]) = 
  let mutable maxAngle = 0.0
  let mutable besti = 0
  let mutable bestj = 0
  let mutable bestprogi = 0
  let mutable bestprogj = 0
  let l = programs.GetLength(0)
  for i in 0..(l - 2) do
    for j in 0..(l - 1) do
      let ang, thisi, thisj = resourceSimilarity (programs.[i]) (programs.[j])
      if ang > maxAngle then
        maxAngle <- ang
        besti <- i
        bestj <- j
        bestprogi <- thisi
        bestprogj <- thisj
  (maxAngle, besti, bestj, bestprogi, bestprogj)

//let resourceSimilarityFixedBase (a:float[]) (b:float[])  =
//  let zipped = Array.zip a b
//  let mutable maxAngle = 0.0
//  let mutable besti = 0
//  let mutable bestj = 0
//  for i in 0..(zipped.Length - 1) do
//    for j in i..(zipped.Length - 1) do
//      let a1, a2 = zipped.[i]
//      let b1, b2 = zipped.[j]
//      let a = [| a1; a2|]
//      let b = [| b1; b2|]
//      let alpha = angleBetweenVectors (a) (b)
//      if alpha > maxAngle then
//        maxAngle <- alpha
//        besti <- i
//        bestj <- j
//  (maxAngle, besti, bestj)

let findThird (programs:float[][]) (b1:int) (b2:int)= 
  let mutable maxAngle = 0.0
  let mutable besti = 0
  let mutable bestprogi = 0
  let l = programs.GetLength(0)
  for i in 0..(l - 1) do
    let ang, thisi = resourceSimilarity3 (programs.[b1]) (programs.[b2]) (programs.[i]) b1 b2
    if ang > maxAngle then
        maxAngle <- ang
        besti <- i
        bestprogi <- thisi
  (maxAngle, besti, bestprogi)


// ------------- TODO ----------------
// given a set of measurements programs 
//let findMaxN (programs:float[][]) (basis_indices:int[]) = 
//  let mutable maxAngle = 0.0
//  let mutable besti = 0
//  let mutable bestj = 0
//  let mutable bestprogi = 0
//  let mutable bestprogj = 0
//  let rl = programs.GetLength(0)
//  let bl = basis.GetLength(0)
//  for i in 0..(rl - 1) do
//    for j in 0..(bl - 1) do
//      let ang, thisi, thisj = resourceSimilarity (programs.[i]) (basis.[j])
//      if ang > maxAngle then
//        maxAngle <- ang
//        besti <- i
//        bestj <- j
//        bestprogi <- thisi
//        bestprogj <- thisj
//  (maxAngle, besti, bestj, bestprogi, bestprogj)



// -------------------- Prediction Error -----------------
// returns the prediction error of a target program wrt a resource, using a splitup passed as argument
// outputs: measured value, predicted, error, error as a percentage
// input are:
// splitup: the splitup of the target program
// basis: the indices of the basis in the resource array, they need to be in the same order as the splitup
// target: the index of the target program in the resource array
// resource: measurements of a resource, one float for every program
let predictionError (splitup:float[]) (basis:int[]) (target:int) (resource:float[]) =
    let measured = resource.[target]
    let zipped = Array.zip splitup basis
    let predicted = Array.fold (fun (s:float) (split:float, i:int) -> s + resource.[i] * split) 0.0 zipped
    let error = (predicted-measured)/measured
    //let error = predicted/measured
    measured, predicted, error
//    if error > 0.0 then
//        measured, predicted, error
//    else
//        printf "ERROR : %f %f " measured predicted
//        measured, predicted, 1.0

let predictionErrors (splitup:float[]) (basis:int[]) (target:int) (resources:float[][]) = 
    Array.map (predictionError splitup basis target) resources

//let calcGeometricMeanAndStdDev (a:float[]) =
//    //Array.iter (printf "%f ") a
//    let prod = Array.fold (fun s v -> s*v) 1.0 a
//    let count = float (a.Length)
//    let mean = System.Math.Pow(prod, 1.0/count)
//    let sum = Array.map (fun v -> System.Math.Pow( System.Math.Log(v/mean), 2.0)) a |> Array.sum
//    //let sum = Array.map (fun v -> System.Math.Pow( System.Math.Log(v/1.0), 2.0)) a |> Array.sum
//    let squared = System.Math.Sqrt(sum/count)
//    let stddev = System.Math.Exp(squared)
//    //printfn "DEBUG %f %f" mean stddev
//    mean, stddev

let calcGeometricMeanAndStdDev (a:float[]) =
    //Array.iter (printf "%f ") a
    let sum = Array.fold (fun s v -> s + v) 0.0 a
    let count = float (a.Length)
    let mean = sum / count
    let sum2 = Array.fold (fun s v -> s + (v)*(v)) 0.0 a
    //let sum2 = Array.fold (fun s v -> s + (v-mean)*(v-mean)) 0.0 a
    let count2 = float (a.Length - 1)
    mean, System.Math.Sqrt(sum2 / count2)
    

// given the splitup of a target program and an array of resources, returns the average prediction error and std dev
let progErr (splitup:float[]) (basis:int[]) (target:int) (resources:float[][]) = 
    let errors = Array.map (fun (m:float,pred:float,err:float) -> err) (predictionErrors splitup basis target resources)
    let mean, stddev = calcGeometricMeanAndStdDev errors
    mean, stddev, errors, splitup


// --------------------------------------------------


let getConStr = 
    //let conStr = System.String.Format("server='{0}';database='{1}';User Id='{2}';password='{3}';", server, database, user, password) in
    let conStr = System.String.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", server, dbname) in
    conStr;
let GetLinqContext = 
    let context = new SQLExpress.Measure(getConStr)
    if (context.DatabaseExists() = false) then
            context.CreateDatabase()
    context
let db = 
    let context = GetLinqContext
    context.Connection.Open()
    context

let firstExpID = 30121
let limite = 4307
let suitesize = 17
let misure_all = db.Measures.Where(fun (m:Measures) -> m.ExperimentID >= firstExpID && m.ExperimentID < (firstExpID + limite*suitesize)).OrderBy(fun m -> m.ExperimentID) |> Seq.toList 
let misure_f = misure_all|> Seq.map (fun (m:Measures) -> (m.ExperimentID - firstExpID)/suitesize, m.AverageValue.Value)  |> Seq.groupBy fst |> Seq.map (fun (_,x) -> Seq.map snd x |> Seq.toArray)
let misure_array = Seq.toArray misure_f

let findUrl i = 
    db.Experiments.Where(fun (e:Experiments) -> e.Id = firstExpID + i*suitesize).First().Note


#r "GlpkProxy.dll"
open GlpkProxy

//// returns the measured, estimated, error and error as a percentage 
//let getEstimationErrorOnSystem (measured:float) (splitup:float[]) (testbed:float[]) =
//    let estimated = Seq.fold (fun (s:float) (a:float,b:float) -> s + a*b) 0.0 (Seq.zip splitup testbed)
//    let difference = Math.Abs(estimated - measured)
//    let percentage = difference / measured
//    measured, estimated, difference, percentage
//
//// returns the average estimation error, standard deviation, max error and index of max error
//let getEstimationError (measured:float[]) (splitup:float[]) (testbed:float[][]) =
//    let measures = Seq.zip measured testbed
//    let estimations = Seq.map (fun (m,t) -> getEstimationErrorOnSystem m splitup t) measures |> Seq.cache
//    let sum, count = Seq.fold (fun (s,i) (m,e,d,p) -> (s + p, i+1.0)) (0.0, 0.0) estimations
//    let average = sum / count
//    let sum2, count2 = Seq.fold (fun (s,i) (m,e,d,p) -> (s + (average - p) * (average - p), i+1.0)) (0.0, 0.0) estimations
//    let average2 = sum2 / count2
//    let stddev = Math.Sqrt(average2)
//    let selMax (maximum:float,i:int,maxi:int) (e:float,m:float,d:float,p:float) = if p > maximum then (p, i+1, i) else (maximum, i+1, maxi)
//    let maximum, i, maxi = Seq.fold selMax (0.0,0,0) estimations
//    average, stddev, maximum, maxi

// finds a program's splitup, splitupfinder needs to have the testbed already set
let getSplitup measures  (s:SplitupFinder) =
    let p = new Program()
    p.Measures <- measures
    s.Target <- p
    if (s.FindSplitup()) then
        s.Splitup
    else
        [|  |]


// ---------- print results ---------------
open System.Text

let printErrors (errors:(float*float*float[]*float[])[]) (n:int) =
    let allerrors = errors |> Array.map (fun (_,_,v,_) -> v) |> Array.fold (fun (s:float[]) (v:float[]) -> s.Concat(v) |> Seq.toArray) [| |]  |>  Array.sort
    let dict = new Dictionary<int,int>()
    let addOrIncrement (v:float) =
        let rounded = int (v*100.0)
        if dict.ContainsKey rounded then
            dict.[rounded] <- dict.[rounded] + 1
        else
            dict.Add(rounded, 1)
    let avg, stddev = calcGeometricMeanAndStdDev allerrors
    let sb = new System.Text.StringBuilder()
    sb.AppendLine("total error;")
    sb.AppendLine("avg err;MSE;")
    sb.AppendFormat("{0};{1};", avg, stddev)
    sb.AppendLine()
    sb.AppendLine("programs error;")
    sb.AppendLine("program;avg err;MSE;")
    for i in 0..(errors.Length-1) do
        let a,b,_,_ = errors.[i]
        sb.AppendFormat("{0};{1};{2}", i, a, b)
        sb.AppendLine()
        printfn "prog %i %f %f" i a b
    sb.AppendLine()
    sb.AppendLine("programs splitup;")
    sb.AppendLine("program;splitup;")
    for i in 0..(errors.Length-1) do
        let _,_,_,s = errors.[i]
        sb.AppendFormat("{0};", i)
        Array.iter (fun (si:float) -> sb.AppendFormat("{0};", si) |> ignore) s
        sb.AppendLine()
    sb.AppendLine()
    sb.AppendLine("buckets;")
    sb.AppendLine("bucket;number;")
    // merge all errors
    Array.iter addOrIncrement allerrors
    let printbucket bucket =
        let count = if dict.ContainsKey bucket then dict.[bucket] else 0
        sb.AppendFormat("{0};{1};", bucket, count)
        sb.AppendLine() |> ignore
    let buckets = dict.Keys |> Seq.toArray |> Array.sort
    for i in (buckets.First())..(buckets.Last()) do
        printbucket i    
    let filename = String.Format(@"C:\data\FP_errors_{0}.csv", n)
    System.IO.File.WriteAllText(filename, sb.ToString())
    printfn "saved to %s" filename
    printfn "mean=%f, std dev=%f"  avg stddev
    avg, stddev

let findCandidate (errors:(float*float*float[]*float[])[])  =
    let indexed = seq {
        for i in 0..(errors.Length-1) do
            let a,b,_,_ = errors.[i]
            let range = System.Math.Max(System.Math.Abs(a-b),System.Math.Abs(a+b))
            yield i,range }
    let cand,v = Seq.maxBy (fun (i,v) -> v) indexed
    cand


let justErrors (errors:(float*float*float[]*float[])[]) (n:int) =
    let allerrors = errors |> Array.map (fun (_,_,v,_) -> v) |> Array.fold (fun (s:float[]) (v:float[]) -> s.Concat(v) |> Seq.toArray) [| |]  |>  Array.sort
    let dict = new Dictionary<int,int>()
    let addOrIncrement (v:float) =
        let rounded = int (v*100.0)
        if dict.ContainsKey rounded then
            dict.[rounded] <- dict.[rounded] + 1
        else
            dict.Add(rounded, 1)
    let avg, stddev = calcGeometricMeanAndStdDev allerrors
    avg, stddev

// ---------- global variables about the experiment -----------------
let r = new Random()


let execCycle numberOfSystemsForModel =
    let numberOfSystemsForTest = misure_array.Length - numberOfSystemsForModel
    let selectedIndices = Seq.toArray (seq {
        for i in 1..numberOfSystemsForModel do
            yield r.Next(0,misure_array.Length - 1) 
        })
    let misure_with_indices = Array.init (misure_array.Length) (fun i -> i) |> Array.zip misure_array
    // this is the set of measures we'll use to build the model
    let misure_model = Array.filter (fun (v,i) -> selectedIndices.Contains(i)) misure_with_indices |> Array.map fst
    // this is the set of measures we'll use to test
    let misure_test = Array.filter (fun (v,i) -> not (selectedIndices.Contains(i))) misure_with_indices |> Array.map fst
    let testBedFromBasis (basis:int[])= 
        Array.map (fun i -> 
                let prog = new Program()
                prog.Measures <- Array.map (fun (v:float[]) -> v.[i]) misure_model
                prog
            ) basis
    let findSplitup i (s:SplitupFinder) =
        let p = new Program()
        p.Measures <- Array.map (fun (v:float[]) -> v.[i]) misure_model // risorse relative al programma
        s.Target <- p
        if s.FindSplitup() then
            s.Splitup
        else
            raise (System.Exception("Couldn't find a splitup"))

    // given the splitup of a target program and an array of resources, returns the average prediction error and std dev
    //let progErr (splitup:float[]) (basis:int[]) (target:int) (resources:float[][]) = 
    let getIthErr i (s:SplitupFinder) basis =
        let s1 = findSplitup i s
        printf "splitup for the %i th program is "
        Array.iter (printf "%f ") s1
        printfn ""
        progErr s1 basis i misure_test

    // prints the errors in a csv file and proposes the index of the program we should include in the basis next
    let avgErr (split:SplitupFinder) (basis:int[]) = 
        let allerrors =  Array.init suitesize (fun i -> i) |> Seq.fold (fun (s:(float*float*float[]*float[])[]) (i:int) -> 
            if (not (basis.Contains i)) then Array.concat [| s; [| getIthErr i split basis |] |] else s ) [| |] 
        let avg,std = printErrors allerrors basis.Length
        let cand = findCandidate allerrors
        let added = Array.fold (fun (s:int) (i:int) -> if i <= s then s+1 else s) cand (Array.sort basis)
        Console.WriteLine() 
        Console.WriteLine("basis is ")
        Array.iter (fun (i:int) -> Console.Write("{0} ", i) ) basis
        Console.WriteLine() 
        Console.WriteLine("cand={0} add={1}", cand, added)
        Console.WriteLine() 
        avg, std, added

    let exec p basis =
        let extendedBasis = Array.append basis [| p |]
        let s = new SplitupFinder()
        s.Testbed <- testBedFromBasis extendedBasis
        let avg, std, next = avgErr s extendedBasis
        avg, std, next, extendedBasis

    let p1 = r.Next(0, suitesize)
    let rec pump (avgs:float[]) (stddevs:float[]) (next:int) (basis:int[]) (nextHist:int[]) (n:int) =
        match n with
        | 0 -> avgs, stddevs, nextHist
        | _ -> let avg, std, next, extBasis = exec next basis
               let newAvgs = Array.append avgs [| avg |]
               let newStddevs = Array.append stddevs [| std |]
               let newHist = Array.append nextHist [| next |]
               pump newAvgs newStddevs next extBasis newHist (n-1)
    let res = pump (Array.empty<float>) (Array.empty<float>) p1 (Array.empty<int>) (Array.empty<int>) 12
    let sb = System.Text.StringBuilder()
    sb.AppendLine("average;MSE;introduced program")
    let a,b,c = res
    let zipped = Array.zip3 a b c
    Array.iter (fun (a:float,b:float,c:int) -> sb.AppendFormat("{0};{1};{2};", a, b, c) |> ignore; sb.AppendLine() |> ignore ) zipped
    let filename = @"C:\data\FP_cycle.csv"
    System.IO.File.WriteAllText(filename, sb.ToString())
    printfn "cycle saved to %s" filename


let findBestModelSize max incr =
    let runCycle numberOfSystemsForModel =
        let numberOfSystemsForTest = misure_array.Length - numberOfSystemsForModel
        let selectedIndices = Seq.toArray (seq {
            for i in 1..numberOfSystemsForModel do
                yield r.Next(0,misure_array.Length - 1) 
            })
        let misure_with_indices = Array.init (misure_array.Length) (fun i -> i) |> Array.zip misure_array
        // this is the set of measures we'll use to build the model
        let misure_model = Array.filter (fun (v,i) -> selectedIndices.Contains(i)) misure_with_indices |> Array.map fst
        // this is the set of measures we'll use to test
        let misure_test = Array.filter (fun (v,i) -> not (selectedIndices.Contains(i))) misure_with_indices |> Array.map fst
        let testBedFromBasis (basis:int[])= 
            Array.map (fun i -> 
                    let prog = new Program()
                    prog.Measures <- Array.map (fun (v:float[]) -> v.[i]) misure_model
                    prog
                ) basis
        let findSplitup i (s:SplitupFinder) =
            let p = new Program()
            p.Measures <- Array.map (fun (v:float[]) -> v.[i]) misure_model // risorse relative al programma
            s.Target <- p
            if s.FindSplitup() then
                s.Splitup
            else
                raise (System.Exception("Couldn't find a splitup"))

        // given the splitup of a target program and an array of resources, returns the average prediction error and std dev
        //let progErr (splitup:float[]) (basis:int[]) (target:int) (resources:float[][]) = 
        let getIthErr i (s:SplitupFinder) basis =
            let s1 = findSplitup i s
            printf "splitup for the %i th program is "
            Array.iter (printf "%f ") s1
            printfn ""
            progErr s1 basis i misure_test

        // prints the errors in a csv file and proposes the index of the program we should include in the basis next
        let avgErr (split:SplitupFinder) (basis:int[]) = 
            let allerrors =  Array.init suitesize (fun i -> i) |> Seq.fold (fun (s:(float*float*float[]*float[])[]) (i:int) -> 
                if (not (basis.Contains i)) then Array.concat [| s; [| getIthErr i split basis |] |] else s ) [| |] 
            let avg,std = justErrors allerrors basis.Length
            let cand = findCandidate allerrors
            let added = Array.fold (fun (s:int) (i:int) -> if i <= s then s+1 else s) cand (Array.sort basis)
            Console.WriteLine() 
            Console.WriteLine("avg={0} std={1}", avg, std)
            Console.WriteLine() 
            Console.WriteLine("basis is ")
            Array.iter (fun (i:int) -> Console.Write("{0} ", i) ) basis
            Console.WriteLine() 
            Console.WriteLine("cand={0} add={1}", cand, added)
            Console.WriteLine() 
            avg, std, added

        let exec p basis =
            let extendedBasis = Array.append basis [| p |]
            let s = new SplitupFinder()
            s.Testbed <- testBedFromBasis extendedBasis
            let avg, std, next = avgErr s extendedBasis
            avg, std, next, extendedBasis

        let p1 = r.Next(0, suitesize)
        let rec pump (avgs:float[]) (stddevs:float[]) (next:int) (basis:int[]) (nextHist:int[]) (n:int) =
            match n with
            | 0 -> avgs, stddevs, nextHist
            | _ -> let avg, std, next, extBasis = exec next basis
                   let newAvgs = Array.append avgs [| avg |]
                   let newStddevs = Array.append stddevs [| std |]
                   let newHist = Array.append nextHist [| next |]
                   pump newAvgs newStddevs next extBasis newHist (n-1)
        let res = pump (Array.empty<float>) (Array.empty<float>) p1 (Array.empty<int>) (Array.empty<int>) (suitesize - 4)
        let a,b,c = res
        let bestAvg = a.[a.Length - 1]
        let bestRMS = b.[b.Length - 1]
        numberOfSystemsForModel, bestAvg, bestRMS
    //runCycle max

    let start = suitesize
    let results = Array.init ((max-start)/incr) (fun i -> i * incr + start) |> Array.map runCycle
    let sb = System.Text.StringBuilder()
    sb.AppendLine("model size;average;RMS;")
    Array.iter (fun (a:int,b:float,c:float) -> sb.AppendFormat("{0};{1};{2};", a, b, c) |> ignore; sb.AppendLine() |> ignore ) results
    let filename = @"C:\data\model_size_FP.csv"
    System.IO.File.WriteAllText(filename, sb.ToString())
    printfn "cycle saved to %s" filename
    results

let res = findBestModelSize 500 20

execCycle 20




open Energon.Storage.Loader

let e = ExperimentLoader(1001, server, dbname)
let time = e.Sensors.First()
time
let r = e.ExperimentCases.[0].ExperimentRuns.[0]
r.Re
r.Readings
r.Results

e.ExperimentCases.[0].ExperimentRuns.[0]
e.Sensors
findMax resources
e.ExperimentCases.[0]



resourceSimilarity R1 R2
resourceSimilarity R2 R3
resourceSimilarity R1 R3
  
  for i in 0..(merged.GetLength(0) - 1) do
    
    printf "ciao"

resourceSimilarity [| [| 1.0 |]; [| 3.0 |]  |] [| [| 2.0 |]; [| 4.0 |] |]

let t = [| [| 1.0; 2.0 |] |]
t.GetLength(0)

runs_array.Length

let l1,s1,e1 = runrs_array.[0]
l1
s1

content.Count
urls.Length
urls.[2353]





let expID = 1000

let dbExperiment = db.Experiments.Where(fun (x:Experiments) -> x.Id = expID).First()
dbExperiment

let dbCases = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = expID)

let runsOfCase (case:ExperimentCases) =
    db.ExperimentRuns.Where(fun (x:ExperimentRuns) -> x.Experiment_case_id = case.Id)

let runs = runsOfCase (dbCases.First())
runs.First()
let run = new DatabaseExperimentRun([| |])
run

let sensorsOfRun (run:ExperimentRuns) =
    db.Sensors.Where(fun (x:Sensors) -> x.Experiment_run_id = run.Id)
// the list of sensors this experient is using
let sensors = new List<DatabaseSensor>()
let sensorToSensorClass (s:Sensors) =
    db.SensorClasses.Where(fun (cl:SensorClasses) -> cl.Id = s.Sensor_class_id)
let sensorClassToSensor (s:SensorClasses) =
    let sensArray = sensors.ToArray()
    let sens = sensArray.Where(fun (x:DatabaseSensor) -> x.ID = s.Id)
    if (sens.Count() > 0) then
        let newSens = new DatabaseSensor(s.SensorName)
        newSens.ID <- s.Id
        sensors.Add(newSens)
        newSens
    else
        sens.First()
let sensorToSensorClass (s:Sensors) =
    db.SensorClasses.Where(fun (x:SensorClasses) -> x.Id = s.Sensor_class_id).First()
let runseqseq = Seq.map runsOfCase dbCases 
let runseq2senseqseq (runseq:seq<ExperimentRuns>) =
    runseq |> Seq.map (fun (x:ExperimentRuns) ->
            sensorsOfRun x
        )
let handleSensors (s:Sensors) =
    let sensRes = sensors.Where(fun (x:DatabaseSensor) -> x.ID = s.Sensor_class_id)
    if (sensRes.Count() = 0) then
        let cl = sensorToSensorClass s
        let newSens = new DatabaseSensor(cl.SensorName)
        newSens.ID <- cl.Id
        sensors.Add(newSens)
runseqseq |> Seq.iter (fun (runseq:seq<ExperimentRuns>) -> 
        let sensseqseq = runseq2senseqseq runseq
        sensseqseq |> Seq.iter ( fun (sensseq:seq<Sensors>) -> 
            Seq.iter handleSensors sensseq
        )
    )  
let exp = new DatabaseExperiment(dbExperiment.Name, sensors, dbExperiment.Iter.Value, Seq.empty, Seq.empty)
let sensorSeq = seq {
        for s in sensors do yield s:>GenericSensor
    }
dbCases |> Seq.iter (fun (x:ExperimentCases) ->
        let split = [";"].ToArray()
        let args = x.Args.Split(split, StringSplitOptions.RemoveEmptyEntries)
        let c = new DatabaseExperimentCase(sensorSeq, exp.IterCount,  args |> Seq.map (fun x -> x :> obj) )
        exp.ExperimentCases.Add(c)
        let dbRuns = runsOfCase x
        dbRuns |> Seq.iter ( fun (r:ExperimentRuns) ->
            let run = new DatabaseExperimentRun(sensorSeq)
            c.ExperimentRuns.Add(run)
        )
    )
exp
