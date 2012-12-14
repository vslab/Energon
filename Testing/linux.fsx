#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#I @"C:\progetti\Energon\bin\Debug"
#r @"Energon.Measuring.dll"
open Energon.Measuring


#r @"Energon.Storage.dll"
//#r @"C:\Users\Davide\Desktop\Projects\Energon\Storage\bin\Debug\Energon.Measurement.dll"
//#r @"C:\Users\Davide\Desktop\Projects\Energon\SqlCompactDb\Measurement\bin\Debug\Energon.Measurement.dll"


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

// ------ remote experiment
open Energon.Measuring.Remote

#r "Energon.Extech.dll"

open Energon.Extech380803

//let extechAmp = new Energon.Extech380803.Extech380803Sensor("extechAmp", DataType.Ampere, 1.0)
let extechWatt = new Energon.Extech380803.Extech380803Sensor("extechWatt", DataType.Watt, 1.0)
//let extechPF = new Energon.Extech380803.Extech380803Sensor("extechPF", DataType.PowerFactor, 1.0)
//let extechV = new Energon.Extech380803.Extech380803Sensor("extechV", DataType.Volt, 1.0)
//extechWatt.Start()
//extechWatt.CurrValue()
//extechWatt.Stop()

//let sensors = [| extechWatt :> GenericSensor ; new RemoteSensor("test", DataType.Unknown) :> GenericSensor; new RemoteSensor("test", DataType.Unknown) :> GenericSensor|]
//let sensors = [| new RemoteSensor("cpu-cycles", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-references", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-instructions", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]
let sensors = [| extechWatt :> GenericSensor; new RemoteSensor("cpu-cycles", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-references", DataType.Unknown) :> GenericSensor; new RemoteSensor("cache-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-instructions", DataType.Unknown) :> GenericSensor; new RemoteSensor("branch-misses", DataType.Unknown) :> GenericSensor; new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]

// declare a remote sensor

//let sensors = [|extechAmp :> GenericSensor; extechWatt :> GenericSensor; extechPF :> GenericSensor; extechV :> GenericSensor; r1 :> GenericSensor |]
//let sensors = [| r1 :> GenericSensor |]

// DEBUG

// CPU SPEC
let e = new Experiment("401.bzip2_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("435.gromacs_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("445.gobmk_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("444.namd_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("410.bwaves_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("454.calculix_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("429.mcf_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("464.h264ref_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("458.sjeng_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("471.omnetpp_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("434.zeusmp_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("453.povray_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("436.cactusADM_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("465.tonto_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("483.xalancbmk_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("462.libquantum_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("470.lbm_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("437.leslie3d_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("433.milc_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("450.soplex_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("403.gcc_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("482.sphinx3_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("456.hmmer_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("416.gamess_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("999.specrand_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("447.dealII_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("473.astar_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("400.perlbench_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("481.wrf_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("459.gemsFDTD_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("998.specrand_linux64", sensors, 0, [| "size" |], [||], fun _ -> ())

// ARM
let e = new Experiment("quick_arm", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("merges_arm", sensors, 0, [| "size" |], [||], fun _ -> ())
let e = new Experiment("heap_arm", sensors, 0, [| "size" |], [||], fun _ -> ())

//TODO
// win
// randMemAccess


let e = new Experiment("randMemAccess_win", sensors, 0, [| "size" |], [||], fun _ -> ())

let e = new Experiment("simpleINT_win", sensors, 0, [| "size" |], [||], fun _ -> ())

let e = new Experiment("simpleFPU_win", sensors, 0, [| "size" |], [||], fun _ -> ())

// db helper
let server = "HPLAB\SQLEXPRESS"
let dbname = "Measure"
let saver = new Energon.Storage.ExperimentRuntimeSaverExpress(e, server, dbname )


// the helper makes easy to handle remote loads and remote sensors
let helper = new RemoteExperimentHelper(e)

helper.Start()

helper.Stop()




let database = dbname
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

let expId = 50

let exp = db.Experiments.Where(fun (e:SQLExpress.Experiments) -> e.Id = expId ).First()
let cases = db.ExperimentCases.Where(fun (c:SQLExpress.ExperimentCases) -> c.Experiment_id = exp.Id) |> Seq.toList
let runs = db.ExperimentRuns.Where(fun (r:SQLExpress.ExperimentRuns) -> cases |> Seq.exists (fun (c:SQLExpress.ExperimentCases) -> c.Id = r.Experiment_case_id) ) |> Seq.toList
let sensors = db.Sensors.Where(fun (s:SQLExpress.Sensors) -> runs |> Seq.exists (fun (r:SQLExpress.ExperimentRuns) -> s.Experiment_run_id = r.Id ))
let used_classes = sensors |> 
    Seq.map (fun (s:SQLExpress.Sensors) -> db.SensorClasses.Where(fun (c:SQLExpress.SensorClasses) -> s.Sensor_class_id = c.Id ).First().Id ) |> 
    Set.ofSeq |> Set.toList |> List.map (fun id -> db.SensorClasses.Where(fun (c:SQLExpress.SensorClasses) -> c.Id = id ).First())

used_classes |> List.iter (fun (c:SQLExpress.SensorClasses) -> System.Console.WriteLine(c.SensorName) )


sensors.Count()









let exp_id = 0
Energon.Storage.Loader.ExperimentList
let list_all = Energon.Storage.Loader.ExperimentList(server, dbname)
let list_linux32 = list |> Seq.filter (fun (e:SQLExpress.Experiments) -> e.Name.EndsWith("linux")) |> Seq.map (fun (e:SQLExpress.Experiments) -> (e.Id, e.Name) )
let list_cpuspec32 = list |> Seq.filter (fun (e:SQLExpress.Experiments) -> e.Name.Contains("cpuspec")) |> Seq.map (fun (e:SQLExpress.Experiments) -> (e.Id, e.Name) )
list2 |> Seq.map (fun (id,name) -> System.Console.WriteLine(name) )



let l = Energon.Storage.Loader.ExperimentLoader(32, server, dbname)
l.ExperimentCases.First().Runs







//CompactSQL
//Energon.CompactSQL.SaveExperiment e dbfile

let dbfile = @"C:\Users\root\Desktop\Energon\Measures\DB01.sdf"
let dbfile = @"C:\Users\root\Desktop\Energon\Measures\DB02.sdf"
let dbfile = @"C:\Users\root\Desktop\Energon\Measures\DB03.sdf"
let dbfile = @"C:\Users\root\Desktop\Energon\Measurements.sdf"

let dbfile = @"C:\progetti\Energon\Measures\DB01.sdf"
let dbfile = @"C:\progetti\Energon\Measures\DB02.sdf"
let dbfile = @"C:\progetti\Energon\Measures\DB03.sdf"
let dbfile = @"C:\progetti\Energon\Measures\DB04.sdf"
let dbfile = @"C:\progetti\Energon\Measures\Measurements.sdf"

// example getting data from db
open Energon.SQLCE
open Energon.CompactSQL
let db = Energon.CompactSQL.GetLinqContext dbfile
let exp = db.Experiments
exp.Count()
let expCases = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = 1 )
expCases.Count()
expCases
let arg1 (case:ExperimentCases) =
    let tags = case.Args.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    tags.[0]
for c in expCases do
    printf "%s\n" (arg1 c)

let expRuns (case:Energon.SQLCE.ExperimentCases) =
    db.ExperimentRuns.Where(fun (x:Energon.SQLCE.ExperimentRuns) -> x.Experiment_case_id = case.Id)
let handleRun (x:Energon.SQLCE.ExperimentRuns) =
    let sensors = db.Sensors.Where(fun (s:Energon.SQLCE.Sensors) -> s.Experiment_run_id = x.Id).OrderBy(fun (s:Energon.SQLCE.Sensors) -> s.Sensor_class_id) |> Seq.toArray
    let getSensorClass (s:Energon.SQLCE.Sensors) =
        db.SensorClasses.First(fun (c:Energon.SQLCE.SensorClasses) -> c.Id = s.Sensor_class_id)
    let getReadings (s:Energon.SQLCE.Sensors) =
        let measures = db.Measurements1.Where(fun (m:Energon.SQLCE.Measurements1) -> m.Sensor_id = s.Id )
        measures |> Seq.map (fun (m:Energon.SQLCE.Measurements1) -> m.Value) |> Seq.average
    let vals =
        Seq.map getReadings sensors
    (sensors.ToArray(), vals)

let handleCase (case:Energon.SQLCE.ExperimentCases) =
    let run = (expRuns case).First()
    let s,v = handleRun run
    let args = case.Args.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    let firstArg = match args.[0] with
        | "HOST_SEQ" -> 0.
        | "HOST_PAR" -> 1.
        | "OPENCL" -> 2.
        | _ -> -1.
    let argsToFloatSeq = seq {
            yield firstArg
            let argN = args.Length - 1
            for i in 1..argN do
                yield float(System.Single.Parse(args.[i]))
        }
    let argArray = argsToFloatSeq.ToArray()
    let valArray = v.ToArray()
    Array.concat [| argArray; valArray |]

let casesSubset = expCases.Where(fun (e:Energon.SQLCE.ExperimentCases) -> 
    let firstArg = match e.Args.Split([|";"|], StringSplitOptions.RemoveEmptyEntries).[0] with
                    | "HOST_SEQ" -> 0.
                    | "HOST_PAR" -> 1.
                    | "OPENCL" -> 2.
                    | _ -> -1.
    let sixthArg = match e.Args.Split([|";"|], StringSplitOptions.RemoveEmptyEntries).[5] with
                    | "0" -> 0.
                    | "1" -> 1.
                    | "2" -> 1.
                    | "3" -> 1.
                    | _ -> -1.
    firstArg = 2. && sixthArg = 1. ) |> Seq.skip 11 |> Seq.take 11
let casesSubset = expCases
casesSubset.Count()
expRuns (casesSubset.First())
let data cases =
    cases |> Seq.map (fun (c:Energon.SQLCE.ExperimentCases) -> (handleCase c))
handleCase (casesSubset.First())
let valuesMatrix = data casesSubset

let colNames (e:Energon.SQLCE.Experiments) (c:Energon.SQLCE.ExperimentCases) (r:Energon.SQLCE.ExperimentRuns)=
    let args = e.ArgNames.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    let sensors = db.Sensors.Where(fun (s:Energon.SQLCE.Sensors) -> s.Experiment_run_id = r.Id)
    let sensorName (s:Energon.SQLCE.Sensors) =
        db.SensorClasses.Where(fun (cl:Energon.SQLCE.SensorClasses) -> cl.Id = s.Sensor_class_id).First().SensorName
    let sensorsNames = (Seq.map sensorName sensors).ToArray()
    Array.concat [| args ; sensorsNames |]

let names = colNames (exp.First()) (expCases.First()) (Seq.head (expRuns (casesSubset.First())))

let sb = new System.Text.StringBuilder()
names |> Seq.iter (fun (s:string) -> sb.AppendFormat(@"{0};", s) |> ignore)
sb.AppendLine("")

valuesMatrix |> Seq.iter (fun (vals:float[]) ->
    for f in vals do
        sb.AppendFormat(@"{0};", f) |> ignore
    sb.AppendLine("") |> ignore
    )

sb.ToString()

System.IO.File.WriteAllText(@"C:\progetti\Energon\Measures\DB04.csv", sb.ToString())
System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\DB04.csv", sb.ToString())



let corrMatr = getCorrMatrix valuesMatrix

let cols = 77
let row = 1
for row in 0..76 do
    for i in row..76 do
        let value = corrMatr.[row,i]
        if not (row = i) then
            if not (Double.IsNaN(value) ) then
                if value > 0.4 then
                    printf "%s,%s:%f " (names.[row]) (names.[i]) corrMatr.[row,i]
                if value < -0.4 then
                    printf "%s,%s:%f " (names.[row]) (names.[i]) corrMatr.[row,i]



let getCorrMatrix data =
    let getMatrixInfo nCol (count,crossProd:float array array,sumVector:float array,sqVector:float array) (newLine:float array)   = 
        for i in 0..(nCol-1) do
                sumVector.[i]<-sumVector.[i]+newLine.[i]
                sqVector.[i]<-sqVector.[i]+(newLine.[i]*newLine.[i])
                for j in (i+1)..(nCol-1)  do
                    crossProd.[i].[j-(i+1)]<-crossProd.[i].[j-(i+1)]+newLine.[i]*newLine.[j] 
        let newCount = count+1
        //(newCount,newMatrix,newSumVector,newSqVector)    
        (newCount,crossProd,sumVector,sqVector)         
    //Get number of columns
    let nCol = data|>Seq.head|>Seq.length
    //Initialize objects for the fold
    let matrixStart = Array.init nCol (fun i -> Array.create (nCol-i-1) 0.0)                    
    let sumVector = Array.init nCol (fun _ -> 0.0)
    let sqVector = Array.init nCol (fun _ -> 0.0)
    let init = (0,matrixStart,sumVector,sqVector)
    //Run the fold and obtain all the elements to build te correlation matrix
    let (count,crossProd,sum,sq) = 
        data
        |>Seq.fold(getMatrixInfo nCol) init
    //Compute averages standard deviations, and finally correlations
    let averages = sum|>Array.map(fun s ->s/(float count))
    let std = Array.zip3 sum sq averages
              |> Array.map(fun (elemSum,elemSq,av)-> let temp = elemSq-2.0*av*elemSum+float(count)*av*av 
                                                     sqrt (temp/(float count-1.0)))
    //Map allteh elements to correlation                                         
    let rec getCorr i j =
        if i=j then
            1.0
        elif i<j then
            (crossProd.[i].[j-(i+1)]-averages.[i]*sum.[j]-averages.[j]*sum.[i]+(float count*averages.[i]*averages.[j]) )/((float count-1.0)*std.[i]*std.[j])
        else
            getCorr j i
    let corrMatrix =  Array2D.init nCol nCol (fun i j -> getCorr i j)
    corrMatrix



let a,b = handleRun ((expRuns (casesSubset.First())).First())
a
b

db.Measurements1.Where(fun (m:Energon.SQLCE.Measurements1) -> m.Sensor_id = 29405)
let sensors = db.Sensors.Where(fun (s:Energon.SQLCE.Sensors) -> s.Experiment_run_id = 1)
sensors.Count()
let sensorsArray = sensors.ToArray()
sensorsArray.[0]

db.SensorClasses.Where(fun (c:Energon.SQLCE.SensorClasses) -> c.Id = sensorsArray.[0].Sensor_class_id)
db.Measurements1.Where(fun (m:Energon.SQLCE.Measurements1) -> m.Sensor_id = sensorsArray.[0].Id)


for i in 0..61 do
    let c = db.Measurements1.Where(fun (m:Energon.SQLCE.Measurements1) -> m.Sensor_id = sensorsArray.[i].Id).Count()
    printf "%i " c

for i in 0..61 do
    let c = db.Measurements1.Where(fun (m:Energon.SQLCE.Measurements1) -> m.Sensor_id = sensorsArray.[i].Id).First()
    printf "%f " c.Value

db.Sensors

let meas = db.Measurements1.Where(fun _ -> true)
let measArray = meas.ToArray()
measArray.Count()
measArray.[319]

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
