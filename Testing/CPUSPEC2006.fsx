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

let resourceSimilarity (a:float[]) (b:float[]) =
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

let R1 = [| 1.0; 1.0 |]
let R2 = [| 1.0; 2.0 |]
let R3 = [| 0.0; 2.0 |]

let resources = [| R1; R2; R3 |]

let findMax (resources:float[][]) = 
  let mutable maxAngle = 0.0
  let mutable besti = 0
  let mutable bestj = 0
  let mutable bestprogi = 0
  let mutable bestprogj = 0
  let l = resources.GetLength(0)
  for i in 0..(l - 2) do
    for j in 0..(l - 1) do
      let ang, thisi, thisj = resourceSimilarity (resources.[i]) (resources.[j])
      if ang > maxAngle then
        maxAngle <- ang
        besti <- i
        bestj <- j
        bestprogi <- thisi
        bestprogj <- thisj
  (maxAngle, besti, bestj, bestprogi, bestprogj)







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

let limite = 10
let misure_all = db.Measures.Where(fun (m:Measures) -> m.ExperimentID >= 319 && m.ExperimentID < (319 + limite*12)).OrderBy(fun m -> m.ExperimentID) |> Seq.toList 
let misure_f = misure_all|> Seq.map (fun (m:Measures) -> (m.ExperimentID - 319)/12, m.AverageValue.Value)  |> Seq.groupBy fst |> Seq.map (fun (_,x) -> Seq.map snd x |> Seq.toList)









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
