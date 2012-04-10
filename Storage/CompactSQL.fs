module Energon.CompactSQL

(*
#I @"C:\Program Files (x86)\Microsoft SQL Server Compact Edition\v4.0\Desktop"
#r "System.Data.SqlServerCe.dll"

#I @"..\Measuring\bin\Debug"
#r "Energon.Measuring.dll"

#I @"..\SqlCompactDb\Measurement\bin\Debug"
#r "Energon.Measurement.dll"

#r "System.Data.Linq.dll"
#r "System.Linq.dll"

#r "FSharp.PowerPack.Linq.dll"
#r "FSharp.Data.TypeProviders.dll"
*)

open System
open Microsoft.FSharp.Data.TypeProviders
open System.Data.Linq.SqlClient
open System.Linq
open Microsoft.FSharp.Linq
open System.Data.Linq
open System.Data.SqlServerCe;
open Energon.Measuring
open System.Text
open Energon.SQLCE

 
let getConStr file = 
    let conStr = "Data Source=" + file + ";" in
    conStr;

let GetLinqContext file = 
    let context = new Measurements(getConStr file)
    if (context.DatabaseExists() = false) then
         context.CreateDatabase()
    context

let SaveExperiment (exp:Energon.Measuring.Experiment) file =
    let db = GetLinqContext file
    // create an experiment linq item
    let experiment = new Energon.SQLCE.Experiments()
    experiment.Name <- exp.Name
    experiment.Note <- exp.Note
    experiment.Iter <- new Nullable<int>( exp.IterCount)
    db.Experiments.InsertOnSubmit(experiment)
    db.SubmitChanges()
    // now handle sensors
    let handleSensor (s:Energon.Measuring.GenericSensor) =
        // find sensor class with same name
        let sensorclasses = db.SensorClasses.Where(fun (x:SensorClasses) -> x.SensorName.ToLower() = s.Name.ToLower() )
        if (sensorclasses.Count() > 0) then
            // sensor class already present
            sensorclasses.First()
        else
            // create one
            let newsensorclass = new Energon.SQLCE.SensorClasses()
            newsensorclass.SensorName <- s.Name
            db.SensorClasses.InsertOnSubmit(newsensorclass)
            db.SubmitChanges()
            newsensorclass
    let sensors = Seq.map (fun s1 -> handleSensor s1 ) (exp.Sensors.ToArray())
    let argsToString (args:seq<obj>) = 
        let sb = new StringBuilder()
        Seq.iter2 (fun x y -> sb.AppendFormat("{0}={1},", x, y.ToString()) |> ignore ) exp.ArgNames args
        sb.ToString()
    // for every experiment case
    let saveCase (c:Energon.Measuring.ExperimentCase) =        
        let sensorClassFromSensor (s:Energon.Measuring.GenericSensor) =
            sensors.First(fun x -> x.SensorName.ToLower() = s.Name.ToLower())
        let expCase = new Energon.SQLCE.ExperimentCases()
        expCase.Experiment_id <- experiment.Id
        expCase.Args <- argsToString c.Args
        db.ExperimentCases.InsertOnSubmit(expCase)
        db.SubmitChanges()
        let maxi = c.IterCount - 1
        for i in 0..maxi do
            let first (a,b) = a
            let second (a,b) = b
            // new experiment run
            let run = new Energon.SQLCE.ExperimentRuns()
            run.Experiment_case_id <- expCase.Id
            run.Args <- argsToString c.Args
            run.Start <- new Nullable<DateTime>( first (c.StartStopTimes.[i]))
            run.End <- new Nullable<DateTime>(second (c.StartStopTimes.[i]))
            db.ExperimentRuns.InsertOnSubmit(run)
            // link sensors classes and experiment id 
            let handleSensor (s:GenericSensor) = 
                let sensor = new Energon.SQLCE.Sensors()
                sensor.Experiment_run_id <- run.Id
                let sensorClass = sensorClassFromSensor s
                sensor.Sensor_class_id <- sensorClass.Id
                db.Sensors.InsertOnSubmit(sensor)
                db.SubmitChanges()
                // handle measurements
                let data = c.Results.[s].[i]
                let getLinqValue (d:Reading) =
                    let measurement = new Energon.SQLCE.Measurements1()
                    measurement.Sensor_id <- sensor.Id
                    measurement.Value <- d.Value
                    //measurement.Row <- d.Raw
                    measurement.Timestamp <- d.Timestamp
                    measurement
                db.Measurements1.InsertAllOnSubmit (Seq.map (fun d -> getLinqValue d) data)
                db.SubmitChanges()
            c.Sensors |> Seq.iter handleSensor
    exp.Cases |> Seq.iter saveCase

