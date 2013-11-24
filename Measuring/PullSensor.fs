namespace Energon.Measuring

open System

type PullSensor(name:string, valueType) = 
    inherit GenericSensor(name, valueType)
    let lastr = new Reading(DateTime.Now, valueType, 0., null)

    ///<summary> Starts acquiring data.
    ///</summary>
    abstract member CurrValue: unit -> Reading
    default u.CurrValue() = 
        lastr
     
