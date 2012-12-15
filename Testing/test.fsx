#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#r @"Energon.Measuring.dll"
#r @"Energon.Storage.dll"
#r "System.Data.Linq.dll"
#r "System.Linq.dll"

#r "FSharp.PowerPack.Linq.dll"
#r "FSharp.Data.TypeProviders.dll"

#r "System.Data.DataSetExtensions.dll"
#r "System.Core.dll"
#r "SQLExpress.dll"

open Energon.Storage
open System
open Microsoft.FSharp.Data.TypeProviders
open System.Data.Linq.SqlClient
open System.Linq
open Microsoft.FSharp.Linq
open System.Data.Linq
open Energon.Measuring
open Energon.Measuring.Database
open System.Text
open System.Collections.Generic
open SQLExpress

type HandySensor () as self =
    let mutable sensorID = 0
    let mutable sensorName = ""
    let mutable measurements:seq<float> = Seq.empty<float>
    let mutable average = 0.
    let mutable stddev = 0.
    member x.SensorID 
        with get() = sensorID
        and set(v) = sensorID <- v
    member x.SensoName
        with get() = sensorName
        and set(v) = sensorName <- v
    member x.Measurements 
        with get() = measurements
        and set(v) = measurements <- v
    member x.Average
        with get() = average
        and set(v) = average <- v
    member x.StandardDeviation
        with get() = stddev
        and set(v) = stddev <- v

let server = "HPLAB\SQLEXPRESS"
let database = "Measure"

let getConStr = 
        //let conStr = System.String.Format("server='{0}';database='{1}';User Id='{2}';password='{3}';", server, database, user, password) in
        let conStr = System.String.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", server, database) in
        conStr
let GetLinqContext = 
        let context = new SQLExpress.Measure(getConStr)
        if (context.DatabaseExists() = false) then
             context.CreateDatabase()
        context
let db = 
        let context = GetLinqContext
        context.Connection.Open()
        context
        
let exp ids = Seq.collect (fun (i:int) -> db.Experiments.Where(fun (e:Experiments) -> e.Id = i)) ids
let cases exp = Seq.collect (fun (e:Experiments) -> db.ExperimentCases.Where(fun (c:ExperimentCases) -> c.Experiment_id = e.Id) ) exp

(*

let roba = cases (exp [| 127 |])
roba
db.AvgMeasures.Where(fun (a:AvgMeasures) -> a.Experiment_id = 158)


let sensors = db.SensorClasses.Where(fun _ -> true) 
Seq.iter (fun (c:SensorClasses) -> System.Console.WriteLine("{0}:{1}", c.Id, c.SensorName)) sensors
Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("simple") || e.Name.StartsWith("rand")))
Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("simple") || e.Name.StartsWith("rand")))

// all experiments
Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> true))

db.AvgMeasures.Where(fun (a:AvgMeasures) -> (a.Sensor_class_id=87 || a.Sensor_class_id=22) && a.Experiment_id = 141).OrderBy(fun x -> x.Sensor_class_id)

db.AvgMeasures.Where(fun (a:AvgMeasures) -> (a.Sensor_class_id=87 || a.Sensor_class_id=94) && a.Experiment_id = 191)

*)

let getAveragesForCase case =
    db.AvgMeasures.Where(fun (a:AvgMeasures) -> (a.Experiment_case_id = case)).OrderBy(fun x -> x.Sensor_class_id)

let caseid = 3409

let averagePhi = 0.7
let WsensorID = 94
let V = 220.0

let otherWsensor = 22
//let averagePhi = 1.0
//let WsensorID = 22
//let V = 1.0

let caseToAverages caseid =
    let avgs = getAveragesForCase caseid
    let w = avgs.Where(fun (a:AvgMeasures) -> a.Sensor_class_id=WsensorID).First()
    let s = avgs.Where(fun (a:AvgMeasures) -> a.Sensor_class_id=87).First()
    let j = w.Average.Value * s.Average.Value * averagePhi * V
    let filtered = avgs.Where(fun (a:AvgMeasures) -> a.Sensor_class_id <> WsensorID)
    let response = Seq.map (fun (a:AvgMeasures) -> a.Average.Value) filtered
    Seq.append response [| j |]

