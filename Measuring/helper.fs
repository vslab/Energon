namespace Energon.Measuring.Helper


open System
open System.Net
open System.Text
open Energon.Measuring
open System.Collections.Generic

/// <summary>An helper class that handles an experiment where the load is run remotely, with remote sensors</summary>
type ExperimentHelper(e:Experiment) = 
    member x.start() = 
        // start a new experiment run
        let er = new ExperimentRun(e.Sensors)
        let case = e.Cases.Item (e.Cases.Count - 1)
        case.AddExperimentRun(er)
        er.Start(true)
        printf "received start/n"
    member x.stop(vals) =
        let vq = new Queue<string>()
        Seq.iter (fun (s:string) -> vq.Enqueue(s)) vals
        vq.Dequeue() |> ignore // first is "stop"
        Seq.iter (fun (s:GenericSensor) -> 
            match s with
            | :? RemoteSensor as rs -> 
                let raw = vq.Dequeue()
                let ci = System.Globalization.CultureInfo.InstalledUICulture
                let ni =  ci.NumberFormat.Clone()  :?> (System.Globalization.NumberFormatInfo)
                ni.NumberDecimalSeparator <- "."

                let floatV = System.Single.Parse(raw, ni)
                printf "%f\n" floatV
                let r = new Reading(DateTime.Now, rs.ValueType(), float(floatV), raw :> obj)
                rs.PushValue r
            | _ -> ()
            ) e.Sensors
        let case = e.Cases.Item (e.Cases.Count - 1)
        let er = case.Runs.Item (case.Runs.Count - 1)
        er.Stop()
        printf "case "

    member x.caseCallback(args) =
        let tail = Seq.skip 1 args
        let argsAsObj = tail |> Seq.map (fun (s:string) -> (s :> obj))
        let newCase = new ExperimentCase(e.Sensors, 0, argsAsObj , (fun _ -> ()) )
        e.AddExperimentCase newCase


