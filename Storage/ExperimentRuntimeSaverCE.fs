namespace Energon.Storage

open System
//open Microsoft.FSharp.Data.TypeProviders
open System.Data.Linq.SqlClient
open System.Linq
open Microsoft.FSharp.Linq
open System.Data.Linq
open System.Data.SqlServerCe;
open Energon.Measuring
open System.Text
open Energon.SQLCE
open System.Collections.Generic


type ExperimentRuntimeSaverCE(exp:Experiment, file) =
    let l = ref 0 
    let mutable currExpId = 0
    let mutable currExpCaseId = 0
    let mutable currExpRunId = 0
    let getConStr file = 
        let conStr = "Data Source=" + file + ";" in
        conStr;

    let GetLinqContext file = 
        let context = new Measurements(getConStr file)
        if (context.DatabaseExists() = false) then
             context.CreateDatabase()
        context
    let db = 
        let context = GetLinqContext file
        context.Connection.Open()
        context
    let first (a,b) = a
    let second (a,b) = b
    let argsToString (args:seq<obj>) = 
        let sb = new StringBuilder()
        args |> Seq.iter (fun x -> sb.AppendFormat(@"{0};", x.ToString()) |> ignore )
        sb.ToString()
    let argsNamesToString (args:seq<string>) = 
        let sb = new StringBuilder()
        args |> Seq.iter (fun x -> sb.AppendFormat(@"{0};", x) |> ignore )
        sb.ToString()

    let handleSensor (s:Energon.Measuring.GenericSensor) =
        // find sensor class with same name
        let sensorclasses = db.SensorClasses.Where(fun (x:SensorClasses) -> x.SensorName.ToLower() = s.Name.ToLower() )
        if (sensorclasses.Count() > 0) then
            // sensor class already present
            let s = sensorclasses.First()
            s
        else
            // create one
            let newsensorclass = new Energon.SQLCE.SensorClasses()
            newsensorclass.SensorName <- s.Name
            //lock l (fun () ->
            //let t = db.Connection.BeginTransaction()
            //db.Transaction <- t
            db.SensorClasses.InsertOnSubmit(newsensorclass)
            db.SubmitChanges(ConflictMode.ContinueOnConflict)
            //t.Commit()
            //    )
            newsensorclass
    let sensors = Seq.map (fun s1 -> handleSensor s1 ) (exp.Sensors.ToArray())

    let sensorClassFromSensor (s:Energon.Measuring.GenericSensor) =
        sensors.First(fun x -> x.SensorName.ToLower() = s.Name.ToLower())

    let saveExperiment (exp:Experiment) =
        let experiment = new Energon.SQLCE.Experiments()
        experiment.Name <- exp.Name
        experiment.Note <- exp.Note
        experiment.Iter <- new Nullable<int>( exp.IterCount)
        experiment.ArgNames <- argsNamesToString (exp.ArgNames)
        //lock l (fun () ->
        //let t = db.Connection.BeginTransaction()
        //db.Transaction <- t
        db.Experiments.InsertOnSubmit(experiment)
        db.SubmitChanges(ConflictMode.ContinueOnConflict)
        //t.Commit()
        exp.ID <- experiment.Id

    let saveExperimentCase (exp:Experiment) (case:ExperimentCase) =
        let expCase = new Energon.SQLCE.ExperimentCases()
        expCase.Experiment_id <- exp.ID
        expCase.Args <- argsToString (case.Args)
        //lock l (fun () ->
        //let t = db.Connection.BeginTransaction()
        //db.Transaction <- t
        db.ExperimentCases.InsertOnSubmit(expCase)
        db.SubmitChanges(ConflictMode.ContinueOnConflict)
        //t.Commit()
        case.ID <- expCase.Id

    let saveExperimentRun (c:ExperimentCase) (r:ExperimentRun) =
        let run = new Energon.SQLCE.ExperimentRuns()
        run.Experiment_case_id <- c.ID
        run.Args <- argsToString c.Args
        run.Start <- new Nullable<DateTime>( r.StartTime )
        //run.End <- new Nullable<DateTime>( r.EndTime )
        //let t = db.Connection.BeginTransaction()
        //db.Transaction <- t
        db.ExperimentRuns.InsertOnSubmit(run)
        db.SubmitChanges(ConflictMode.ContinueOnConflict)
        //t.Commit()
        r.ID <- run.Id
        let handleSensor (s:GenericSensor) = 
            let sensor = new Energon.SQLCE.Sensors()
            sensor.Experiment_run_id <- run.Id
            let sensorClass = sensorClassFromSensor s
            sensor.Sensor_class_id <- sensorClass.Id
            //let t = db.Connection.BeginTransaction()
            //db.Transaction <- t
            db.Sensors.InsertOnSubmit(sensor)
            db.SubmitChanges(ConflictMode.ContinueOnConflict)
            //t.Commit()
            s.ID <- sensor.Id
        c.Sensors |> Seq.iter handleSensor
        db.SubmitChanges(ConflictMode.ContinueOnConflict)
        
    let experimentRunStarting (r:ExperimentRun) =
        let run = db.ExperimentRuns.Where(fun (x:ExperimentRuns) -> x.Id = r.ID).First()
        run.Start <- new Nullable<DateTime>( r.StartTime )
        //let t = db.Connection.BeginTransaction()
        //db.Transaction <- t
        db.SubmitChanges(ConflictMode.ContinueOnConflict)
        //t.Commit()

    let readingsList = new List<Experiment*ExperimentCase*ExperimentRun*GenericSensor*Reading>()

    let saveReading (run:ExperimentRun) (sensor:GenericSensor) (reading:Reading) =
        let r = new Measurements1()
        r.Timestamp <- reading.Timestamp
        r.Sensor_id <- sensor.ID
        r.Value <- reading.Value
        // TODO: RAW
        //let t = db.Connection.BeginTransaction()
        //db.Transaction <- t
        try
            db.Measurements1.InsertOnSubmit(r)
            db.SubmitChanges(ConflictMode.ContinueOnConflict)
        with
        | _ -> printf "exception saving measure\n"
        //t.Commit()

    let experimentRunStopping (r:ExperimentRun) =
        let run = db.ExperimentRuns.Where(fun (x:ExperimentRuns) -> x.Id = r.ID).First()
        run.End <- new Nullable<DateTime>( r.EndTime )
        //let t = db.Connection.BeginTransaction()
        //db.Transaction <- t
        db.SubmitChanges(ConflictMode.ContinueOnConflict)
        //t.Commit()
        for (e,c,r,s,read) in readingsList do
            saveReading r s read
        readingsList.Clear()

    let handleReading(exp:Experiment, case:ExperimentCase, run:ExperimentRun, sensor:GenericSensor, reading:Reading) =
        readingsList.Add(exp,case,run,sensor,reading)
        //saveReading run sensor reading
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
        exp.ExperimentRunStoppingEvent.Add(fun (exp, case, run) -> 
            experimentRunStopping run )


    member x.LinqContext
        with get() = db



