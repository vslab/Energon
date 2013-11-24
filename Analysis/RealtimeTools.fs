

module RealtimeTools

open System
open System.Collections.Generic
open Energon.Measuring


let movingAverageReadingSeq n s =
    Seq.map (fun (r:Reading) -> r.Value) s
    |> Seq.windowed n
    |> Seq.map Array.average 

let movingAverageFloatSeq n s =
    Seq.windowed n (s:seq<float>)
    |> Seq.map Array.average 

let movingAverageFloatEvt n nums = 
    let mem = Array.create n 0.
    let avg (i, _) (x) =
        Array.set mem i x
        ((i+1) % n, Array.average mem)
 
    // The initial vaues of n
    let startValues = (0, 0.)
    // The resulting event stream is a scan over the input events 
    nums 
    |> Event.scan avg startValues
    |> Event.map (fun (n, v) -> v)  

let movingAverageReadingEvt n nums = 
    let mem = Array.create n 0.
    let avg (i, _) (x:Reading) =
        Array.set mem i x.Value
        ((i+1) % n, Array.average mem)
    // The initial vaues of n
    let startValues = (0, 0.)
    // The resulting event stream is a scan over the input events 
    nums
    |> Event.scan avg startValues
    |> Event.map (fun (n, v) -> v)  

let meanAndStdDevFloatSeq (nums:seq<float>) = 
    // A function which updates the on-line computation of 
    // n, mean and M2 given a new datapoint x
    let newValues (n, mean, M2) x = 
        let n' = n+1.
        let delta = x - mean
        let mean' = mean + delta/n'
        let M2' = M2 + delta*(x - mean')
        (n', mean', M2')
    // The initial vaues of n, mean and M2
    let startValues = (0., 0., 0.)
    // The resulting event stream is a scan over the input events 
    nums 
    |> Seq.scan newValues startValues
    |> Seq.map (fun (n, mean, M2) -> (mean, sqrt(M2/(n))))

let meanAndStdDevReadingSeq (nums:seq<Reading>) = 
    // A function which updates the on-line computation of 
    // n, mean and M2 given a new datapoint x
    let newValues (n, mean, M2) (x:Reading) = 
        let n' = n+1.
        let delta = x.Value - mean
        let mean' = mean + delta/n'
        let M2' = M2 + delta*(x.Value - mean')
        (n', mean', M2')
    // The initial vaues of n, mean and M2
    let startValues = (0., 0., 0.)
    // The resulting event stream is a scan over the input events 
    nums 
    |> Seq.scan newValues startValues
    |> Seq.map (fun (n, mean, M2) -> (mean, sqrt(M2/(n))))

/// Compute a running mean and standard deviation over the 
/// input event stream of floating point numbers
let meanAndStdDevFloatEvt (nums:IEvent<'b, float>) =
    // A function which updates the on-line computation of n, mean and M2
    let newValues (n, mean, M2) (x) = 
        let n' = n+1.
        let delta = x - mean
        let mean' = mean + delta/n'
        let M2' = M2 + delta*(x - mean')
        (n', mean', M2')
    // The initial vaues of n, mean and M2
    let startValues = (0., 0., 0.)
    // The resulting event stream is a scan over the input events 
    nums 
    |> Event.scan newValues startValues
    |> Event.map (fun (n, mean, M2) -> (mean, sqrt(M2/(n))))  

/// Compute a running mean and standard deviation over the 
/// input event stream of floating point numbers
let meanAndStdDevReadingEvt (nums:IEvent<'b, Reading>)  = 
    // A function which updates the on-line computation of n, mean and M2
    let newValues (n, mean, M2) (x:Reading) = 
        let n' = n+1.
        let delta = x.Value - mean
        let mean' = mean + delta/n'
        let M2' = M2 + delta*(x.Value - mean')
        (n', mean', M2')
    // The initial vaues of n, mean and M2
    let startValues = (0., 0., 0.)
    // The resulting event stream is a scan over the input events 
    nums 
    |> Event.scan newValues startValues
    |> Event.map (fun (n, mean, M2) -> (mean, sqrt(M2/(n))))  


