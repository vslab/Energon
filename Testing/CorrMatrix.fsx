﻿#I @"C:\Users\root\Desktop\Energon\bin\Debug"
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


#r "System.Data.Linq.dll"
#r "System.Linq.dll"

#r "FSharp.PowerPack.Linq.dll"
#r "FSharp.Data.TypeProviders.dll"

#r "System.Data.DataSetExtensions.dll"
#r "System.Core.dll"
#r "SQLExpress.dll"

open System
open Microsoft.FSharp.Data.TypeProviders
open System.Data.Linq.SqlClient
open System.Linq
open Microsoft.FSharp.Linq
open System.Data.Linq

open Energon.Measuring
open System.Text
open System.Data.DataSetExtensions

open System.Data.SqlServerCe;




open SQLExpress

let server = "HPLAB\SQLEXPRESS"
let database = "Measure"

let getConStr = 
    //let conStr = System.String.Format("server='{0}';database='{1}';User Id='{2}';password='{3}';", server, database, user, password) in
    let conStr = System.String.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", server, database) in
    conStr;

let GetLinqContext = 
    let context = new SQLExpress.Measure(getConStr)
    if (context.DatabaseExists() = false) then
            context.CreateDatabase()
    context

let db = GetLinqContext
let exps = db.Experiments
//exps.ToArray()
let exp = db.Experiments.Where(fun (x:Experiments) -> x.Id = 4)
let exp = db.Experiments.Where(fun (x:Experiments) -> x.Id = 7)
let exp = db.Experiments.Where(fun (x:Experiments) -> x.Id = 11)
let expCasesConvolution = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = 4 )
let expCasesConvolutionDGPU = db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
    if x.Experiment_id = 4 then
        let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
        let mode = args.[0]
        let ndev = args.[6]
        let dev = args.[7]
        if mode = "OPENCL" && ndev = "1" && dev = "0" then
            true
        else
            false
    else
        false    
    )
//expCasesConvolutionDGPU.Count()
let expCasesConvolutionIGX = db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
    if x.Experiment_id = 4 then
        let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
        let mode = args.[0]
        let ndev = args.[6]
        let dev = args.[7]
        if mode = "OPENCL" && ndev = "1" && dev = "1" then
            true
        else
            false
    else
        false
    )

let expCasesReduce = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = 11 )
let expCasesSaxpy = db.ExperimentCases.Where(fun (x:ExperimentCases) -> x.Experiment_id = 7 )

let expCasesSaxpyDGPU = db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
    if x.Experiment_id = 7 then
        let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
        let mode = args.[0]
        let ndev = args.[5]
        let dev = args.[6]
        if mode = "OPENCL" && ndev = "1" && dev = "0" then
            true
        else
            false
    else
        false    
    )
//expCasesConvolutionDGPU.Count()
let expCasesSaxpyIGX = db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
    if x.Experiment_id = 7 then
        let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
        let mode = args.[0]
        let ndev = args.[5]
        let dev = args.[6]
        if mode = "OPENCL" && ndev = "1" && dev = "1" then
            true
        else
            false
    else
        false
    )

let expCasesReduceDGPU = db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
    if x.Experiment_id = 11 then
        let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
        let mode = args.[0]
        let ndev = args.[5]
        let dev = args.[6]
        if mode = "OPENCL" && ndev = "1" && dev = "0" then
            true
        else
            false
    else
        false    
    )
//expCasesConvolutionDGPU.Count()
let expCasesReduceIGX = db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
    if x.Experiment_id = 11 then
        let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
        let mode = args.[0]
        let ndev = args.[5]
        let dev = args.[6]
        if mode = "OPENCL" && ndev = "1" && dev = "1" then
            true
        else
            false
    else
        false
    )


let expCases = expCasesReduceDGPU
let expCases = expCasesReduceIGX
//let expCases = expCasesSaxpyDGPU
//let expCases = expCasesSaxpyIGX
let expCases = expCasesConvolutionDGPU
let expCases = expCasesConvolutionIGX
//let expCases = expCasesReduce
//let expCases = expCasesSaxpy
//let expCases = expCasesConvolution

let arg1 (case:ExperimentCases) =
    let tags = case.Args.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    tags.[0]

let expRuns (case:ExperimentCases) =
    db.ExperimentRuns.Where(fun (x:ExperimentRuns) -> x.Experiment_case_id = case.Id)

