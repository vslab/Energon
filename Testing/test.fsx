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
let ts = getAveragesForCase 3416
Seq.map (fun (a:AvgMeasures) -> System.Console.WriteLine("{0}", a.SensorName)) ts

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

let caseToAveragesOnlyT caseid =
    let avgs = getAveragesForCase caseid
    let s = avgs.Where(fun (a:AvgMeasures) -> a.Sensor_class_id=87).First()
    s.Average.Value

let caseToAveragesNoJ caseid =
    let avgs = getAveragesForCase caseid
    let filtered = avgs.Where(fun (a:AvgMeasures) -> a.Sensor_class_id <> WsensorID && a.Sensor_class_id <> otherWsensor)
    let response = Seq.map (fun (a:AvgMeasures) -> a.Average.Value) filtered
    response

// print sensornames
//let caseid = 3206
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
let getTestBedAveragesArrays (list:seq<int array>) =
    (Seq.map (fun (l:int array) -> (getProgAverages l).ToArray() ) list).ToArray()   
let buildTestBed (list:seq<seq<float>>) =
    let programFromAverages (l:seq<float>) =
        let p = new Program()
        p.Measures <- l.ToArray()
        p
    let programSeq = Seq.map (fun (l:seq<float>) -> programFromAverages l) list
    programSeq.ToArray()

let getProgNames (list:seq<int>) =
    let concatNameAndArgs (name:string) (args:string) = 
        //String.Format("{0}_{1}", name, args)
        name
    let getExperimentNameAndArgs caseid = 
        let tmp = db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Id = caseid).First()
        concatNameAndArgs (tmp.Name) (tmp.Args)
    Seq.fold (fun (state:seq<string>) (id:int) -> Seq.append state [|getExperimentNameAndArgs id|]) Seq.empty<string> list

let getProgNamesAndArgs (list:seq<int>) =
    let concatNameAndArgs (name:string) (args:string) = 
        String.Format("{0}_{1}", name, args)
    let getExperimentNameAndArgs caseid = 
        let tmp = db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Id = caseid).First()
        concatNameAndArgs (tmp.Name) (tmp.Args)
    Seq.fold (fun (state:seq<string>) (id:int) -> Seq.append state [|getExperimentNameAndArgs id|]) Seq.empty<string> list

let getColumnsNames (list:seq<int array>) =
    getProgNames (Seq.map (fun (l:int array) -> l.[0]) list)

let sanityzeString (s:string) = 
    s.Replace(";", " ")

let sanityzeStringSeq (l:seq<string>) =
    Seq.map sanityzeString l

open System.Globalization





let getEstimationError (target:int) (casesTestbed:seq<int>) (splitup:seq<float>) =
    let testbedMeasuredT =  Seq.map (fun (id:int) -> caseToAveragesOnlyT id) casesTestbed
    let estimateT =
        let zipped = Seq.zip testbedMeasuredT splitup
        Seq.fold (fun (state:float) (a:float,b:float) -> state + a*b) 0.0 zipped
    let measuredTargetT = caseToAveragesOnlyT target
    let difference = Math.Abs(measuredTargetT - estimateT)
    let percentage = difference / measuredTargetT
    (measuredTargetT, estimateT, difference, percentage)

let printEstimationErrors (target:int) (casesTestbed:seq<int>) (splitup:seq<float>) (sb:StringBuilder) =
    let expandcase id = db.ExperimentAndCases.Where(fun (c:ExperimentAndCases) -> c.Id = id).First()
    let getname (expandcase:ExperimentAndCases) = 
        String.Format("{0}_{1}", expandcase.Name, expandcase.Args)
    let appendInfo id =
        let name = getname (expandcase id)
        let m,e,d, p = getEstimationError id casesTestbed splitup
        sb.AppendLine(String.Format("{0};{1};{2};{3};{4}", sanityzeString name, m, e, d, p)) |> ignore
    appendInfo target

