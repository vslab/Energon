namespace Energon.Measuring
open System
open System.Collections.Generic

type Experiment(sensors:seq<GenericSensor>, iter:int, argNames:seq<string>, args:seq<seq<obj>>, load: seq<obj> -> unit, ?waitInterval) =
    let wait = defaultArg waitInterval 1000
    let results = new Dictionary<GenericSensor, List<List<Reading[]>>>()
    let means = new Dictionary<GenericSensor, List<(float*float)>>()
    member x.Run() =
        results.Clear()
        means.Clear()
        sensors |> Seq.iter (fun s -> results.Add(s, new List<List<Reading[]>>()); means.Add(s, new List<(float*float)>()) )
        let runCase (a:seq<obj>) =
            let case = new ExperimentCase(sensors, iter, a, load, wait)
            case.Run()
            sensors |> Seq.iter (fun s -> results.[s].Add(case.Results.[s]);means.[s].Add(case.MeansAndStdDev.[s]) )
        args |> Seq.iter runCase 
        
    ///<summary>
    /// For every sensor and for every run the complete list of Reading taken during all the experiment runs
    ///</summary>
    member x.Results 
        with get() = results
    ///<summary>
    /// for every sensor the average value and the standard deviation 
    ///</summary>
    member x.MeansAndStdDev 
        with get() = means
    member x.Sensors
        with get() = sensors
    member x.IterCount
        with get() = iter


