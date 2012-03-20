#r @"C:\Users\Davide\Desktop\Projects\Energon\Measuring\bin\Debug\Energon.Measuring.dll"
open Energon.Measuring

// a couple of sensors...
let proc = new PerfCounter("Process", "% Processor Time", 1.)
proc.Instance <- "FSI"
let proc2 = new PerfCounter("Process", "% User Time", 1.)
proc2.Instance <- "FSI"

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

let e = new Experiment( [| proc; proc2 |], 3, [|"n"|], args, load)
e.Run()
e.Results


let exp = new ExperimentCase([| proc; proc2 |], 3, [| (42 :> obj) |], load)
exp.Run()





exp.Results
exp.MeansAndStdDev

// ****************** Analysis ***************************
#r @"C:\Users\Davide\Desktop\Projects\Energon\Analysis\bin\Debug\Energon.Analysis.dll"

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
open Energon.Storage

saveExperimentResultsToCompactCSV(@"C:\Users\Davide\Downloads\test.csv",exp,1000.)



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

(*
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
