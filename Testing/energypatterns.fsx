#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#I @"C:\progetti\Energon\bin\Debug"
#r @"Energon.Measuring.dll"
open Energon.Measuring

#r @"Energon.Storage.dll"

#r @"C:\Program Files (x86)\Microsoft SQL Server Compact Edition\v4.0\Desktop\System.Data.SqlServerCe.dll"

#r "Energon.SQLCE.dll"

#r "SQLExpress.dll"

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
let dbfile = @"C:\Users\root\Desktop\Energon\Measurements.sdf"

#r @"Energon.Phidgets.dll"
open Phidgets30A

openPhidgets() 

let phidgetAmmeter = new AmmeterSensor("PhidgetsVA", 0, ifkit, 10.)

//let sensors = [| extechWatt :> GenericSensor ; new RemoteSensor("test", DataType.Unknown) :> GenericSensor; new RemoteSensor("test", DataType.Unknown) :> GenericSensor|]
//let sensors = [| new RemoteSensor("cpu-cycles", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-references", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-instructions", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]
//let sensors = [| extechWatt :> GenericSensor; new RemoteSensor("cpu-cycles", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-references", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-instructions", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]
let sensors = [| 
                phidgetAmmeter :> GenericSensor; 
                new RemoteSensor("instructions", DataType.Unknown) :> GenericSensor; 
                new RemoteSensor("threads", DataType.Unknown) :> GenericSensor; 
                new RemoteSensor("avg_energy", DataType.Unknown) :> GenericSensor; 
                new RemoteSensor("energy_per_instruction", DataType.Unknown) :> GenericSensor; 
                new RemoteSensor("duration", DataType.Unknown) :> GenericSensor; 
                new RemoteSensor("iterations", DataType.Unknown) :> GenericSensor
                |]

let sensors = [| new RemoteSensor("cpu-cycles", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-references", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-instructions", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]

let sensors = [| phidgetAmmeter :> GenericSensor; new RemoteSensor("faults", DataType.Unknown) :> GenericSensor; new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]
let sensors = [| new RemoteSensor("faults", DataType.Unknown) :> GenericSensor; new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]

// declare a remote sensor

//let sensors = [|extechAmp :> GenericSensor; extechWatt :> GenericSensor; extechPF :> GenericSensor; extechV :> GenericSensor; r1 :> GenericSensor |]
//let sensors = [| r1 :> GenericSensor |]

// DEBUG

// iozone
let e = new Experiment("iozone_ubuntu32kvm_load", sensors, 0, [| "prog"; "size" |], [||], fun _ -> ())

// ARM
let createExp (prog:string) (system:string) =
  new Experiment(System.String.Format("{0}_{1}", prog, system), sensors, 0, [| "size" |], [||], fun _ -> ())

let system = "ubuntu32kvm_load"
let e = createExp "quick" system
let e = createExp "merges" system
let e = createExp "randMemAccess" system
let e = createExp "simpleINT" system
let e = createExp "simpleFPU" system

let e = createExp "pi" system

let e = createExp "heap" system


//let e = new Experiment("test_linux", sensors, 0, [| "size" |], [||], fun _ -> ())

// db helper
let server = "HPLAB\SQLEXPRESS"
let dbname = "Measure"
let saver = new Energon.Storage.ExperimentRuntimeSaverExpress(e, server, dbname )


open Energon.Measuring.Remote

// the helper makes easy to handle remote loads and remote sensors
let helper = new RemoteExperimentHelper(e)

helper.Start()

helper.Stop()


