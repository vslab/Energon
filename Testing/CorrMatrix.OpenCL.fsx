#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#r @"Energon.Measuring.dll"
open Energon.Measuring

open System.Drawing

let testGetCorrMatrix (data:seq<float array>) =
    let dataAsArray = Seq.toArray data
    let firstRow = dataAsArray.[0]
    let cols = firstRow.Length
    let rows = dataAsArray.Length
    let matr = Array2D.init rows cols (fun i j -> dataAsArray.[i].[j])
    matr
    (*
    let col c = seq {
            for i in 0..(rows-1) do
                yield dataAsArray.[i].[c]
        }
    Array2D.init cols cols (fun i j -> Seq.average (col j))
    *)
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
db.Experiments.Where(fun (x:Experiments) -> x.Id = 7).First()
db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
        if x.Experiment_id = 7 then
            let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
            let mode = args.[0]
            let ndev = args.[5]
            let dev = args.[6]
            let mem = args.[7]
            if mode = "OPENCL" && ndev = "1" && dev = "1" && mem = "2" then
                true
            else
                false
        else
            false    
        ).First()
db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
        if x.Experiment_id = 7 then
            let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
            let mode = args.[0]
            let ndev = args.[5]
            let dev = args.[6]
            let mem = args.[7]
            if mode = "OPENCL" && ndev = "1" && dev = "1"  then
                true
            else
                false
        else
            false    
        ).ToArray()



