//#I @"C:\Users\root\Desktop\Energon\bin\Debug"
//#r @"Energon.Measuring.dll"


module Benchmark

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
(*
#r @"Energon.Storage.dll"

#r @"C:\Program Files (x86)\Microsoft SQL Server Compact Edition\v4.0\Desktop\System.Data.SqlServerCe.dll"

#r "Energon.SQLCE.dll"

#r "System.Data.Linq.dll"
#r "System.Linq.dll"

#r "FSharp.PowerPack.Linq.dll"
#r "FSharp.Data.TypeProviders.dll"

#r "System.Data.DataSetExtensions.dll"
#r "System.Core.dll"
*)

open System
//open Microsoft.FSharp.Data.TypeProviders
open System.Data.Linq.SqlClient
open System.Linq
//open Microsoft.FSharp.Linq
open System.Data.Linq
open System.Data.SqlServerCe;
open Energon.Measuring
open System.Text
//open System.Data.DataSetExtensions


//CompactSQL
//Energon.CompactSQL.SaveExperiment e dbfile

let dbfile = @"C:\Users\root\Desktop\Energon\Measurements.sdf"

// example getting data from db
open Energon.SQLCE
open Energon.CompactSQL
let db = Energon.CompactSQL.GetLinqContext dbfile
let exp = db.Experiments



let expCasesReduce = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = 1 )
let expCasesSaxpy = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = 7 )

//let expCases = expCasesReduce
let expCases = expCasesSaxpy

let arg1 (case:ExperimentCases) =
    let tags = case.Args.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    tags.[0]

let expRuns (case:Energon.SQLCE.ExperimentCases) =
    db.ExperimentRuns.Where(fun (x:Energon.SQLCE.ExperimentRuns) -> x.Experiment_case_id = case.Id)
let handleRun (x:Energon.SQLCE.ExperimentRuns) =
    let sensors = db.Sensors.Where(fun (s:Energon.SQLCE.Sensors) -> s.Experiment_run_id = x.Id).OrderBy(fun (s:Energon.SQLCE.Sensors) -> s.Sensor_class_id) |> Seq.toArray
    let getSensorClass (s:Energon.SQLCE.Sensors) =
        db.SensorClasses.First(fun (c:Energon.SQLCE.SensorClasses) -> c.Id = s.Sensor_class_id)
    let getReadings (s:Energon.SQLCE.Sensors) : float =
        System.Console.WriteLine(System.String.Format("{0}, {1}", x.Id, s.Id)) |> ignore
        db.Measurements1.Where(fun (m:Energon.SQLCE.Measurements1) -> m.Sensor_id = s.Id).Average(fun (x : Energon.SQLCE.Measurements1) -> x.Value)
        
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

let data cases =
    cases |> Seq.map (fun (c:Energon.SQLCE.ExperimentCases) -> (handleCase c))

let cols = 78

let colNames (e:Energon.SQLCE.Experiments) (c:Energon.SQLCE.ExperimentCases) (r:Energon.SQLCE.ExperimentRuns)=
    let args = e.ArgNames.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    let sensors = db.Sensors.Where(fun (s:Energon.SQLCE.Sensors) -> s.Experiment_run_id = r.Id)
    let sensorName (s:Energon.SQLCE.Sensors) = 
        db.SensorClasses.Where(fun (cl:Energon.SQLCE.SensorClasses) -> cl.Id = s.Sensor_class_id).First().SensorName
    let sensorsNames = (Seq.map sensorName sensors).ToArray()
    Array.concat [| args ; sensorsNames ; [|"J"|] |]

let names = colNames (exp.First()) (expCases.First()) (Seq.head (expRuns (expCases.First())))

let colIdx name = 
    let mutable res = -1
    let namesL = names.Length - 1 
    for i in 0..namesL do
        if name = names.[i] then
            res <- i
    res

let colName idx = names.[idx]

let buildLabel primo idxsecondo =
    System.String.Format("{0},{1}", primo, (colName idxsecondo))

let correlationsLabels = seq {
    let maxCols = cols - 1
    for i in 0..maxCols do
        yield buildLabel "J" i
    for i in 0..maxCols do
        yield buildLabel "extechWatt" i
    for i in 0..maxCols do
        yield buildLabel "completionTime" i
}

let extendedColNames =
    Array.concat [| names ; correlationsLabels.ToArray() |]

let sb = new System.Text.StringBuilder()
//names |> Seq.iter (fun (s:string) -> sb.AppendFormat(@"{0};", s) |> ignore)
correlationsLabels |> Seq.iter (fun (s:string) -> sb.AppendFormat(@"{0};", s) |> ignore)
sb.AppendLine("") |> ignore

let sb2 = new System.Text.StringBuilder()

[<EntryPoint>]
let main args =
    for round in 0..22 do
        let offset = round * 11
        let casesSubset = expCases.Skip offset |> Seq.take 11
        let valuesMatrix = data casesSubset
        let vals = valuesMatrix.ToArray()
        let corrMatr = getCorrMatrix vals
        let j_index = colIdx "J"
        let w_index = colIdx "extechWatt" 
        let t_index = colIdx "completionTime" 

        let correlationsVals (corrMatrix:float[,]) = seq {
                let maxCols = cols - 1
                for i in 0..maxCols do
                    yield corrMatrix.[j_index,i]
                for i in 0..maxCols do
                    yield corrMatrix.[w_index,i]
                for i in 0..maxCols do
                    yield corrMatrix.[t_index,i]
            }
        sb.Append(casesSubset.First().Args.Replace(";", " ")) |> ignore
        correlationsVals corrMatr |> Seq.iter (fun f -> sb.AppendFormat(@"{0};", f) |> ignore)
        sb.AppendLine("") |> ignore

        for row in 0..(cols-1) do
            for i in row..(cols-1) do
                let value = corrMatr.[row,i]
                if not (row = i) then
                    if not (Double.IsNaN(value) ) then
                        if value > 0.4 then
                            sb2.AppendFormat("{0},{1}:{2};", (names.[row]), (names.[i]), corrMatr.[row,i]) |> ignore
                        if value < -0.4 then
                            sb2.AppendFormat("{0},{1}:{2};", (names.[row]), (names.[i]), corrMatr.[row,i]) |> ignore
        sb2.AppendLine("") |> ignore
    System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\saxpy_correlations.csv", sb.ToString())
    System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\saxpy_correlations_text.csv", sb2.ToString())
    0
