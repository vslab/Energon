namespace Energon.Measuring.Database

open System
open System.Collections.Generic
open Energon.Measuring


/// a sensors, not usable, just a dummy representation from rthe database
type DatabaseSensor(name:string, id:int) as self = 
    inherit GenericSensor(name, DataType.Unknown)
    do
      self.ID <- id

    override x.Start() =
        raise (System.InvalidOperationException())

    override x.Stop() = 
        raise (System.InvalidOperationException())

    override x.Open() = 
        raise (System.InvalidOperationException())

    override x.Close() = 
        raise (System.InvalidOperationException())

    override x.Reset() = 
        raise (System.InvalidOperationException())


/// a database run read from database
type DatabaseExperimentRun(sensors:seq<GenericSensor>, id:int) as self =
    inherit ExperimentRun(sensors)
    do
      self.ID <- id

    let mutable startTime = DateTime.Now
    let mutable endTime = DateTime.Now

    let readings = new List<Reading>()

    member x.Readings
        with get() = readings

    ///<summary> Date and Time of the start of the experiment
    ///</summary>
    member x.StartTime
        with get() = startTime
        and set(v) = startTime <- v

    ///<summary> Date and Time of the end of the experiment
    ///</summary>
    member x.EndTime
        with get() = endTime
        and set(v) = endTime <- v



type DatabaseExperimentCase(sensors:seq<GenericSensor>, iter:int, args:seq<obj>, id:int) as self =
    inherit ExperimentCase(sensors, iter, args, (fun (x:seq<obj>) -> ()))
    do
      self.ID <- id

    let experimentRuns = List<DatabaseExperimentRun>()

    member x.ExperimentRuns
        with get() = experimentRuns

    member x.Run() =
        raise (System.InvalidOperationException())

    member x.Results 
        with get() = raise (System.NotImplementedException())
    ///<summary>
    /// For every sensor and for every run the average value and the std dev of the reading taken during every experiment run
    ///</summary>
    member x.ResultsMeansAndStdDev 
        with get() = raise (System.NotImplementedException())
    ///<summary>
    /// for every sensor the average value and the standard deviation 
    ///</summary>
    member x.MeansAndStdDev 
        with get() = raise (System.NotImplementedException())


type DatabaseExperiment(name:string, sensors:seq<DatabaseSensor>, iter:int, argNames:seq<string>, args:seq<seq<obj>>, id:int) as self=
    inherit Experiment(name, Seq.map (fun (x:DatabaseSensor) -> x :> GenericSensor) sensors, iter, argNames, args, (fun (x:seq<obj>) -> ()))
    do
      self.ID <- id

    let experimentCases = new List<DatabaseExperimentCase>()

    member x.ExperimentCases
        with get() = experimentCases

    member x.Run() = 
        raise (System.InvalidOperationException())

    member x.Results 
        with get() = raise (System.NotImplementedException())
