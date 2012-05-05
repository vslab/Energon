namespace Energon.Measuring

open System
open System.Collections.Generic

type ExperimentRun(sensors:seq<GenericSensor>) as self =
    let mutable startTime = DateTime.MinValue
    let mutable endTime = DateTime.MinValue
    let mutable running = false
    let mutable push = false
    let mutable dt = 1000.
    let results = new Dictionary<GenericSensor, Reading[] >()
    let currentData = new Dictionary<GenericSensor, Queue<Reading>>()
    let means = new Dictionary<GenericSensor, float*float>()
    let timer = new System.Timers.Timer(dt)
    let newReadingEvent = new Event<ExperimentRun * GenericSensor * Reading>()
    let experimentRunStarting = new Event<ExperimentRun>()
    let experimentRunStopping = new Event<ExperimentRun>()

    let pullValues() =
        printfn "pull Values %d %d" (DateTime.Now.Second) (DateTime.Now.Millisecond)
        let pullSensorVal (s:GenericSensor) = 
            let q = currentData.[s] 
            let v = (s:?> PullSensor).CurrValue()
            let eventArg = (self, s, v)
            q.Enqueue(v)
            newReadingEvent.Trigger eventArg
        Seq.iter pullSensorVal sensors
    do
        timer.AutoReset <- true
        timer.Enabled <- false
        timer.Elapsed.Add(fun _ -> pullValues())

    let mutable id = 0

    let mutable (handler:Reading->unit) = fun (r:Reading) -> ()

    [<CLIEvent>]
    member this.NewReadingEvent = newReadingEvent.Publish

    [<CLIEvent>]
    member this.ExperimentRunStartingEvent = experimentRunStarting.Publish

    [<CLIEvent>]
    member this.ExperimentRunStoppingEvent = experimentRunStopping.Publish

    member x.ID
        with get() = id
        and set(v) = id <- v

    /// ignored after Start(true) has been invoked
    /// default value is 1000
    ///</summary>
    member x.TimeInterval
        with get() = dt
        and set(v) = dt <- v

    member x.Sensors
        with get() = sensors

    ///<summary> Date and Time of the start of the experiment
    ///</summary>
    member x.StartTime
        with get() = startTime

    ///<summary> Date and Time of the end of the experiment
    ///</summary>
    member x.EndTime
        with get() = endTime

    ///<summary> true if the experiment is running
    ///</summary>
    member x.Running 
        with get() = running

    ///<summary> Open all the sensors
    ///</summary>
    member x.OpenSensors() =
        Seq.iter (fun (s:GenericSensor) -> s.Open()) sensors

    ///<summary> Starts the experiment
    /// a Push experiment rely on the Sensors for gathering data
    /// every sensor will have a different time interval
    /// a Pull experiment will explicitly pull data from sensors
    /// using a fixed time interval, all the readings will have similar timestamps
    /// GetResultsCompactForm will not calculate averages
    ///</summary>
    member x.Start(?isPush) =
        push <- defaultArg isPush false
        startTime <- DateTime.Now
        Seq.iter (fun (s:GenericSensor) -> s.Reset()) sensors
        Seq.iter (fun (s:GenericSensor) -> s.Start()) sensors
        running <- true
        experimentRunStarting.Trigger(self)
        if not push then
            timer.Interval <- dt
            timer.Enabled <- true
            currentData.Clear()
            Seq.iter (fun s -> currentData.Add(s, new Queue<Reading>())) sensors
        else
            let handleSensor (s:GenericSensor) =
                handler <- fun (r:Reading) -> newReadingEvent.Trigger(self, s, r)
                match s with
                | :? PushSensor as ps -> 
                    ps.NewValue.Add(handler)
                | :? RemoteSensor as ps -> 
                    ps.NewValue.Add(handler)
                | _ -> ()
                ()
            sensors |> Seq.iter handleSensor

    ///<summary> End the experiment
    ///</summary>
    member x.Stop() =
        endTime <- DateTime.Now
        running <- false
        Seq.iter (fun (s:GenericSensor) -> s.Stop()) sensors
        results.Clear()
        experimentRunStopping.Trigger(self)
        if push then
            Seq.iter (fun (s:GenericSensor) -> 
                match s with
                | :? PushSensor as ps -> results.Add(s, ps.Results )
                | :? RemoteSensor as rems -> results.Add(s, rems.Results )
                | :? PullSensor as ps -> ()
                | _ -> () // TODO: handle pull sensors
                ) sensors
        else
            timer.Enabled <- false
            Seq.iter (fun (s:GenericSensor) -> results.Add(s, currentData.[s].ToArray() ) ) sensors
        let meanAndStdDevReading numSeq = 
            if Seq.isEmpty numSeq then
                (0.,0.)
            else
                try
                    let sqr (x:float) = x * x
                    let mean = 
                        numSeq |> Seq.map (fun (r:Reading) -> r.Value) |> Seq.average
                    let variance = 
                        numSeq |> Seq.map (fun (r:Reading) -> r.Value) |> Seq.averageBy (fun x -> sqr(x - mean))
                    (mean, sqrt(variance))
                with
                | _ -> (0.0,0.0)
        // means and std. dev.
        sensors |> Seq.iter (fun s -> means.Add(s, meanAndStdDevReading results.[s])) 
        
        let handleSensor (s:GenericSensor) =
                match s with
                | :? PushSensor as ps -> 
                    ps.ResetHandlers()
                | :? RemoteSensor as ps -> 
                    ps.ResetHandlers()
                | _ -> ()
                ()
        sensors |> Seq.iter handleSensor



        
    ///<summary> Close all the sensors
    ///</summary>
    member x.CloseSensors() =
        if running then x.Stop()
        Seq.iter (fun (s:GenericSensor) -> s.Close()) sensors

    ///<summary> The Means and StdDev of measurement taken during the experiment as a Dictionary where Sensors are the keys and values are couples of (mean, stddev)
    ///</summary>
    member x.MeansAndStdDev
        with get() = means
    
    ///<summary> The reading taken during the experiment as a Dictionary where Sensors are the keys and values are lists of Readings
    ///</summary>
    member x.Results
        with get() = results

    ///<summary> Transforms the Results in order to have uniform time intervals. 
    /// If the experiment is Push the returned values are averages 
    /// (if the requested time interval is higher than the one used by the sensor)
    /// or copies of the same value 
    /// (if the requested time interval is smaller that the one used by the sensor).
    /// uses 0 as Reading.Value if no Reading is available (and no previous readings available) 
    /// fot that time interval.
    /// If the experiment is Pull these are the same values as Results
    ///</summary>
    member x.GetResultsCompactForm(dt) =
        if push then
            // calculates the average value of the readings in the specified interval
            // TODO: I should handle errors here...
            let rec getAvgReadingInTimeInterval sensor tstart =
                if tstart >= startTime then
                    let filtered = results.[sensor] |> Array.filter (fun r -> r.Timestamp >= tstart && r.Timestamp < tstart.AddMilliseconds(dt))
                    if filtered.Length = 0 then
                        getAvgReadingInTimeInterval sensor (tstart.AddMilliseconds(-dt)) // take the previous
                    else
                        let avg = Array.averageBy (fun (r:Reading) -> r.Value) filtered
                                in  new Reading(tstart, sensor.DataType, avg, null)               
                else
                    new Reading(tstart, sensor.DataType, 0., null)
                
            // makes the list with average values using a fixed time interval
            let makeAverageList sensor =
                let rec f tstart =
                    let r = getAvgReadingInTimeInterval sensor tstart
                    if tstart.AddMilliseconds(dt) > endTime then
                        [ r ]
                    else
                        r :: (f (tstart.AddMilliseconds(dt)))
                f startTime
            let sensorsList = Seq.toList sensors
            let readingsCount = (makeAverageList (sensorsList.[0])).Length
            let resultMatrix = Array2D.zeroCreate<Reading> (sensorsList.Length) readingsCount
            for i in 0..(sensorsList.Length-1) do
                let resList = makeAverageList (List.nth sensorsList i)
                for j in 0..(readingsCount-1) do
                    resultMatrix.[i,j] <- resList.[j]
            (sensorsList, resultMatrix)
        else
            let sensorsList = Seq.toList sensors
            let readingsCount = currentData.[sensorsList.Head].Count
            let resultMatrix = Array2D.zeroCreate<Reading> (sensorsList.Length) readingsCount
            for i in 0..(sensorsList.Length-1) do
                let resList = currentData.[(List.nth sensorsList i)].ToArray()
                for j in 0..(readingsCount-1) do
                    resultMatrix.[i,j] <- resList.[j]            
            (sensorsList, resultMatrix)
        


   