let printEstimations (targets:seq<int>) (casesTestbed:seq<int array>) (splitup:seq<float>) (sb:StringBuilder) =
    let targetsArray = targets.ToArray()
    let casesTestbedArray = casesTestbed.ToArray()
    for i in 0..(targetsArray.Count()-1) do
        let selectedtb = Seq.map (fun (l:int array) -> l.ElementAt i ) casesTestbed
        let selectedt = targetsArray.ElementAt(i)
        printEstimationErrors selectedt selectedtb splitup sb

let printSplitups (progname:string) listOfTargets  (s:SplitupFinder) columnsNames (casesTestbed:seq<int array>) =
    let sbSplitups = new System.Text.StringBuilder()
    sbSplitups.Append("program;") |> ignore
    sbSplitups.AppendLine(String.concat ";" (sanityzeStringSeq columnsNames))  |> ignore
    let sbEstimations = new System.Text.StringBuilder()
    sbEstimations.AppendLine("program;measured;estimated;error;perc")  |> ignore
    let findSplitup target =
        let progname = (getProgNamesAndArgs target).First()
        sbSplitups.AppendFormat("{0};", (sanityzeString progname)) |> ignore
        let p = new Program()
        p.Measures <- (getProgAverages target).ToArray()
        //p.Measures <- (getProgAverages casesRandMemAccess).ToArray()
        s.Target <- p
        if (s.FindSplitup()) then
            let floatToStrings (l:seq<float>) =
                let ni = new System.Globalization.NumberFormatInfo()
                ni.NumberDecimalSeparator <- "."
                ni.NumberGroupSeparator <-""
                Seq.map (fun (f:float) -> f.ToString(ni) ) l
            sbSplitups.AppendLine(String.concat ";" (floatToStrings s.Splitup))  |> ignore
            printEstimations target casesTestbed (s.Splitup) sbEstimations
        else
            System.Console.WriteLine("could not find splitup")
    Seq.iter findSplitup listOfTargets
    let filename = String.Format(@"C:\Users\root\Desktop\Energon\data\splitups{0}.csv", progname)
    System.IO.File.WriteAllText(filename, sbSplitups.ToString())
    let filename2 = String.Format(@"C:\Users\root\Desktop\Energon\data\estimations{0}.csv", progname)
    System.IO.File.WriteAllText(filename2, sbEstimations.ToString())
    Console.WriteLine(String.Format(@"---- done processing {0}", progname))


// --------------------- ONLY LINUX64 LINUX --------------------------

// pi and sorting algs: using linux64, linux and win
let casesRandMemAccess =  [| 3409; 3415; |]
let casesSimpleINT = [| 3410; 3416; |]
let casesSimpleFPU = [| 3411; 3417;|]
let casesTestbed = [| casesRandMemAccess; casesSimpleINT; casesSimpleFPU |]
let columnsNames = getColumnsNames casesTestbed

let s = new SplitupFinder()
let testbedAvgs = getTestBedAverages casesTestbed
s.Testbed <- (buildTestBed testbedAvgs)


Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("pi")))
let casesPi = [| 3412; 3418; 3422 |]
printSplitups "Pi_simpleTestbed" [| casesPi |] s columnsNames casesTestbed

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("randMem")))
Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("simple")))


Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("heap")))
let casesHeap4M = [| 3293; 3198 ; |]
let casesHeap16M = [| 3294; 3199;   |]
let casesHeap64M = [| 3295; 3200;  |]
let casesHeap256M = [| 3296; 3201;  |]
printSplitups "Heapsort_simpleTestbed" [| casesHeap4M; casesHeap16M; casesHeap64M; casesHeap256M |] s columnsNames casesTestbed

//Seq.iter (fun (a:AvgMeasures) -> System.Console.WriteLine(String.Format("{0}:{1}", a.SensorName, a.Sensor_class_id )) ) (getAveragesForCase 3409)

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("merge")))
let casesMerge4M = [| 3287 ; 3192;  |]
let casesMerge16M = [| 3288; 3193; |]
let casesMerge64M = [| 3289; 3194; |]
let casesMerge256M = [| 3290; 3195; |]
printSplitups "Mergesort_simpleTestbed" [| casesMerge4M; casesMerge16M; casesMerge64M; casesMerge256M |] s columnsNames casesTestbed

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("quick")))
let casesQuick4M = [| 3281 ; 3181; |]
let casesQuick16M = [| 3282; 3182; |]
let casesQuick64M = [| 3283; 3183; |]
let casesQuick256M = [| 3284; 3184; |]
printSplitups "Quicksort_simpleTestbed" [| casesQuick4M; casesQuick16M; casesQuick64M; casesQuick256M |] s columnsNames casesTestbed



