// Learn more about F# at http://fsharp.net

module Energon.CSV

open System
open System.IO
open System.Text
open Energon.Measuring

let saveSensorDataToCSV (filename, sensor:PushSensor) =
    let file = File.CreateText filename
    let sb = new StringBuilder()
    sb.AppendFormat("timestamp;{0};{1}\r\n", sensor.Name) |> ignore
    sensor.Results |> Seq.iter (fun r -> sb.AppendFormat("{0};{1};", r.Timestamp.ToLongTimeString(), r.Value) |> ignore)
    file.Write(sb.ToString())
    file.Close()

let saveExperimentRunResultsToCompactCSV (filename, experiment:ExperimentRun, timeinterval) =
    let file = File.CreateText filename
    let sb = new StringBuilder()
    let results = experiment.GetResultsCompactForm(timeinterval)
    let (sensors, readings) = results
    sb.Append("timestamp;") |> ignore
    sensors |> Seq.iter (fun (s:GenericSensor) -> sb.AppendFormat("{0};", s.Name) |> ignore)
    sb.Append("\r\n") |> ignore
    for j in 0..(readings.GetUpperBound(1)) do
        sb.AppendFormat("{0};", readings.[0,j].Timestamp.ToLongTimeString()) |> ignore
        for i in 0..(readings.GetUpperBound(0)) do
            let r = readings.[i,j];
            sb.AppendFormat("{0};", r.Value) |> ignore
        sb.Append("\r\n") |> ignore 
    file.WriteLine(sb.ToString())
    file.Close()

    