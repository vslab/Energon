#r @"C:\Users\Davide\Desktop\Projects\energon\energon\EnergonFramework\Measuring\bin\Debug\Energon.Measuring.dll"
open Energon.Measuring

#load "RealtimeTools.fs" 

let numEvent = new Event<Reading>()
let numEventStream, fireNewNum = numEvent.Publish, numEvent.Trigger

// Derive a new event that computes the running mean and stddev 
// over the original event stream
let stddevEvents = RealtimeTools.movingAverageReadingEvt 5 numEventStream

// Hook up an event handler to print out the new mean and standard deviation 
do stddevEvents |> Event.add (fun (mean) -> printfn "Mean = %A" mean)
ShowSensor


do fireNewNum 3.
do fireNewNum 7.
do fireNewNum 7.
do fireNewNum 19.