// --------------------- ONLY LINUX64 LINUX WIN --------------------------

// pi and sorting algs: using linux64, linux and win
let casesRandMemAccess =  [| 3409; 3415; 3419 |]
let casesSimpleINT = [| 3410; 3416; 3420 |]
let casesSimpleFPU = [| 3411; 3417; 3421 |]
let casesTestbed = [| casesRandMemAccess; casesSimpleINT; casesSimpleFPU |]
let columnsNames = getColumnsNames casesTestbed

let s = new SplitupFinder()
let testbedAvgs = getTestBedAverages casesTestbed
s.Testbed <- (buildTestBed testbedAvgs)


Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("pi")))
let casesPi = [| 3412; 3418; 3422 |]
printSplitups "Pi" [| casesPi |] s columnsNames casesTestbed

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("randMem")))
Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("simple")))


Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("heap")))
let casesHeap4M = [| 3293; 3198 ; 3363;  |]
let casesHeap16M = [| 3294; 3199; 3364;  |]
let casesHeap64M = [| 3295; 3200; 3365;  |]
let casesHeap256M = [| 3296; 3201; 3366;  |]
printSplitups "Heapsort_no_arm" [| casesHeap4M; casesHeap16M; casesHeap64M; casesHeap256M |] s columnsNames casesTestbed

//Seq.iter (fun (a:AvgMeasures) -> System.Console.WriteLine(String.Format("{0}:{1}", a.SensorName, a.Sensor_class_id )) ) (getAveragesForCase 3409)

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("merge")))
let casesMerge4M = [| 3287 ; 3192; 3357; |]
let casesMerge16M = [| 3288; 3193; 3358;  |]
let casesMerge64M = [| 3289; 3194; 3359;  |]
let casesMerge256M = [| 3290; 3195; 3360;  |]
printSplitups "Mergesort_no_arm" [| casesMerge4M; casesMerge16M; casesMerge64M; casesMerge256M |] s columnsNames casesTestbed

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("quick")))
let casesQuick4M = [| 3281 ; 3181; 3351;  |]
let casesQuick16M = [| 3282; 3182; 3352;  |]
let casesQuick64M = [| 3283; 3183; 3353;  |]
let casesQuick256M = [| 3284; 3184; 3354; |]
printSplitups "Quicksort_no_arm" [| casesQuick4M; casesQuick16M; casesQuick64M; casesQuick256M |] s columnsNames casesTestbed




// --------    with arm    --------

let casesRandMemAccess =  [| 3409; 3415; 3419; 3329 |]
let casesSimpleINT = [| 3410; 3416; 3420; 3330 |]
let casesSimpleFPU = [| 3411; 3417; 3421; 3332 |]
let casesTestbed = [| casesRandMemAccess; casesSimpleINT; casesSimpleFPU |]
let columnsNames = getColumnsNames casesTestbed

let s = new SplitupFinder()
let testbedAvgs = getTestBedAverages casesTestbed
s.Testbed <- (buildTestBed testbedAvgs)

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("heap")))
let casesHeap4M = [| 3293; 3198 ; 3363; 3335 |]
let casesHeap16M = [| 3294; 3199; 3364; 3336 |]
let casesHeap64M = [| 3295; 3200; 3365; 3337 |]
let casesHeap256M = [| 3296; 3201; 3366; 3338 |]
printSplitups "Heapsort_arm" [| casesHeap4M; casesHeap16M; casesHeap64M; casesHeap256M |] s columnsNames casesTestbed

