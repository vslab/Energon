namespace Energon.Measuring
open System
open System.Collections.Generic

type Experiment(name:string, sensors:seq<GenericSensor>, iter:int, argNames:seq<string>, args:seq<seq<obj>>, load: seq<obj> -> unit, ?waitInterval) as self =
    let wait = defaultArg waitInterval 1000
    let results = new Dictionary<GenericSensor, List<List<Reading[]>>>()
    let means = new Dictionary<GenericSensor, List<(float*float)>>()
    let mutable push = false
    let mutable note = ""
    let cases = new List<ExperimentCase>()
    let mutable id = 0

    let newReadingEvent = new Event<Experiment * ExperimentCase * ExperimentRun * GenericSensor * Reading>()

    [<CLIEvent>]
    member this.NewReadingEvent = newReadingEvent.Publish

    member x.ID
        with get() = id
        and set(v) = id <- v

    member x.Run(?isPush) =
        push <- defaultArg isPush false
        results.Clear()
        means.Clear()
        cases.Clear()
        sensors |> Seq.iter (fun s -> results.Add(s, new List<List<Reading[]>>()); means.Add(s, new List<(float*float)>()) )
        let runCase (a:seq<obj>) =
            let case = new ExperimentCase(sensors, iter, a, load, wait)
            case.NewReadingEvent.Add(fun (case, run, sensor, reading) ->
                let args = (self, case, run, sensor, reading)
                newReadingEvent.Trigger(args)
                )
            cases.Add(case)
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
    member x.Name
        with get() = name
    member x.Note
        with get() = note
        and set(v) = note <- v
    member x.ArgNames
        with get() = argNames
    member x.Args
        with get() = args
    member x.Cases
        with get() = cases


