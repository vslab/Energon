module Tools

open Energon.Measuring

let MovingAverageFloat n (s:float seq) =
    Seq.windowed n s
    |> Seq.map Array.average 

let MovingAverageReading n (s:Reading seq) =
    Seq.map (fun (r:Reading) -> r.Value) s
    |> Seq.windowed n
    |> Seq.map Array.average 

/// Compute the standard deviation of a sequence of numbers
let meanAndStdDevFloat numSeq = 
    let sqr (x:float) = x * x
    let mean = 
        numSeq |> Seq.average
    let variance = 
        numSeq |> Seq.averageBy (fun x -> sqr(x - mean))
    (mean, sqrt(variance))

let meanAndStdDevReading numSeq = 
    let sqr (x:float) = x * x
    let mean = 
        numSeq |> Seq.map (fun (r:Reading) -> r.Value) |> Seq.average
    let variance = 
        numSeq |> Seq.map (fun (r:Reading) -> r.Value) |> Seq.averageBy (fun x -> sqr(x - mean))
    (mean, sqrt(variance))


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