let createMatrix alg mode memoryIN memoryOUT expID =
    let exp = db.Experiments.Where(fun (x:Experiments) -> x.Id = expID)

    let expCases = 
      match (alg,mode) with 
      | "convolution", "DGPU" -> db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
        if x.Experiment_id = expID then
            let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
            let mode = args.[0]
            let ndev = args.[4]
            let dev = args.[5]
            let memin = args.[6]
            let memout = args.[7]
            if mode = "DEVICE" && ndev = "1" && dev = "Cypress" && memin.Contains(memoryIN) && memout.Contains(memoryOUT) then
                true
            else
                false
        else
            false    
        )
      | "convolution", "IGX" -> db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
        if x.Experiment_id = expID then
            let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
            let mode = args.[0]
            let ndev = args.[4]
            let dev = args.[5]
            let memin = args.[6]
            let memout = args.[7]
            if mode = "DEVICE" && ndev = "1" && dev = "BeaverCreek" &&  memin.Contains(memoryIN) && memout.Contains(memoryOUT) then
                true
            else
                false
        else
            false
        )
      | "saxpy", "DGPU" -> db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
        if x.Experiment_id = expID then
            let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
            let mode = args.[0]
            let ndev = args.[4]
            let dev = args.[5]
            let memin = args.[6]
            let memout = args.[7]
            if mode = "DEVICE" && ndev = "1" && dev = "Cypress" &&  memin.Contains(memoryIN) && memout.Contains(memoryOUT) then
                true
            else
                false
        else
            false    
        )
      | "saxpy", "IGX" -> db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
        if x.Experiment_id = expID then
            let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
            let mode = args.[0]
            let ndev = args.[4]
            let dev = args.[5]
            let memin = args.[6]
            let memout = args.[7]
            if mode = "DEVICE" && ndev = "1" && dev = "BeaverCreek" &&  memin.Contains(memoryIN) && memout.Contains(memoryOUT) then
                true
            else
                false
        else
            false
        )
      | "reduce", "DGPU" -> db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
        if x.Experiment_id = expID then
            let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
            let mode = args.[0]
            let ndev = args.[4]
            let dev = args.[5]
            let memin = args.[6]
            let memout = args.[7]
            if mode = "DEVICE" && ndev = "1" && dev = "Cypress" && memin.Contains(memoryIN) && memout.Contains(memoryOUT) then
                true
            else
                false
        else
            false    
        )
      | "reduce", "IGX" -> db.ExperimentCases.Where(fun (x:ExperimentCases) -> 
        if x.Experiment_id = expID then
            let args = x.Args.Split([| ";" |], StringSplitOptions.RemoveEmptyEntries)
            let mode = args.[0]
            let ndev = args.[4]
            let dev = args.[5]
            let memin = args.[6]
            let memout = args.[7]
            if mode = "DEVICE" && ndev = "1" && dev = "BeaverCreek" &&  memin.Contains(memoryIN) && memout.Contains(memoryOUT) then
                true
            else
                false
        else
            false
        )


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
        let run = (expRuns case).First() // TODO
        let s,v = handleRun run
        let args = case.Args.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
        let firstArg = match args.[0] with
                        | "HOST" -> 0.
                        | "DEVICE" -> 1.
                        | _ -> -1.
        let memArg (a:System.String) = if a.StartsWith("DEVICE COPY") then 0.
                                       else 
                                            if a.StartsWith("CL_MEM_ALLOC_HOST_PTR") then 1.
                                            else 
                                                if a.StartsWith("CL_MEM_USE_PERSISTENT_MEM_AMD") then 2.
                                                else -1.0
        let argsToFloatSeq = seq {
                yield firstArg
                yield float(System.Single.Parse(args.[1]))
                yield float(System.Single.Parse(args.[2]))
                yield float(System.Single.Parse(args.[3]))
                yield float(System.Single.Parse(args.[4]))
                yield 0.0
                yield memArg (args.[6])
                yield memArg (args.[7])
                yield 0.0
                yield memArg (args.[9])
                yield memArg (args.[10])
                yield 0.0
                yield memArg (args.[12])
                yield memArg (args.[13])
            }
        let argArray = argsToFloatSeq.ToArray()
        let valArray = v.ToArray()
        let j = valArray.[0] * valArray.[1]
        let name1 = "completionTime"
        let names = [|  "GPUTime" ; "gpuBusy" ; "aluInsts" ; "fetchInsts" ; "wrInsts" ; "waveFronts" ; "AluBusy" ; "aluFetchRatio" ; 
                "aluPacking" ; "fetchUnitBusy" ; "fetchUnitStalled" ; "fetchSize" ; "cacheHit" ; "writeUnitStalled" ; 
                "ldsFetchInst" ; "ldsWriteInst" ; "aluStalledByLds" ; "ldsBankConfl" ; "fastPath" ; "completePath" ; "pathUtil" |]

        let valSeq = seq {
                yield valArray.[0] //completion time
                for it in 0..2 do
                    for n in 0..(names.Length - 1) do
                        let thisv = valArray.[1+it*(names.Length)+n]
                        let thisgputime = valArray.[1+it*(names.Length)]
                        match n with
                        | 1 -> yield thisv * thisgputime
                        | 6 -> yield thisv * thisgputime
                        | 9 -> yield thisv * thisgputime
                        | 10 -> yield thisv * thisgputime
                        | 13 -> yield thisv * thisgputime
                        | 16 -> yield thisv * thisgputime
                        | _ -> yield thisv 
            }
        let valArray2 = valSeq.ToArray()

        match alg with
        | "convolution" -> 
            let mat_size = argArray.[1] * argArray.[2]
            Array.concat [| argArray; valArray2; [| j ; mat_size |] |]
        | _ -> Array.concat [| argArray; valArray2; [| j; argArray.[1] |] |]
        //let mat_size = argArray.[1] * argArray.[2]
        //Array.concat [| argArray; valArray; [| j ; mat_size |] |]

    let data cases = cases |> Seq.map (fun (c:ExperimentCases) -> (handleCase c))

    let colNames (e:Experiments) (c:ExperimentCases) (r:ExperimentRuns)=
        let args = e.ArgNames.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
        let run = (expRuns c).First() // TODO
        let sensors,v = handleRun run
        //let sensors = db.Sensors.Where(fun (s:Sensors) -> s.Experiment_run_id = r.Id)
        let sensorName (s:Sensors) = 
            db.SensorClasses.Where(fun (cl:SensorClasses) -> cl.Id = s.Sensor_class_id).First().SensorName

        let sensorsNames = (Seq.map sensorName sensors).ToArray()
        Array.concat [| args ; sensorsNames ; [|"J"; "vector_size"|] |]

    let firsExp = exp.First()
    let firstCase = expCases.First()
    let names = colNames firsExp firstCase (Seq.head (expRuns (firstCase)))

    let cols = names.Count()

    let colIdx name = 
        let mutable res = -1
        let namesL = names.Length - 1 
        for i in 0..namesL do
            if name = names.[i] then
                res <- i
        res

    let colName idx = names.[idx]

    let casesSubset = expCases
    //let casesSubset = expCases.Skip (round*11) |> Seq.take 11
    let valuesMatrix = data casesSubset
    let vals = valuesMatrix.ToArray()
    let corrMatr = getCorrMatrix vals
    let corrMatr2 = testGetCorrMatrix vals
    let rows = names.Length - 1
    //let interestingIdx = [| 1;2;7;8;9;16;17;18;19;20;21;22;23;24;25;26;27;28;29;30;31;32;33;34;35;36;37;38 |]
    //let interestingIdx = [| 1;2;7;8;9;16;17;18;22;23;26;27;28;29;30;35 |]
    //let interestingIdx = [| 1..36 |]
    (*let interestingIdx = match mode with
        | "DGPU" -> [| 
            colIdx "vector_size"; 
            colIdx "extechWatt"; 
            colIdx "completionTime"; 
            colIdx "J"; 
            colIdx "d0_mode_in"; 
            colIdx "d0_mode_out"; 
            colIdx "gpuBusy_1"; 
            colIdx "waveFronts_1"; 
            colIdx "AluBusy_1"; 
            colIdx "aluPacking_1"; 
            colIdx "fetchUnitBusy_1"; 
            colIdx "fetchUnitStalled_1"; 
            colIdx "fastPath_1"; 
            |]
        | "IGX" -> [| 
            colIdx "vector_size"; 
            colIdx "extechWatt"; 
            colIdx "completionTime"; 
            colIdx "J"; 
            colIdx "d0_mode_in"; 
            colIdx "d0_mode_out"; 
            colIdx "gpuBusy_2"; 
            colIdx "waveFronts_2"; 
            colIdx "AluBusy_2"; 
            colIdx "aluPacking_2"; 
            colIdx "fetchUnitBusy_2"; 
            colIdx "fetchUnitStalled_2"; 
            colIdx "fastPath_2"; 
            |]
    *)
    let interestingIdx = [| 1..(rows-1) |]
    let sb = new System.Text.StringBuilder()
    sb.Append(" ;") |> ignore
    //names |> Seq.iter (fun (s:string) -> sb.AppendFormat(@"{0};", s) |> ignore)
    interestingIdx |> Seq.iter (fun i -> sb.AppendFormat(@"{0};", colName (i)) |> ignore)

    for i in interestingIdx do
        sb.AppendLine("") |> ignore
        sb.AppendFormat(@"{0};", colName (i)) |> ignore
        for j in interestingIdx do
            sb.AppendFormat(@"{0};", corrMatr.[i,j]) |> ignore


    let filename = String.Format(@"C:\Users\root\Desktop\Energon\Measures\{0}_{1}_{2}_{3}_full_lin.csv", alg, mode,  memoryIN, memoryOUT)
    System.IO.File.WriteAllText(filename, sb.ToString())

    let sb2 = new System.Text.StringBuilder()
    sb2.Append(" ;") |> ignore
    //names |> Seq.iter (fun (s:string) -> sb.AppendFormat(@"{0};", s) |> ignore)
    interestingIdx |> Seq.iter (fun i -> sb2.AppendFormat(@"{0};", colName (i)) |> ignore)
    
    let rows = corrMatr2.GetLength(0)
    for i in 0..(rows-1) do
        sb2.AppendLine("") |> ignore
        sb2.AppendFormat(@" ;") |> ignore
        //sb.AppendFormat(@"{0};", colName (i)) |> ignore
        for j in interestingIdx do
            sb2.AppendFormat(@"{0};", corrMatr2.[i,j]) |> ignore

    let filename2 = String.Format(@"C:\Users\root\Desktop\Energon\Measures\data_{0}_{1}_{2}_{3}_full_lin.csv", alg, mode, memoryIN, memoryOUT)
    System.IO.File.WriteAllText(filename2, sb2.ToString())

