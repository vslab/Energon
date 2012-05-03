#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#r @"Energon.Measuring.dll"
open Energon.Measuring

(*
// a couple of sensors...
let proc = new PerfCounter("Process", "% Processor Time", 1.)
proc.Instance <- "FSI"
let proc2 = new PerfCounter("Process", "% User Time", 1.)
proc2.Instance <- "FSI"
let proc3 = new PerfCounter("IPv4", "Datagrams/sec", 1.)

// something that uses CPU/mem
let rec fib(n) = 
    match n with
    | 0 -> 0
    | 1 -> 1
    | x -> fib (x-1) + fib (x-2)


let load(s:seq<obj>) = 
    let n_o = Seq.head s
    let n = n_o :?> int
    printfn "fib(%d)=%d" (n) (fib(n))


//define an exepriment
let args = seq {
        for i in 40..42 -> 
            seq {
                yield i :> obj
            }
    }

*)

(*
let e = new Experiment("fibonacci 40..42", [| proc; proc2; proc3 |], 3, [|"n"|], args, load)
e.Run(true)
e.Results
*)

// ****************** SqlCE ***************************
#r @"Energon.Storage.dll"
//#r @"C:\Users\Davide\Desktop\Projects\Energon\Storage\bin\Debug\Energon.Measurement.dll"
//#r @"C:\Users\Davide\Desktop\Projects\Energon\SqlCompactDb\Measurement\bin\Debug\Energon.Measurement.dll"


#r @"C:\Program Files (x86)\Microsoft SQL Server Compact Edition\v4.0\Desktop\System.Data.SqlServerCe.dll"

#r "Energon.SQLCE.dll"

#r "System.Data.Linq.dll"
#r "System.Linq.dll"

#r "FSharp.PowerPack.Linq.dll"
#r "FSharp.Data.TypeProviders.dll"

#r "System.Data.DataSetExtensions.dll"
#r "System.Core.dll"

open System
open Microsoft.FSharp.Data.TypeProviders
open System.Data.Linq.SqlClient
open System.Linq
open Microsoft.FSharp.Linq
open System.Data.Linq
open System.Data.SqlServerCe;
open Energon.Measuring
open System.Text
open System.Data.DataSetExtensions
let dbfile = @"C:\Users\root\Desktop\Energon\test.sdf"

// ------ remote experiment
open Energon.Measuring.Remote

#r "Energon.Extech.dll"

open Energon.Extech380803

//let extechAmp = new Energon.Extech380803.Extech380803Sensor("extechAmp", DataType.Ampere, 1.0)
let extechWatt = new Energon.Extech380803.Extech380803Sensor("extechWatt", DataType.Watt, 1.0)
//let extechPF = new Energon.Extech380803.Extech380803Sensor("extechPF", DataType.PowerFactor, 1.0)
//let extechV = new Energon.Extech380803.Extech380803Sensor("extechV", DataType.Volt, 1.0)
(*
extechPF.Close()
extechAmp.Close()
extechWatt.Close()
extechV.Close()
*)
// declare a remote sensor
let r1 = new RemoteSensor("completionTime", DataType.Unknown)

let sensors = [| r1 :> GenericSensor ; extechWatt :> GenericSensor |]
//let sensors = [|extechAmp :> GenericSensor; extechWatt :> GenericSensor; extechPF :> GenericSensor; extechV :> GenericSensor; r1 :> GenericSensor |]
//let sensors = [| r1 :> GenericSensor |]

// declare an experiment
let e = new Experiment("saxpy_test4", sensors, 0, [| "mode"; "vector_size"; "samples"; "use_float_4"; "n_thread_host"; "n_device"; "d0_size"; "d0_mode_in"; "d0_mode_out"; "d1_size"; "d1_mode_in"; "d1_mode_out"; "d2_size"; "d2_mode_in"; "d2_mode_out" |], [||], fun _ -> ())
// db helper
let saver = new Energon.Storage.ExperimentRuntimeSaver(e, dbfile)

