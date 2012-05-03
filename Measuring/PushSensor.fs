namespace Energon.Measuring

open System
open System.Threading
  
type PushSensor(name:string, valueType, hz) = 
    inherit PullSensor(name, valueType)
    let mutable newValue = new Event<Reading>()
    let mutable size = 0
    let mutable error = 0.
    let mutable running = false
    /// the last consolidated value
    let mutable lastv = new Reading(DateTime.Now, valueType, 0., null)
    let mutable samplingRate = hz 
    let mutable accumulator = 0. 
    let mutable samplescount = 0. 
    let interval() = float(1. / samplingRate) * 1000.
    let mutable startInterval = System.DateTime.Now 
    let buffer = new System.Collections.Generic.Queue<Reading>() 
    /// the last reading from the sensor
    let mutable lastr = new Reading(DateTime.Now, valueType, 0., null)  
    let timer = new System.Timers.Timer(interval())

    let checkTime() =
        let dt = interval() 
        let t = System.DateTime.Now 
        let elapsed = (t - startInterval).TotalMilliseconds
        if elapsed > float(dt) then 
            if samplescount > 0. then 
                lastv <- new Reading(t, valueType, (accumulator / samplescount), null)
            if size > 0 && buffer.Count = size then
                buffer.Dequeue() |> ignore
            buffer.Enqueue(lastv)
            startInterval <- t 
            samplescount <- 0.
            accumulator <- 0.
            newValue.Trigger(lastv)
    //timer.Interval <- dt - elapsed


    do
        timer.AutoReset <- true
        timer.Enabled <- false
        timer.Elapsed.Add(fun _ -> checkTime())
  
    [<CLIEvent>]
    member this.NewValue = newValue.Publish

    member this.ResetHandlers() =
        newValue <- new Event<Reading>()

    ///<summary> Starts acquiring data.
    ///</summary>
    override u.Start() = 
        base.Start()
        timer.Enabled <- true

    ///<summary> Stops acquiring data
    ///</summary>
    override u.Stop() = 
        base.Stop()
        timer.Enabled <- false

    abstract member PushValue: Reading -> unit
    default x.PushValue v = 
        lastr <- v 
        accumulator <- float(lastr.Value) + accumulator 
        samplescount <- samplescount + 1. 
        checkTime()

    member x.lastValue
        with get() = lastr

    ///<summary> Get or Set the buffer size.
    /// When the buffer reaches the limit discards older Readings.
    /// W size of 0 means there is no limit.
    /// Default value is 0.
    ///</summary>
    member x.BufferSize
        with get() = size
        and set(v) = size <- v

    ///<summary> The sample rate of this sensor
    ///</summary>
    member x.SamplingRate 
        with get() = samplingRate 
        and set(v) = samplingRate <- v 
     
    ///<summary> Deletes the buffer.
    ///</summary>
    override x.Reset() =
        buffer.Clear()
  
    ///<summary> Return the Readings as a sequence
    ///</summary>
    member x.ResultsAsSequence
        with get() = seq {
                for i in buffer do
                    yield i
            }
  
    ///<summary> the array or Readings
    ///</summary>
    member x.Results
        with get() = buffer.ToArray()

    ///<summary> Return the buffer of Readings and reset it.
    /// Use it to clear the sensor's memory while the experiment is running
    ///</summary>
    member x.GetResultsAndReset =
        let l = buffer.ToArray()
        buffer.Clear()
        l
  
    ///<summary> An Observable that raises an event everytime a new Reading is available
    ///</summary>
    member x.ObservableReadings
        with get() = newValue.Publish

    override this.CurrValue() = 
        new Reading(DateTime.Now, valueType, lastv.Value, lastv.Raw)