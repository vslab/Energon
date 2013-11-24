#load "DataType.fs"
open Sensor
#load "Reading.fs"
open Sensor
#load "Sensor.fs"
open Sensor
#load "Extech380803.fs"
open Sensor



#r @"C:\Users\Davide\Desktop\Projects\energon\energon\EnergonFramework\Phidgets\Phidget21.NET.dll" 
#load "Phidgets30A.fs"

open Phidgets30A

openPhidgets() 

let s1 = new Extech380803Sensor("A", DataType.Ampere, 1.)
let s2 = new Extech380803Sensor("PF", DataType.PowerFactor, 1.)
let p = new AmmeterSensor("VA", 0, ifkit, 1.)


let sensors = [| s1 :> GenericSensor ; s2 :> GenericSensor; p :> GenericSensor|]




#load "PerformanceCounter.fs"
open PerformanceCounter


let perf1 = new PerfCounter("Processor", "% Processor Time", 1.)
perf1.Instance <- "_Total"
perf1.Start()

perf1.Stop()

let run = ref true
let foo2 =
    async {
        while !run do
            let v = perf.nextValue()
            printfn "------> %s:%d %f" (v.Timestamp.ToLongTimeString()) (v.Timestamp.Millisecond) (v.Value)
            //System.Threading.Thread.Sleep(1)
    }
Async.Start(foo2)
run := false


let values(amount) = 
  seq { 
    while true do 
      let e = perf.nextValue().Value
      System.Threading.Thread.Sleep(110) 
      yield e 
  } |> Seq.take amount 

let values2(amount) =
    perf.ToSequence() |> Seq.take amount

perf.ToSequence()

values2(10) |> Seq.iter (fun v -> (printfn " %d %f" (v.Timestamp.Millisecond) (float(v.Value)))) 

perf.Stop() 




//let counters = [| |]

let addCounter cat name =
    let p = new PerfCounter( cat, name, 1.)
    p
    //l |> Array.append [| p |]

let addCounterInst cat name inst =
    let p = new PerfCounter( cat, name, 1.)
    p.Instance <- inst
    p
    //l |> Array.append [| p |]

let catNameList = [| ("Memory", "Page Faults/sec") |]
let catNameInstList = [| ("PhysicalDisk", "% Disk Time", "_Total"); ("Process", "% Processor Time", "FSI"); ("Process", "Page Faults/sec", "FSI"); ("Process", "Thread Count", "FSI") |]

let counters = Array.map (fun v -> v :> GenericSensor) (Array.append (Array.map (fun (c,n) -> addCounter c n ) catNameList) (Array.map (fun (c,n,i) -> addCounterInst c n i) catNameInstList))

let applyAll f l =
    l |> Array.iter f

applyAll (fun (s:GenericSensor) -> s.Start()) counters

open System
open System.IO 
open System.Text
  
let out = File.CreateText(@"data.csv") 

let sbTitle = new StringBuilder()
Array.iter (fun (s:GenericSensor) -> sbTitle.AppendFormat("{0};", s.Name) |> ignore) counters
out.WriteLine(sbTitle.ToString())

let run = ref true
//printfn "A pro %f PF pro %f A cheap %f" (s1.nextValue().Value.Value) (s2.nextValue().Value.Value) (p.nextValue().Value.Value)
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

applyAll (fun (s:GenericSensor) -> s.Stop())
applyAll (fun (s:GenericSensor) -> s.Close())



let p2 = new PerfCounter("Memory", "Page Faults/sec", 1.)
p2.Start()
p2.Stop()
p2.Close()

#quit;







