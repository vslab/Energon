namespace Energon.Measuring

open System

type Reading(timestamp:DateTime, unitOfMeasurement:DataType, value:float, raw) =
    let t = timestamp
    let v = value
    let u = unitOfMeasurement
    let r = raw
    member x.Timestamp
        with get() = t   
    member x.Value
        with get() = v    
    member x.UnitOfMeasurement
        with get() = u
    member x.Raw
        with get() = r