let caseToAveragesOnlyJ caseid =
    let avgs = getAveragesForCase caseid
    let w = avgs.Where(fun (a:AvgMeasures) -> a.Sensor_class_id=WsensorID).First()
    let s = avgs.Where(fun (a:AvgMeasures) -> a.Sensor_class_id=87).First()
    let j = w.Average.Value * s.Average.Value * averagePhi * V
    j

let caseToAveragesNoJ caseid =
    let avgs = getAveragesForCase caseid
    let filtered = avgs.Where(fun (a:AvgMeasures) -> a.Sensor_class_id <> WsensorID && a.Sensor_class_id <> otherWsensor)
    let response = Seq.map (fun (a:AvgMeasures) -> a.Average.Value) filtered
    response

// print sensornames
//let caseid = 3198
//Seq.iter (fun (c:AvgMeasures) -> System.Console.WriteLine("{0}:{1}:{2}",c.Sensor_class_id, c.SensorName, c.Average.Value)) ( getAveragesForCase caseid)

// print values
//Seq.iter (fun (c:float) -> System.Console.WriteLine("{0}", c)) (caseToAverages caseid)
//Seq.iter (fun (c:float) -> System.Console.WriteLine("{0}", c)) (caseToAveragesNoJ caseid)

#r "GlpkProxy.dll"
open GlpkProxy

let getProgAverages (list:seq<int>) =
    Seq.fold (fun (state:seq<float>) (id:int) -> Seq.append state (caseToAveragesNoJ id)) Seq.empty<float> list
let getTestBedAverages (list:seq<int array>) =
    Seq.map (fun (l:int array) -> getProgAverages l) list    
let buildTestBed (list:seq<seq<float>>) =
    let programFromAverages (l:seq<float>) =
        let p = new Program()
        p.Measures <- l.ToArray()
        p
    let programSeq = Seq.map (fun (l:seq<float>) -> programFromAverages l) list
    programSeq.ToArray()


let casesRandMemAccess =  [| 3409; 3415; 3419 |]
let casesSimpleINT = [| 3410; 3416; 3420 |]
let casesSimpleFPU = [| 3411; 3417; 3421 |]
let casesTestbed = [| casesRandMemAccess; casesSimpleINT; casesSimpleFPU |]

let casesPi = [| 3412; 3418; 3422 |]

let casesHeap4M = [| 3198 ; 3293; 3363 |]
let casesHeap16M = [| 3199; 3294; 3364 |]
let casesHeap64M = [| 3200; 3295; 3365 |]
let casesHeap256M = [| 3201; 3296; 3366 |]

let s = new SplitupFinder()
let testbedAvgs = getTestBedAverages casesTestbed
s.Testbed <- (buildTestBed testbedAvgs)

let p = new Program()
p.Measures <- (getProgAverages casesHeap256M).ToArray()
//p.Measures <- (getProgAverages casesRandMemAccess).ToArray()
s.Target <- p

//s.Testbed
//p.Measures

s.FindSplitup()
s.Splitup

let randMemAccessJ = Seq.map caseToAveragesOnlyJ casesRandMemAccess
let simpleINTJ = Seq.map caseToAveragesOnlyJ casesSimpleINT
let simpleFPUJ = Seq.map caseToAveragesOnlyJ casesSimpleFPU
let PiJ = Seq.map caseToAveragesOnlyJ casesHeap256M

randMemAccessJ

let splitupWithoutJ = s.Splitup
let splitupWithJ = s.Splitup
splitupWithoutJ
splitupWithJ
let prev0 = Seq.map (fun (p:float) -> p * s.Splitup.[0]) randMemAccessJ
let prev1 = Seq.map (fun (p:float) -> p * s.Splitup.[1]) simpleINTJ
let prev2 = Seq.map (fun (p:float) -> p * s.Splitup.[2]) simpleFPUJ
PiJ
prev0
prev1
prev2




























