﻿#I @"C:\Users\root\Desktop\Energon\bin\Debug"
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

let system = "ImportedFromSpecWebsite"

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
    let wc = System.Net.WebClient()
    let content = wc.DownloadString(addr)
    let righe = content.Split([| '\n' |])
    let filtro (last:string) (b:bool,starr) (c:string) = 
        if b then
            if c.Trim().StartsWith(last) then
            (false,starr)
            else
            let tokens = c.Trim().Split([| ' ' |], System.StringSplitOptions.RemoveEmptyEntries)
            if tokens.Length < 8 then
                (true,starr)
            else
                let n = (tokens.[0], tokens.[6])       
                (true,n::starr)
        else
            if c.Trim().StartsWith("====") then
            (true,starr)
            else
            (false,starr)
    let setup  = 
        let from = content.IndexOf("HARDWARE")
        let upto = content.IndexOf("Operating System Notes")
        content.Substring(from, upto-from)
    let filtroSpecInt = filtro "SPECint(R)_base2006"
    let filtroSpecFPU = filtro "SPECfpu(R)_base2006"
    let filtro =
        if content.Trim().StartsWith("SPEC(R) CINT2006 Summary") then
            filtroSpecInt
        else
            filtroSpecFPU
    let _, misure = Seq.fold filtro (false, []) righe  
    (misure, setup, addr)

//    let addr2 = "res2001q4/cpu2000-20011023-01096.asc"
//
//    let misure, setup = fetchFile addr2


Console.WriteLine("downloading index...")
let baseaddr = "http://www.spec.org/cpu2006/results/cpu2006.html" 
let wc = new System.Net.WebClient()
let content = wc.DownloadString(baseaddr)
Console.WriteLine("...index downloaded")

let avanza (testo:string) (dove:string) = 
    let indice = testo.IndexOf(dove)
    if indice + dove.Length < testo.Length then
      let prima = testo.Substring(0, indice + dove.Length)
      prima, testo.Substring(indice + dove.Length)
    else
      ("",testo)
  
let pre_table, post_pre_table = avanza content "<table"
let table, post_table = avanza post_pre_table "</table"
let _, body = avanza table "<tbody>"
let _, from_second = avanza body "<tr>"
let rec analyse (urls,blob) =
    let second, from_third = avanza blob "<tr>"
    if second.Length > 0 then
        let _, from_href = avanza second "<a href=\""
        let _, from_href2 = avanza from_href "<a href=\""
        let _, from_href3 = avanza from_href2 "<a href=\""
        let link, _ = avanza from_href3 ".txt"
        if link.Length > 0 then
            analyse (link::urls, from_third)
        else
            urls,blob
    else
        urls,blob

let urls, blob = analyse ([], from_second)

System.Console.WriteLine("index contains " + urls.Length.ToString() + " elements" )

//let urls2 = Seq.skip 2352 urls
//let runs = Seq.map fetchFile urls2
//urls.Length

let runs = Seq.map fetchFile urls
let run_clean = Seq.filter (fun (l:(string*string) list,d:string,e:string) -> l.Length > 0) runs
//let runs_array = Seq.toArray run_clean


let save prog time system note  =
  saveCpuSpecRun system note prog time

let savePrograms ((l:(string*string) list),systemDiscard,note) =
  Seq.iter (fun (prog,time) -> save prog time system note ) l
  System.Console.WriteLine(note + " saved ")
  System.Threading.Thread.Sleep(2000)
      
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

let resourceSimilarityFixedBase (a:float[]) (b:float[])  =
  let zipped = Array.zip a b
  let mutable maxAngle = 0.0
  let mutable besti = 0
  let mutable bestj = 0
  for i in 0..(zipped.Length - 1) do
    for j in i..(zipped.Length - 1) do
      let a1, a2 = zipped.[i]
      let b1, b2 = zipped.[j]
      let a = [| a1; a2|]
      let b = [| b1; b2|]
      let alpha = angleBetweenVectors (a) (b)
      if alpha > maxAngle then
        maxAngle <- alpha
        besti <- i
        bestj <- j
  (maxAngle, besti, bestj)


// ------------- TODO ----------------
// given a set of measurements programs 
let findMaxN (programs:float[][]) (basis_indices:int[]) = 
  let mutable maxAngle = 0.0
  let mutable besti = 0
  let mutable bestj = 0
  let mutable bestprogi = 0
  let mutable bestprogj = 0
  let rl = programs.GetLength(0)
  let bl = basis.GetLength(0)
  for i in 0..(rl - 1) do
    for j in 0..(bl - 1) do
      let ang, thisi, thisj = resourceSimilarity (programs.[i]) (basis.[j])
      if ang > maxAngle then
        maxAngle <- ang
        besti <- i
        bestj <- j
        bestprogi <- thisi
        bestprogj <- thisj
  (maxAngle, besti, bestj, bestprogi, bestprogj)



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
    let error = System.Math.Abs(predicted-measured)
    measured, predicted, error, error / measured

let predictionErrors (splitup:float[]) (basis:int[]) (target:int) (resources:float[][]) = 
    Array.map (predictionError splitup basis target) resources

// given the splitup of a target program and an array of resources, returns the average prediction error and std dev
let progErr (splitup:float[]) (basis:int[]) (target:int) (resources:float[][]) = 
    let errors = Array.map (fun (m:float,pred:float,err:float,perc:float) -> perc) (predictionErrors splitup basis target resources)
    let average = errors.Average()
    let sum = Array.fold (fun (s:float) (v:float) -> s + (v-average) * (v-average) ) 0.0 errors
    let c = float (errors.Count())
    average, System.Math.Sqrt(sum / c )


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

