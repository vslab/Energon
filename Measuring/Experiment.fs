namespace Energon.Measuring
open System
open System.Collections.Generic

type Experiment(sensors:seq<GenericSensor>, iter:int, argNames:seq<string>, args:seq<seq<obj>>, load: seq<obj> -> unit, ?waitInterval) =
    let wait = defaultArg waitInterval 1000
    member x.Run() =
        let runCase (a:seq<obj>) =
            let case = new ExperimentCase(sensors, iter, a, load, wait)
            case.Run()
        args |> Seq.iter (function i -> runCase i) 
        
    ///<summary>
    /// For every sensor and for every run the complete list of Reading taken during all the experiment runs
    ///</summary>
    member x.Results 
        with get() = ignore
    ///<summary>
    /// Fpr every sensor and for every run the average value and the std dev of the reading taken during every experiment run
    ///</summary>
    member x.ResultsMeansAndStdDev 
        with get() = ignore
    ///<summary>
    /// for every sensor the average value and the standard deviation 
    ///</summary>
    member x.MeansAndStdDev 
        with get() = ignore
    member x.Sensors
        with get() = sensors
    member x.IterCount
        with get() = iter