Seq.iter (fun (c:AvgMeasures) -> System.Console.WriteLine("{0}:{1}", c.SensorName, c.Average.Value)) (caseToAverages caseid)


let runsOfCase id = db.ExperimentRuns.Where(fun (r:ExperimentRuns) -> r.Experiment_case_id=id)
let sensorsOfRun id = db.Sensors.Where(fun (s:Sensors) -> s.Experiment_run_id = id)
let measuresOfSensor id = db.Measurements.Where(fun (m:Measurements) -> m.Sensor_id = id)

let handySensorFromSensorId id = 
    let handy = new HandySensor()
    handy.SensorID <- id
    let sensor = db.Sensors.Where(fun (s:Sensors) -> s.Id = id).First()
    let sensorClass = db.SensorClasses.Where(fun (sc:SensorClasses) -> sc.Id = sensor.Sensor_class_id).First()
    handy.SensorID <- sensorClass.Id
    handy.SensoName <- sensorClass.SensorName
    let measures = measuresOfSensor id
    handy.Measurements <- Seq.map (fun (m:Measurements) -> m.Value) measures
    handy.Average <- Seq.average (handy.Measurements)
    handy.StandardDeviation <- Seq.fold(fun state v -> state + (v - handy.Average) * (v - handy.Average)) 0. handy.Measurements
    if (handy.Measurements.Count() > 1) then
        handy.StandardDeviation <- handy.StandardDeviation / float(handy.Measurements.Count()-1)
    else
        if (handy.Measurements.Count() > 0) then
            handy.StandardDeviation <- handy.StandardDeviation / float(handy.Measurements.Count())
    handy  

let handyFromRun runid = 
    Seq.map (fun (s:Sensors) -> handySensorFromSensorId s.Id) ( sensorsOfRun runid)

let handyFromCase caseid = 
    Seq.map (fun (r:ExperimentRuns) -> handyFromRun r.Id) (runsOfCase caseid)

let averages caseid = 
    seq {
        let runs = handyFromCase caseid
        let sensors = runs.First()
        for i in 0..(sensors.Count()-1) do
            let handy = new HandySensor()
            let s = sensors.ElementAt i
            handy.SensoName <- s.SensoName
            handy.SensorID <- s.SensorID
            handy.Average <- Seq.averageBy (fun (h:seq<HandySensor>) -> (h.ElementAt i).Average) runs
            yield handy
    }

let ids = [| 40 |]
let e = exp ids
let c = cases e
let c1 = c.ElementAt 1
let avgs = averages c1.Id

handyFromCase c1.Id
let runs = runsOfCase c1.Id
let sens = sensorsOfRun (runs.First().Id)
let s = sens.ElementAt 0
s.Sensor_class_id
let h = handySensorFromSensorId s.Id
measuresOfSensor s.Id

let simpleINT = [| 133,"_linux"; 128,"_linux64"; 140,"_arm"; 149,"_win"; 159,"_vostro";|]
let simpleFPU = [| 134,"_linux"; 129,"_linux64"; 141,"_arm"; 150,"_win"; 160,"_vostro";|]
let randMemAccess = [| 131,"_linux"; 127,"_linux64"; 139,"_arm"; (*150,"_win";*) 158,"_vostro";|]

let merges = [| 42,"_linux"; 124,"_linux64"; 137,"_arm"; 147,"_win"; 156,"_vostro";|]

let filterArg a id = Seq.filter (fun (e:ExperimentCases) -> e.Args.Equals(a)) (cases (exp id))
let c = cases (exp [|140|])
Seq.iter (fun (c:ExperimentCases) -> System.Console.WriteLine("{0}", c.Args)) c

let size = "1073741824;"
let tmp = filterArg size (Seq.singleton 128)
let avgs = averages (tmp.First().Id)
avgs
let export (name:string) arg (list:seq<int*string>) = 
    let listlist = list |> Seq.map (fun (id,post) -> 
            let resourcename = System.String.Format("{0}{1}", name, post)
            let filtCases = filterArg arg id
            handyFromCase (filtCases.First().Id)
        )
    filterArg arg 