let limite = 130
let misure_all = db.Measures.Where(fun (m:Measures) -> m.ExperimentID >= 319 && m.ExperimentID < (319 + limite*12)).OrderBy(fun m -> m.ExperimentID) |> Seq.toList 
let misure_f = misure_all|> Seq.map (fun (m:Measures) -> (m.ExperimentID - 319)/12, m.AverageValue.Value)  |> Seq.groupBy fst |> Seq.map (fun (_,x) -> Seq.map snd x |> Seq.toArray)
let misure_array = Seq.toArray misure_f

let findUrl i = 
    db.Experiments.Where(fun (e:Experiments) -> e.Id = 319 + i*12).First().Note



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

// ---------- testbed size = 1 ------------------

// just pick a random program as the tested
let rand = new System.Random()
let chosenProg = rand.Next(0, 12)
let chosenSystem = rand.Next(0, misure_array.Length)
let chosenMeasures = misure_array.[chosenSystem]
// the splitups are just the ratio..
let energon = chosenMeasures.[chosenProg]
let splitup1 = Array.map (fun v -> v/energon) chosenMeasures

let measuresWithoutSelectedResources skip_index = 
    let f,i = Array.fold (fun (ret:float[][], i) (x:float[]) -> if i = skip_index then ret, i+1 else Array.append ret [| x |], i+1 ) ([| |], 0) misure_array
    f

let otherMeasures = measuresWithoutSelectedResources chosenSystem

// given the splitup of a target program and an array of resources, returns the average prediction error and std dev
//let progErr (splitup:float[]) (basis:int[]) (target:int) (resources:float[][]) = 
let getIthErr1 i =
    let s1 = [| splitup1.[i] |]
    progErr s1 [| chosenProg |] i otherMeasures

getIthErr1 0

let avgErr1 = 
    let sum = Seq.init 12 (fun i -> i) |> Seq.fold (fun (s:float) (i:int) -> 
        if i <> chosenProg then s + fst (getIthErr1 i) else s ) 0.0
    let c = float 11
    sum / c





//
//

//
////getEstimationErrorOnSystem (measured:float) (splitup:float[]) (testbed:float[]) =
//// getEstimationError (measured:float[]) (splitup:float[]) (testbed:float[][])
//let getErrors = seq {
//    let only i = Array.map (fun (x:float[]) -> x.[i]) misure_array
//    for i in 0..11 do
//        yield getEstimationError (only i) splitup1 [| (only chosenProg) |]
//    }
//let errorsArray = Seq.toArray getErrors       


// ---------- testbed size 2, use max angle -------------

// first 2 resources and programs
let angle, r1, r2, p1, p2 = findMax2 misure_array

let url_r1 = findUrl r1
let url_r2 = findUrl r2

let chosen_res_array = Array.rev (Array.sort [| r1; r2 |])
let skipped1 = measuresWithoutSelectedResources chosen_res_array.[0]
let otherMeasures2 = measuresWithoutSelectedResources chosen_res_array.[1]
let res1 = misure_array.[r1]
let res2 = misure_array.[r2]
let progP1 = new Program()
progP1.Measures <- [| res1.[p1]; res2.[p1] |]
let progP2 = new Program()
progP2.Measures <- [| res1.[p2]; res2.[p2] |]
let s = new SplitupFinder()
s.Testbed <- [| progP1; progP2 |]

let findSplitup2 i =
    let p = new Program()
    p.Measures <- [| res1.[i]; res2.[i] |] // risorse relative al programma
    s.Target <- p
    if s.FindSplitup() then
        s.Splitup
    else
        [|0.0; 0.0 |]

let getIthErr2 i =
    let s1 = findSplitup2 i
    progErr s1 [| p1; p2 |] i otherMeasures2

let avgErr2 = 
    let sum = Seq.init 12 (fun i -> i) |> Seq.fold (fun (s:float) (i:int) -> 
        if i <> p1 && i <> p2 then 
            s + fst (getIthErr2 i)
        else s) 0.0
    let c = float 10
    sum / c




    // TODO
// ---------- testbed size 3, use max angle -------------

// first 2 resources and programs
let angle, r1, r2, p1, p2 = findMax2 misure_array

let url_r1 = findUrl r1
let url_r2 = findUrl r2

let chosen_res_array = Array.rev (Array.sort [| r1; r2 |])
let skipped1 = measuresWithoutSelectedResources chosen_res_array.[0]
let otherMeasures2 = measuresWithoutSelectedResources chosen_res_array.[1]
let res1 = misure_array.[r1]
let res2 = misure_array.[r2]
let progP1 = new Program()
progP1.Measures <- [| res1.[p1]; res2.[p1] |]
let progP2 = new Program()
progP2.Measures <- [| res1.[p2]; res2.[p2] |]
let s = new SplitupFinder()
s.Testbed <- [| progP1; progP2 |]

let findSplitup2 i =
    let p = new Program()
    p.Measures <- [| res1.[i]; res2.[i] |] // risorse relative al programma
    s.Target <- p
    if s.FindSplitup() then
        s.Splitup
    else
        [|0.0; 0.0 |]

let getIthErr2 i =
    let s1 = findSplitup2 i
    progErr s1 [| p1; p2 |] i otherMeasures2

let avgErr2 = 
    let sum = Seq.init 12 (fun i -> i) |> Seq.fold (fun (s:float) (i:int) -> s + fst (getIthErr2 i)) 0.0
    let c = float 12
    sum / c






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
