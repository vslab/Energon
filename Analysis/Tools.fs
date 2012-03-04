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