//Seq.iter (fun (a:AvgMeasures) -> System.Console.WriteLine(String.Format("{0}:{1}", a.SensorName, a.Sensor_class_id )) ) (getAveragesForCase 3409)

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("merge")))
let casesMerge4M = [| 3287 ; 3192; 3357; 3318 |]
let casesMerge16M = [| 3288; 3193; 3358; 3319 |]
let casesMerge64M = [| 3289; 3194; 3359; 3320 |]
let casesMerge256M = [| 3290; 3195; 3360; 3321 |]
printSplitups "Mergesort_arm" [| casesMerge4M; casesMerge16M; casesMerge64M; casesMerge256M |] s columnsNames casesTestbed

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("quick")))
let casesQuick4M = [| 3281 ; 3181; 3351; 3312 |]
let casesQuick16M = [| 3282; 3182; 3352; 3313 |]
let casesQuick64M = [| 3283; 3183; 3353; 3314 |]
let casesQuick256M = [| 3284; 3184; 3354; 3315 |]
printSplitups "Quicksort_arm" [| casesQuick4M; casesQuick16M; casesQuick64M; casesQuick256M |] s columnsNames casesTestbed


// ---------------------------- ONLY LINUX64 LINUX --------------------------------------

// CPUSPEC
let casesRandMemAccess =  [| 3409; 3415 |]
let casesSimpleINT = [| 3410; 3416 |]
let casesSimpleFPU = [| 3411; 3417|]
let casesTestbed = [| casesRandMemAccess; casesSimpleINT; casesSimpleFPU |]
let columnsNames = getColumnsNames casesTestbed

let s = new SplitupFinder()
let testbedAvgs = getTestBedAverages casesTestbed
s.Testbed <- (buildTestBed testbedAvgs)


Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("401")))
let cases401 = [| 3242 ; 3206|]
Seq.iter (fun (c:float) -> System.Console.WriteLine("{0}", c)) (caseToAveragesNoJ (cases401.ElementAt 0) )

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("435")))
let cases435 = [| 3243 ; 3207|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("445")))
let cases445 = [| 3244 ; 3208|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("444")))
let cases444 = [| 3245 ; 3209|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("410")))
let cases410 = [| 3250 ; 3210|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("429")))
let cases429 = [| 3252 ; 3212|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("464")))
let cases464 = [| 3253 ; 3213|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("458")))
let cases458 = [| 3254 ; 3214|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("471")))
let cases471 = [| 3255 ; 3215|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("434")))
let cases434 = [| 3257 ; 3216|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("453")))
let cases453 = [| 3255 ; 3215|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("436")))
let cases436 = [| 3259 ; 3218|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("465")))
let cases465 = [| 3260 ; 3219|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("483")))
let cases483 = [| 3261 ; 3220|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("462")))
let cases462 = [| 3262 ; 3221|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("470")))
let cases470 = [| 3264 ; 3222|]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("437")))
let cases437 = [| 3265 ; 3223 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("433")))
let cases433 = [| 3266 ; 3224 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("450")))
let cases450 = [| 3267 ; 3225 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("403")))
let cases403 = [| 3268 ; 3226 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("482")))
let cases482 = [| 3269 ; 3227 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("456")))
let cases456 = [| 3270 ; 3228 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("416")))
let cases416 = [| 3271 ; 3229 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("999")))
let cases999 = [| 3272 ; 3232 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("447")))
let cases447 = [| 3273 ; 3234 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("473")))
let cases473 = [| 3274 ; 3235 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("400")))
let cases400 = [| 3275 ; 3236 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("481")))
let cases481 = [| 3276 ; 3237 |]


Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("459")))
let cases459 = [| 3277 ; 3238 |]

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("998")))
let cases998 = [| 3278 ; 3239 |]


let specCases = [| cases401; cases435; cases445; cases444; cases410; cases429; cases464; cases458; cases471; cases434; cases453; cases436; cases465; cases483 ; cases462; cases470; cases437; cases433; cases450; cases403; cases482; cases456; cases416; cases999; cases447; cases473; cases400; cases481; cases459; cases998 |]
printSplitups "CPUSPEC" specCases s columnsNames casesTestbed

// -------------- testbed using cpuspec ----------- 

let avgs1 = (getProgAverages cases998).ToArray()
let avgs2 = (getProgAverages cases459).ToArray()
let distance l1 l2 =
    let zipped = Seq.zip l1 l2
    let (sum) = Seq.fold (fun (sum) (v1, v2)-> (sum+(v2-v1)*(v2-v1))) (0.) zipped
    Math.Sqrt sum
