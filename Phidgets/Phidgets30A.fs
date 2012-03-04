module Phidgets30A



open Energon.Measuring
open Phidgets 
open System
  
let ifkit = new InterfaceKit() 
let openPhidgets() = typeof<InterfaceKit>.GetMethod("open", [| |]).Invoke(ifkit, [| |]) |> ignore 
  
[<Measure>] type Hz // Hertz 
[<Measure>] type s // Seconds 
[<Measure>] type ms // Seconds 
  
type AmmeterSensor(name, id:int, kit:InterfaceKit, Hz) as self= 
  inherit PushSensor(name, DataType.Ampere, Hz) 
  let k = 0.04204
  let sensor = kit.sensors.[id]
  do 
    sensor.Sensitivity <- 1 
    kit.SensorChange.Add(fun v -> 
      if v.Index = id then
        let r = new Reading(DateTime.Now, DataType.Ampere, float(v.Value) * k, v.Value)
        in
            self.PushValue(r)
    ) 
  
  