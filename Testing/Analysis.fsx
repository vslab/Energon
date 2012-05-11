#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#r @"Energon.Measuring.dll"
open Energon.Measuring



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



// ****************** SqlCE ***************************
#r @"Energon.Storage.dll"

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


//CompactSQL
//Energon.CompactSQL.SaveExperiment e dbfile

let dbfile = @"C:\Users\root\Desktop\Energon\Reduce.sdf"

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
    let j = valArray.[0] * valArray.[1]
    Array.concat [| argArray; valArray; [| j |] |]

let casesSubset = expCases.Take 11

let casesSubset = expCases.Where(fun (e:Energon.SQLCE.ExperimentCases) -> 
    let args = e.Args.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    let firstArg = match args.[0] with
                    | "HOST_SEQ" -> 0.
                    | "HOST_PAR" -> 1.
                    | "OPENCL" -> 2.
                    | _ -> -1.
    let ndev = args.[5]
    let validDev = 
        match args.[6], args.[9], args.[12] with
        | "0", "1", "2" -> 
    ndev = "1" && firstArg = 1. )

//let casesSubset = expCases
casesSubset.Count()
expRuns (casesSubset.First())

let data cases =
    cases |> Seq.map (fun (c:Energon.SQLCE.ExperimentCases) -> (handleCase c))
let vtest = handleCase (casesSubset.First())
vtest.Count()
let valuesMatrix = data casesSubset

let colNames (e:Energon.SQLCE.Experiments) (c:Energon.SQLCE.ExperimentCases) (r:Energon.SQLCE.ExperimentRuns)=
    let args = e.ArgNames.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    let sensors = db.Sensors.Where(fun (s:Energon.SQLCE.Sensors) -> s.Experiment_run_id = r.Id)
    let sensorName (s:Energon.SQLCE.Sensors) = 
        db.SensorClasses.Where(fun (cl:Energon.SQLCE.SensorClasses) -> cl.Id = s.Sensor_class_id).First().SensorName
    let sensorsNames = (Seq.map sensorName sensors).ToArray()
    Array.concat [| args ; sensorsNames ; [|"J"|] |]

let names = colNames (exp.First()) (expCases.First()) (Seq.head (expRuns (casesSubset.First())))

let colIdx name = 
    let mutable res = -1
    for i in 0..(names.Length) do
        if name = names.[i] then
            res <- i
    res

let colName idx = names.[idx]

let sb = new System.Text.StringBuilder()
names |> Seq.iter (fun (s:string) -> sb.AppendFormat(@"{0};", s) |> ignore)
sb.AppendLine("")

valuesMatrix |> Seq.iter (fun (vals:float[]) ->
    for f in vals do
        sb.AppendFormat(@"{0};", f) |> ignore
    sb.AppendLine("") |> ignore
    )

sb.ToString()

System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\DB03.csv", sb.ToString())

let vals = valuesMatrix.ToArray()
vals.[0].Count()
names.Count()
let corrMatr = getCorrMatrix vals

let cols = 78
let row = 1
for row in 0..(cols-1) do
    for i in row..(cols-1) do
        let value = corrMatr.[row,i]
        if not (row = i) then
            if not (Double.IsNaN(value) ) then
                if value > 0.4 then
                    printf "%s,%s:%f " (names.[row]) (names.[i]) corrMatr.[row,i]
                if value < -0.4 then
                    printf "%s,%s:%f " (names.[row]) (names.[i]) corrMatr.[row,i]