//DEVICE;16777216;103;0;3;Cypress; CL_MEM_USE_PERSISTENT_MEM_AMD ; CL_MEM_ALLOC_HOST_PTR ;BeaverCreek;CL_MEM_USE_PERSISTENT_MEM_AMD;CL_MEM_ALLOC_HOST_PTR;AMD_A8_3800_APU_with_Radeon_tm__HD_Graphics;CL_MEM_USE_PERSISTENT_MEM_AMD;CL_MEM_ALLOC_HOST_PTR;
//DEVICE;4194304;455;0;3;Cypress;CL_MEM_ALLOC_HOST_PTR;CL_MEM_ALLOC_HOST_PTR;BeaverCreek;CL_MEM_ALLOC_HOST_PTR;CL_MEM_ALLOC_HOST_PTR;AMD_A8_3800_APU_with_Radeon_tm__HD_Graphics;CL_MEM_ALLOC_HOST_PTR;CL_MEM_ALLOC_HOST_PTR;
// DEVICE;67108864;31;0;3;Cypress;DEVICE COPY;DEVICE COPY;BeaverCreek;DEVICE COPY;DEVICE COPY;AMD_A8_3800_APU_with_Radeon_tm__HD_Graphics;DEVICE COPY;DEVICE COPY;
createMatrix "convolution" "IGX" "DEVICE COPY" "DEVICE COPY" 32
createMatrix "convolution" "IGX" "CL_MEM_ALLOC_HOST_PTR" "CL_MEM_ALLOC_HOST_PTR" 32
createMatrix "convolution" "IGX" "CL_MEM_USE_PERSISTENT_MEM_AMD" "CL_MEM_ALLOC_HOST_PTR" 32
createMatrix "convolution" "DGPU" "DEVICE COPY" "DEVICE COPY" 32
createMatrix "convolution" "DGPU" "CL_MEM_ALLOC_HOST_PTR" "CL_MEM_ALLOC_HOST_PTR" 32
createMatrix "convolution" "DGPU" "CL_MEM_USE_PERSISTENT_MEM_AMD" "CL_MEM_ALLOC_HOST_PTR" 32