type ExperimentRuntimeSaver(exp:Experiment, file) as self =
    let db = GetLinqContext file
    let first (a,b) = a
    let second (a,b) = b
    let argsToString (args:seq<obj>) = 
        let sb = new StringBuilder()
        Seq.iter2 (fun x y -> sb.AppendFormat("{0}={1},", x, y.ToString()) |> ignore ) exp.ArgNames args
        sb.ToString()

    let handleSensor (s:Energon.Measuring.GenericSensor) =
        // find sensor class with same name
        let sensorclasses = db.SensorClasses.Where(fun (x:SensorClasses) -> x.SensorName.ToLower() = s.Name.ToLower() )
        if (sensorclasses.Count() > 0) then
            // sensor class already present
            sensorclasses.First()
        else
            // create one
            let newsensorclass = new Energon.SQLCE.SensorClasses()
            newsensorclass.SensorName <- s.Name
            db.SensorClasses.InsertOnSubmit(newsensorclass)
            db.SubmitChanges()
            newsensorclass
    let sensors = Seq.map (fun s1 -> handleSensor s1 ) (exp.Sensors.ToArray())

    let sensorClassFromSensor (s:Energon.Measuring.GenericSensor) =
        sensors.First(fun x -> x.SensorName.ToLower() = s.Name.ToLower())

    let saveExperiment (exp:Experiment) =
        let experiment = new Energon.SQLCE.Experiments()
        experiment.Name <- exp.Name
        experiment.Note <- exp.Note
        experiment.Iter <- new Nullable<int>( exp.IterCount)
        db.Experiments.InsertOnSubmit(experiment)
        db.SubmitChanges()
        exp.ID <- experiment.Id

    let saveExperimentCase (exp:Experiment) (case:ExperimentCase) =
        let expCase = new Energon.SQLCE.ExperimentCases()
        expCase.Experiment_id <- exp.ID
        expCase.Args <- argsToString case.Args
        db.ExperimentCases.InsertOnSubmit(expCase)
        db.SubmitChanges()
        case.ID <- expCase.Id

    let saveExperimentRun (c:ExperimentCase) (r:ExperimentRun) =
        let run = new Energon.SQLCE.ExperimentRuns()
        run.Experiment_case_id <- c.ID
        run.Args <- argsToString c.Args
        run.Start <- new Nullable<DateTime>( r.StartTime )
        //run.End <- new Nullable<DateTime>( r.EndTime )
        db.ExperimentRuns.InsertOnSubmit(run)
        db.SubmitChanges()
        r.ID <- run.Id
        let handleSensor (s:GenericSensor) = 
            let sensor = new Energon.SQLCE.Sensors()
            sensor.Experiment_run_id <- run.Id
            let sensorClass = sensorClassFromSensor s
            sensor.Sensor_class_id <- sensorClass.Id
            db.Sensors.InsertOnSubmit(sensor)
            db.SubmitChanges()
            s.ID <- sensor.Id
        c.Sensors |> Seq.iter handleSensor

    let experimentRunStarting (r:ExperimentRun) =
        let run = db.ExperimentRuns.Where(fun (x:ExperimentRuns) -> x.Id = r.ID).First()
        run.Start <- new Nullable<DateTime>( r.StartTime )
        db.SubmitChanges()

    let experimentRunStopping (r:ExperimentRun) =
        let run = db.ExperimentRuns.Where(fun (x:ExperimentRuns) -> x.Id = r.ID).First()
        run.End <- new Nullable<DateTime>( r.EndTime )
        db.SubmitChanges()

    let saveReading (run:ExperimentRun) (sensor:GenericSensor) (reading:Reading) =
        let r = new Measurements1()
        r.Timestamp <- reading.Timestamp
        r.Sensor_id <- sensor.ID
        r.Value <- reading.Value
        // TODO: RAW
        db.Measurements1.InsertOnSubmit(r)
        db.SubmitChanges()

    let handleReading(exp:Experiment, case:ExperimentCase, run:ExperimentRun, sensor:GenericSensor, reading:Reading) =
        saveReading run sensor reading
    do
        exp.NewReadingEvent.Add(handleReading)
        exp.ExperimentRunStartingEvent.Add(fun (exp, case, run) -> 
            if (exp.ID = 0) then
                saveExperiment exp
            if (case.ID = 0) then
                saveExperimentCase exp case
            if (run.ID = 0) then
                saveExperimentRun case run
            experimentRunStarting run 
            )
        exp.ExperimentRunStoppingEvent.Add(fun (exp, case, run) -> experimentRunStopping run )

    member x.LinqContext
        with get() = db

    member x.CloseDB() =
        db.Dispose()        