saver.OpenConnection()

// the helper makes easy to handle remote loads and remote sensors
let helper = new RemoteExperimentHelper(e)
helper.Start()


helper.Stop()



// a remote process starter (this should be run on the remote machine)
let remote = new RemoteSensorHelper("127.0.0.1")
remote.case([| "2"; "1"; "1";"1";"1";"1";"1";"1";"1";"1";"1";"1";"1";"1";"1"; |]) // a new experiment case with its params

remote.start() // experiment starting
// experiment run, I have to send remote sensors values, in the same order as we declared the sensors
remote.stop([| "0.1" |])  



e.Cases.Count
let case = e.Cases.Item 0
case
let run = case.Runs.Item 0
run


let start() =
    printf "start\n"
let stop() =
    printf "stop\n"

let w = new webListener(start, stop)
w.start()
w.stop()

//let db = new Energon.Measurement.Measurements(dbfile)
let db = saver.LinqContext
db.Experiments
let measurements = db.Measurements1
let sel = measurements.First(fun (m:Energon.SQLCE.Measurements1) -> m.Sensor_id = 205)
sel
measurements.DeleteOnSubmit(sel)
db.SubmitChanges()

let exp = db.Experiments.Where(fun (x:Experiments) -> x.Name.StartsWith("fibonacci")).First()
printf "%s\n" exp.Name
let cases = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = exp.Id)
let printCase (c:ExperimentCases) =
    printf "%s=%s" exp.ArgNames c.Args
    let runs = db.ExperimentRuns.Where(fun (r:ExperimentRuns) -> r.Experiment_case_id = c.Id)

let db2 = Energon.CompactSQL.GetLinqContext dbfile
db

#quit;;


//CompactSQL
//Energon.CompactSQL.SaveExperiment e dbfile
(*
// example getting data from db
open Energon.SQLCE
open Energon.CompactSQL
let db = Energon.CompactSQL.GetLinqContext dbfile
let exp = db.Experiments
let expCases = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = exp.First().Id )
expCases
let lastExpCase = expCases.Where(fun (x:ExperimentCases) -> x.Id = 3 ).First()
let runs = db.ExperimentRuns.Where(fun (x:ExperimentRuns) -> x.Experiment_case_id = lastExpCase.Id )
runs
let lastRun = runs.Where(fun (x:ExperimentRuns) -> x.Id = 9).First()
let sensors = db.Sensors.Where(fun (x:Sensors) -> x.Experiment_run_id = lastRun.Id)
let lastSensor = sensors.Where(fun (x:Sensors) -> x.Sensor_class_id = 3).First()
let measurements = db.Measurements1.Where(fun (x:Measurements1) -> x.Sensor_id = lastSensor.Id )
measurements

let exp = new Experiment("fibonacci 40..42", [| proc; proc2; proc3 |], 3, [|"n"|], args, load)
let saver = new Energon.Storage.ExperimentRuntimeSaver(exp, dbfile)

exp.Run(true)

#quit;;
*)




exp.Results
exp.MeansAndStdDev

// ****************** Analysis ***************************
#r @"C:\Users\Davide\Desktop\Projects\Energon\Analysis\bin\Debug\Energon.Analysis.dll"

Tools.expCorrMatrix e

//RealtimeTools
Tools.MovingAverageReading 3 exp.Results.[proc].[1]
exp.Results.[proc].[1]
Tools.meanAndStdDevReading exp.Results.[proc].[1]

// ****************** Draw ***************************


#r @"C:\Users\Davide\Desktop\Projects\Energon\Charts\bin\Debug\Energon.Charts.dll"
open Energon.Charts

showExperimentMeansAndStdDev exp
showExperimentCaseMeansAndStdDevDetail exp

showExperimentCase exp

showSensorListRealtime [proc;proc2]

