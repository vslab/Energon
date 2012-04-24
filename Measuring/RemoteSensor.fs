namespace Energon.Measuring

open System

type RemoteSensor(name:string, valueType) = 
    inherit GenericSensor(name, valueType)

    /// the last reading from the sensor
    let mutable lastr = new Reading(DateTime.Now, valueType, 0., null)  

    abstract member PushValue: Reading -> unit
    default x.PushValue v = 
        lastr <- v 

    member x.lastValue
        with get() = lastr

    ///<summary> the array or Readings
    ///</summary>
    member x.Results
        with get() = [| lastr |]
    
    member x.ValueType() =
        valueType