let average (list) =
    let (sum, count) = list |> Seq.fold (fun (sums, count) (l) -> ((Seq.zip sums l |> Seq.map (fun (v1:float,v2:float)->v1+v2) ), ( Seq.map (fun c -> c + 1.) count ))) (Seq.init (list.First().Count()) (fun _ -> 0.), Seq.init (list.First().Count()) (fun _ -> 0.))
    let l = (Seq.map (fun (s,c) -> s/c) (Seq.zip sum count))
    l.ToArray()
let same s1 s2 =
    Seq.forall2 (fun (f1:float) (f2:float) -> f1=f2) s1 s2

let rec kmeansCore n (seeds:float[][]) (points:seq<float array>) =
    if n=0 then
        seeds
    else
        let grouped = points.GroupBy(fun l -> seeds.OrderBy(fun x -> distance x l).First())
        let newseeds = (Seq.map average grouped).ToArray()
        if (Seq.forall2 same seeds newseeds) then
            seeds
        else
            kmeansCore (n-1) newseeds points

let pickRandomElements (l:float[][]) n =
    let dt = DateTime.Now
    let random = new Random(dt.Millisecond + 1000*dt.Second + 60000*dt.Minute)
    let rec pickRandomElement (res:float[][]) (src:float[][]) n =
        if n>0 then
            let idx = random.Next(0, src.Length - 1)
            let chosen = src.ElementAt idx
            let newres = res.Concat( [| chosen |] ).ToArray()
            pickRandomElement newres (src.Where(fun f -> f <> chosen).ToArray()) (n-1)
        else
            res
    let start:float[][] = [| |]
    pickRandomElement start l n

let clusterError (centers:float[][]) (points:float[][]) = 
     let grouped = points.GroupBy(fun l -> centers.OrderBy(fun x -> distance x l).First())        
     grouped.Aggregate(0., fun (sum:float) (element:IGrouping<float[], float[]>) -> sum + element.ToList().Aggregate(0., fun (insum:float) (el:float[]) -> insum + distance (element.Key) el) )

let kmeanstep (points:float[][]) size =
    let seeds = pickRandomElements points size
    let centers = kmeansCore 10 seeds points
    let error = clusterError centers points
    centers, error

let rec kmeans points n =
    let centers, error = kmeanstep points n
    if n > 2 then
        let othercenters, othererror = kmeans points (n-1)
        if (othererror > error) then
            centers, error
        else
            othercenters, othererror
    else
        centers, error

let normalizedKmeans (points:float[][]) n =
    let resources = points.ElementAt(0).Count()
    let max (v1:float, v2:float) = if v1> v2 then v1 else v2 
    let maxv = points.Aggregate(Array.init (resources) (fun _ -> 0.), fun state prog ->  (Seq.map max (Seq.zip state prog)).ToArray() ).ToArray() 
    let normVect prog = (Seq.zip prog maxv |> Seq.map (fun (a:float, b:float) -> a/b)).ToArray()
    let normalizedPoints = (Seq.map normVect points).ToArray()
    let c, e = kmeans normalizedPoints n
    let nearest = Seq.map (fun (center:float[]) -> normalizedPoints.OrderBy(fun x -> distance x center).First() ) c
    let getIndex prog =
        let mutable idx = -1
        for i in 0..(normalizedPoints.Count()-1) do
            if (Seq.forall2 (fun v1 v2 -> v1 = v2) prog (normalizedPoints.ElementAt i)) then
                idx <- i
        idx
    (Seq.map (fun (prog:float[]) -> points.ElementAt(getIndex prog) ) nearest).ToArray(), (Seq.map (fun (prog:float[]) -> getIndex prog ) nearest).ToArray()
    
let test1 = [| [| 0.; 1. |]; [| 1. ; 0.|] |]
test1.Aggregate(Array.init (2) (fun _ -> 0.), fun state prog -> (Seq.zip state prog |> Seq.map (fun (v1:float,v2:float) -> Math.Max(v1,v2) )).ToArray()  )