let stddevEvents = RealtimeTools.movingAverageReadingEvt 5 proc.ObservableReadings
showSensorRealtime proc
showFloatRealtime stddevEvents

proc.Start()
proc2.Start()

proc.Stop()
proc2.Stop()

// ****************** Storage ***************************

#r @"C:\Users\Davide\Desktop\Projects\Energon\Storage\bin\Debug\Energon.Storage.dll"
open Energon.Measurement


//Energon.Storage.saveExperimentResultsToCompactCSV(@"C:\Users\Davide\Downloads\test.csv",exp,1000.)



(*

// using Extech
open Energon.Extech380803
// using phidgets
#r @"C:\Users\root\Desktop\energon\energon\Phidgets\Phidgets\bin\Debug\Phidget21.NET.dll" 
#r @"C:\Users\Davide\Desktop\Projects\energon\energon\EnergonFramework\Measuring\bin\Debug\Energon.Extech.dll"
#r @"C:\Users\Davide\Desktop\Projects\energon\energon\EnergonFramework\Testing\bin\Debug\Energon.Phidgets.dll"
open Energon.Phidgets
open Phidgets30A

openPhidgets() 


let addCounter cat name =
    let p = new PerfCounter( cat, name, 1., 64)
    p

let addCounterInst cat name inst =
    let p = new PerfCounter( cat, name, 1., 64)
    p.Instance <- inst
    p

let catNameList = [| ("Memory", "Page Faults/sec") |]
let catNameInstList = [| ("PhysicalDisk", "% Disk Time", "_Total"); ("Process", "% Processor Time", "FSI"); ("Process", "Page Faults/sec", "FSI"); ("Process", "Thread Count", "FSI") |]

let counters = Array.map (fun v -> v :> GenericSensor) (Array.append (Array.map (fun (c,n) -> addCounter c n ) catNameList) (Array.map (fun (c,n,i) -> addCounterInst c n i) catNameInstList))

let applyAll f l =
    l |> Array.iter f

// starts sensors
applyAll (fun (s:GenericSensor) -> s.Start()) counters


// write a CSV file
open System
open System.IO 
open System.Text
  
let out = File.CreateText(@"data.csv") 

let sbTitle = new StringBuilder()
Array.iter (fun (s:GenericSensor) -> sbTitle.AppendFormat("{0};", s.Name) |> ignore) counters
out.WriteLine(sbTitle.ToString())

let run = ref true
let getData = 
    async {
        while !run do
            let sb = new StringBuilder() 
            let print (s:GenericSensor) =
                sb.AppendFormat("{0};", s.nextValue().Value) |> ignore
            Array.iter print counters
            out.WriteLine (sb.ToString())
    }
Async.Start(getData)
run := false

out.Close()

Directory.GetCurrentDirectory()


// stop sensors
applyAll (fun (s:GenericSensor) -> s.Stop())
applyAll (fun (s:GenericSensor) -> s.Close())


// graphig
#r @"C:\Users\Davide\Desktop\Projects\energon\energon\EnergonFramework\Testing\MSDN.FSharpChart.dll"
//open System.Windows.Forms.DataVisualization.Charting
open MSDN.FSharp.Charting
open MSDN.FSharp.Charting.ChartTypes

let showReadingsLine (d:Reading[]) =
    d |> Array.map (fun r -> (r.Timestamp.ToLongTimeString(), r.Value) ) |> FSharpChart.FastLine |> FSharpChart.Create

open System.Windows.Forms.DataVisualization.Charting

let showSensorData (s:GenericSensor) =
    FSharpChart.FastLine(Array.map (fun (r:Reading) -> (r.Timestamp, r.Value)) (s.Results), Name=s.Name) |> FSharpChart.WithLegend ( InsideArea = false, Alignment = StringAlignment.Center, Docking = Docking.Top) |> FSharpChart.Create

showSensorData proc

Observable.map (fun (r:Reading) -> (r.Timestamp, r.Value)) proc.ObservableReadings |> FSharpChart.FastLine |> FSharpChart.Create
proc.Start()
proc.Stop()
proc.Results.Length

let c = [| 1 ; 2 ;1 |] |> FSharpChart.FastLine |> FSharpChart.Create




//let l1 = Seq.toList (proc.ToSequence())
//let l2 = Seq.toList (proc2.ToSequence())
let l1 = proc.ToList()
let l2 = proc2.ToList()

showReadings l1
showReadings l2

showSensorData proc

showCombinedSensorsData [| proc;proc2 |]
// TODO: il disegnare consuma i dati, una volta disegnati sono persi


proc2.Stop()
proc.Stop()

let t1 = proc.ToFloatSequence() 
t1 |> FSharpChart.Line |> FSharpChart.Create
let ary = Seq.toList t1
ary |> FSharpChart.Line |> FSharpChart.Create
Seq.iter (fun x -> Console.WriteLine(x.ToString())) ary

FSharpChart.Combine [ proc.ToFloatSequence() |> Seq.toList |> FSharpChart.Line; mem.ToFloatSequence() |> Seq.toList |> FSharpChart.Line ] |> FSharpChart.Create

#quit;








open System

open System.Collections.Generic


let proc = new PerfCounter("Process", "% Processor Time", 1., 64)
proc.Instance <- "FSI"

let proc2 = new PerfCounter("Process", "% User Time", 1., 64)
proc2.Instance <- "FSI"

let sensors =  [| proc :> GenericSensor; proc2 :> GenericSensor |]


let mutable running = false
let results = new Dictionary<GenericSensor, Reading list>()
let getSensorData (s:GenericSensor)=
    async {
        printfn "starting acquiring data for sensor %s" (s.Name.ToString())
        let dataList = s.ToList
        if results.ContainsKey(s) then
            results.[s] <- dataList
        else
            results.Add(s, dataList)
        printfn "done acquiring data for sensor %s" (s.Name.ToString())
    }

let Start() =
    Seq.iter (fun (s:GenericSensor) -> s.Reset()) sensors
    Seq.iter (fun (s:GenericSensor) -> s.Start()) sensors
    running <- true
    Seq.iter (fun (s:GenericSensor) -> Async.Start(getSensorData s) ) sensors

let Stop() =
    running <- false
    Seq.iter (fun (s:GenericSensor) -> s.Stop()) sensors
        
let CloseSensors() =
    if running then Stop()
    Seq.iter (fun (s:GenericSensor) -> s.Close()) sensors


Start()


let rec fib(n) = 
    match n with
    | 0 -> 0
    | 1 -> 1
    | x -> fib (x-1) + fib (x-2)

let load =
    async {
        //Console.WriteLine("starting...")
        printfn "starting..." 
        System.Threading.Thread.Sleep(2000)
        printfn "%d" (fib 40)
        System.Threading.Thread.Sleep(2000)
        printfn "%d" (fib 40)
        System.Threading.Thread.Sleep(2000)
        printfn "...finished"
    }
Async.Start(load)


Stop()
results

let testLoad t =
    async {
        proc.Start()
        printfn "starting..."
        System.Threading.Thread.Sleep(t*1000)    
        printfn "...done"
        proc.Stop()
    }
let getData =
    async {
        List.iter (fun (r:Reading) -> printfn "%f" r.Value ) proc.ToList
    }

let getData2 =
    async {
        printfn "%f" (proc.nextValue().Value.Value)
        printfn "%f" (proc.nextValue().Value.Value)
    }
Async.Start(testLoad 4)
Async.Start(getData)
Async.Start(getData2)

proc.Reset()
proc.Start()
proc.Stop()
proc.ToSequence()
proc.ToList
proc.nextValue()

#quit;

let l1 = 
    seq {
        System.Threading.Thread.Sleep(1000)
        if false then yield 0
    }
l1

*)

let a = 1