let handleRun (x:ExperimentRuns) =
    let sensors = db.Sensors.Where(fun (s:Sensors) -> s.Experiment_run_id = x.Id).OrderBy(fun (s:Sensors) -> s.Sensor_class_id) |> Seq.toArray
    let getSensorClass (s:Sensors) =
        db.SensorClasses.First(fun (c:SensorClasses) -> c.Id = s.Sensor_class_id)
    let getReadings (s:Sensors) =
        //let measures = db.Measurements1.Where(fun (m:Measurements1) -> m.Sensor_id = s.Id )
        //measures |> Seq.map (fun (m:Measurements1) -> m.Value) |> Seq.average
        let measures = db.Measurements.Where(fun (m:Measurements) -> m.Sensor_id = s.Id )
        measures |> Seq.map (fun (m:Measurements) -> m.Value) |> Seq.average
    let vals =
        Seq.map getReadings sensors
    (sensors.ToArray(), vals)

let handleCase (case:ExperimentCases) =
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
    let mat_size = argArray.[1] * argArray.[2]
    Array.concat [| argArray; valArray; [| j ; mat_size |] |]

let data cases =
    cases |> Seq.map (fun (c:ExperimentCases) -> (handleCase c))


let colNames (e:Experiments) (c:ExperimentCases) (r:ExperimentRuns)=
    let args = e.ArgNames.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
    let sensors = db.Sensors.Where(fun (s:Sensors) -> s.Experiment_run_id = r.Id)
    let sensorName (s:Sensors) = 
        db.SensorClasses.Where(fun (cl:SensorClasses) -> cl.Id = s.Sensor_class_id).First().SensorName
    let sensorsNames = (Seq.map sensorName sensors).ToArray()
    Array.concat [| [|" "|] ;args ; sensorsNames ; [|"J"; "matrix_size"|] |]

let names = colNames (exp.First()) (expCases.First()) (Seq.head (expRuns (expCases.First())))

let cols = names.Count()

//names.ToArray().Length
//exp.First().ArgNames.Split([|";"|], StringSplitOptions.RemoveEmptyEntries).Length
//names.Count()

let colIdx name = 
    let mutable res = -1
    let namesL = names.Length - 1 
    for i in 0..namesL do
        if name = names.[i] then
            res <- i
    res

let colName idx = names.[idx]


//sb.AppendLine("")


let casesSubset = expCases
//let casesSubset = expCases.Skip (round*11) |> Seq.take 11
let valuesMatrix = data casesSubset
let vals = valuesMatrix.ToArray()
let corrMatr = getCorrMatrix vals
let rows = names.Length - 1
//let interestingIdx = [| 1;2;7;8;9;16;17;18;19;20;21;22;23;24;25;26;27;28;29;30;31;32;33;34;35;36;37;38 |]
let interestingIdx = [| 1;2;7;8;9;16;17;18;22;23;26;27;28;29;30;35 |]
let interestingIdx = [| 1..36 |]

let sb = new System.Text.StringBuilder()
sb.Append(" ;");
//names |> Seq.iter (fun (s:string) -> sb.AppendFormat(@"{0};", s) |> ignore)
interestingIdx |> Seq.iter (fun i -> sb.AppendFormat(@"{0};", colName (i+1)) |> ignore)

for i in interestingIdx do
    sb.AppendLine("") |> ignore
    sb.AppendFormat(@"{0};", colName (i+1)) |> ignore
    for j in interestingIdx do
        sb.AppendFormat(@"{0};", corrMatr.[i,j]) |> ignore



1+1



sb.ToString()


//System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\convolution_correlations.csv", sb.ToString())
//System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\convolution_correlations_text.csv", sb2.ToString())

System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\convolution_DGPU_corrMatr.csv", sb.ToString())
System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\convolution_IGX_corrMatr.csv", sb.ToString())

System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\saxpy_DGPU_corrMatr.csv", sb.ToString())
System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\saxpy_IGX_corrMatr.csv", sb.ToString())

System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\reduce_DGPU_corrMatr.csv", sb.ToString())
System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\reduce_IGX_corrMatr.csv", sb.ToString())

// --------------------  print values ---------------------

//let casesSubset = expCases.Skip 0 |> Seq.take 1

let sb3 = new System.Text.StringBuilder()
names |> Seq.iter (fun (s:string) -> sb3.AppendFormat(@"{0};", s) |> ignore)
sb3.AppendLine("")

for round in 0..22 do    
//for round in 0..2 do    
    let casesSubset = expCases.Skip (round*11) |> Seq.take 11
    let valuesMatrix = data casesSubset
    let vals = valuesMatrix.ToArray()
    valuesMatrix |> Seq.iter (fun (vals:float[]) ->
        for f in vals do
            sb3.AppendFormat(@"{0};", f) |> ignore
        sb3.AppendLine("") |> ignore
        )




sb3.ToString()


System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\convolution.csv", sb3.ToString())
System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\reduce.csv", sb3.ToString())
System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\saxpy.csv", sb3.ToString())




// mode = 1, n_device=1, 0|1