let programs = getTestBedAveragesArrays specCases

let c, indices = normalizedKmeans programs 3
let casesTestbedOLD = Seq.map (fun i -> specCases.ElementAt(i)) indices
let casesTestbed = Seq.append casesTestbedOLD [| cases450; cases470 |]
let casesTestbed = Seq.append casesTestbedOLD [| casesSimpleFPU |]
let columnsNames = getColumnsNames casesTestbed

let s = new SplitupFinder()
let testbedAvgs = getTestBedAverages casesTestbed
s.Testbed <- (buildTestBed testbedAvgs)

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("heap")))
let casesHeap4M = [| 3293; 3198 ; |]
let casesHeap16M = [| 3294; 3199; |]
let casesHeap64M = [| 3295; 3200; |]
let casesHeap256M = [| 3296; 3201;  |]
printSplitups "Heapsort_cpuspec3_fpu" [| casesHeap4M; casesHeap16M; casesHeap64M; casesHeap256M |] s columnsNames casesTestbed

let casesMerge4M = [| 3287 ; 3192;  |]
let casesMerge16M = [| 3288; 3193; |]
let casesMerge64M = [| 3289; 3194; |]
let casesMerge256M = [| 3290; 3195; |]
printSplitups "Mergesort_cpuspec3_fpu" [| casesMerge4M; casesMerge16M; casesMerge64M; casesMerge256M |] s columnsNames casesTestbed

Seq.iter (fun (c:ExperimentAndCases) -> System.Console.WriteLine("{0}:{1}:{2}:{3}", c.Experiment_id, c.Name, c.Id, c.Args)) (db.ExperimentAndCases.Where(fun (e:ExperimentAndCases) -> e.Name.StartsWith("quick")))
let casesQuick4M = [| 3281 ; 3181; |]
let casesQuick16M = [| 3282; 3182; |]
let casesQuick64M = [| 3283; 3183; |]
let casesQuick256M = [| 3284; 3184; |]
printSplitups "Quicksort_cpuspec3_fpu" [| casesQuick4M; casesQuick16M; casesQuick64M; casesQuick256M |] s columnsNames casesTestbed

let casesRandMemAccess =  [| 3409; 3415; |]
printSplitups "RandMemAccess_cpuspec3_fpu" [| casesRandMemAccess;|] s columnsNames casesTestbed

let casesSimpleINT = [| 3410; 3416; |]
printSplitups "SimpleINT_cpuspec3_fpu" [| casesSimpleINT; |] s columnsNames casesTestbed

let casesSimpleFPU = [| 3411; 3417;|]
printSplitups "SimpleFPU_cpuspec3_fpu" [| casesSimpleFPU;|] s columnsNames casesTestbed


let casesPi = [| 3412; 3418 |]
printSplitups "Pi_cpuspec3_fpu" [| casesPi |] s columnsNames casesTestbed


// all cpuspec
let specCases = [| cases401; cases435; cases445; cases444; cases410; cases429; cases464; cases458; cases471; cases434; cases453; cases436; cases465; cases483 ; cases462; cases470; cases437; cases433; cases450; cases403; cases482; cases456; cases416; cases999; cases447; cases473; cases400; cases481; cases459; cases998 |]
printSplitups "CPUSPEC_cpuspec3_fpu" specCases s columnsNames casesTestbed


let specCases = [| cases401; cases435; cases445; cases444; cases410; cases429; cases464; cases458; cases471; cases434; cases453; cases436; cases483 ; cases470; cases437; cases433; cases450; cases403; cases482; cases456; cases416; cases999; cases447; cases473; cases400; cases481; cases998 |]
printSplitups "CPUSPEC_cpuspec3_1" specCases s columnsNames casesTestbed

let specCases5 = [| cases401; cases435; cases444; cases410; cases429; cases458; cases434; cases453; cases436; cases483 ; cases462; cases470; cases437; cases433; cases450; cases403; cases482; cases456; cases416; cases999; cases447; cases473; cases400; cases481; cases459 |]
printSplitups "CPUSPEC_cpuspec5" specCases5 s columnsNames casesTestbed

columnsNames.ToArray()
























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
