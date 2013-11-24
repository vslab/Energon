namespace Energon.Measuring

open System
open System.Threading
  
type GenericSensor(name:string, valueType) = 
  let valueType:DataType = valueType
  let mutable error = 0.
  let mutable running = false
  let mutable id = 0

  member x.ID 
    with get() = id
    and set(v) = id <- v

  ///<summary> Starts acquiring data.
  ///</summary>
  abstract member Start: unit -> unit
  default u.Start() = 
    running <- true

  ///<summary> Stops acquiring data
  ///</summary>
  abstract member Stop: unit -> unit
  default u.Stop() = 
    running <- false

  ///<summary> Open the sensor. Its state will be unpredictable after this command.
  ///</summary>
  abstract member Open: unit -> unit
  default u.Open() = ()

  ///<summary> Closes the sensor. Its state will be unpredictable after this command.
  ///</summary>
  abstract member Close: unit -> unit
  default u.Close() = ()
  
  ///<summary> Starts acquiring data.
  ///</summary>
  abstract member Reset: unit -> unit
  default u.Reset() = ()
 
  ///<summary> Returns the type of data measured by this sensor
  ///</summary>
  member x.DataType 
    with get() = valueType 
  
  ///<summary> The name of this sensor
  ///</summary>
  member x.Name
    with get() = name

  ///<summary> Get or Set the error associated to this sensor
  ///</summary>
  member x.Error 
    with get() = error 
    and set(v) = error <- v 
   
  ///<summary> true if the sensor is currently acquiring ata
  ///</summary>
  member x.IsRunning
    with get() = running
