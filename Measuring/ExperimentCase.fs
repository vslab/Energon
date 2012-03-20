﻿namespace Energon.Measuring
open System
open System.Collections.Generic

type ExperimentCase(sensors:seq<GenericSensor>, iter:int, args:seq<obj>, load: seq<obj> -> unit, ?waitInterval) =
    let wait = defaultArg waitInterval 1000
    let results = new Dictionary<GenericSensor, List<Reading[]>>()
    let resultsMeans = new Dictionary<GenericSensor, List<float*float>>()
    let means = new Dictionary<GenericSensor, float*float>()
    let startStop = new Queue<DateTime*DateTime>()
    member x.Run() =
        results.Clear()
        Seq.iter (fun s -> results.Add(s, new List<Reading[]>())) sensors
        Seq.iter (fun s -> resultsMeans.Add(s, new List<float*float>())) sensors
        let rec runIter i =
            let exp = new ExperimentRun(sensors)
            exp.Start()
            load(args)
            exp.Stop()
            startStop.Enqueue((exp.StartTime, exp.EndTime))
            Seq.iter (fun s -> results.[s].Add(exp.Results.[s])) sensors
            Seq.iter (fun s-> resultsMeans.[s].Add(exp.MeansAndStdDev.[s])) sensors
            if wait > 0 then
                System.Threading.Thread.Sleep(wait)
            let next = i + 1
            if next < iter then
                runIter next
        runIter 0
        // means and dev std
        // TODO: currently I'm only using means, discarding std devs of experiment runs
        let meanAndStdDevReading numSeq = 
            let sqr (x:float) = x * x
            let mean = 
                numSeq |> Seq.average
            let variance = 
                numSeq |> Seq.averageBy (fun x -> sqr(x - mean))
            (mean, sqrt(variance))
        sensors |> Seq.iter (fun s -> 
            let a = Array.map (fun l -> fst(l)) (resultsMeans.[s].ToArray())
            let m = meanAndStdDevReading a
            in means.Add(s, m))
    ///<summary>
    /// For every sensor and for every run the complete list of Reading taken during all the experiment runs
    ///</summary>
    member x.Results 
        with get() = results
    ///<summary>
    /// Fpr every sensor and for every run the average value and the std dev of the reading taken during every experiment run
    ///</summary>
    member x.ResultsMeansAndStdDev 
        with get() = resultsMeans
    ///<summary>
    /// for every sensor the average value and the standard deviation 
    ///</summary>
    member x.MeansAndStdDev 
        with get() = means
    member x.Sensors
        with get() = sensors
    member x.IterCount
        with get() = iter
    member x.StartStopTimes
        with get() = startStop.ToArray()