createMatrix "saxpy" "IGX" "0" "0"


for mode in [| "DGPU"; "IGX" |] do
    createMatrix "convolution" mode "0" "0"
    createMatrix "convolution" mode "1" "1"
    createMatrix "convolution" mode "2" "2"
    createMatrix "saxpy" mode "0" "0"
    createMatrix "saxpy" mode "1" "1"
    createMatrix "saxpy" mode "1" "2"
    createMatrix "reduce" mode "0" "0"
    createMatrix "reduce" mode "1" "1"
    createMatrix "reduce" mode "1" "2"



1+1


let b = new Bitmap(1024, 1024)
let g = Graphics.FromImage(b)
let p = new Pen(Color.Black)
let brush = new SolidBrush(Color.Black)
let f = new Font("Arial", 24.0F, FontStyle.Bold)
g.DrawString("test", f, brush, 1.0F, 100.0F) 
g.DrawRectangle(p, 100, 100, 900, 900)
g.Dispose()
b.Save(@"C:\Users\root\Desktop\Energon\Measures\bitmap.png")



sb.ToString()


//System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\convolution_correlations.csv", sb.ToString())
//System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\convolution_correlations_text.csv", sb2.ToString())

System.IO.File.WriteAllText(@"C:\Users\root\Desktop\Energon\Measures\corrMatr.csv", sb.ToString())

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



