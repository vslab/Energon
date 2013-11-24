// Learn more about F# at http://fsharp.net
module BenchRunner

open System
open Energon.Measuring

[<EntryPoint>]
let main args =
    //let procTest1 = new PerfCounter("Processore", "% Tempo utente", 1.)
    //procTest1.Instance <- "_Total"
    //let procTest2 = new PerfCounter("Memory", "Cache Faults/sec", 1.)

    let runExp sensors exp =
        let run (s:PullSensor) =
            s.Start()
        let halt (s:PullSensor) =
            s.Stop()
        let halt (s:PullSensor) =
            s.Stop()
        let getAvg (rl:seq<Reading>) =
            let asList = Seq.toList rl
            let sum = Seq.fold (fun state (reading:Reading)-> state + reading.Value ) 0. rl
            let n = float(List.length asList)
            sum / n
        let getRes (s:PushSensor) =
            getAvg s.GetResultsAndReset
        Seq.iter run sensors
        exp()
        Seq.iter halt sensors
        Seq.map getRes sensors


    let runProg (prog:string) args =
        let psi = new System.Diagnostics.ProcessStartInfo(System.String.Format("C:\\progetti\\Energon\\benchmarks\\{0}.exe", prog))
        psi.RedirectStandardOutput <- true
        psi.WindowStyle <- System.Diagnostics.ProcessWindowStyle.Hidden
        psi.UseShellExecute <- false
        psi.Arguments <- args
        let proc = System.Diagnostics.Process.Start(psi)
        let myOut = proc.StandardOutput
        proc.WaitForExit()

    let helper = new Energon.Measuring.Remote.RemoteSensorHelper("131.114.88.115")
    let run prog arg = 
        let load() = 
            runProg prog arg
        let starttime = System.DateTime.Now
        helper.start() |> ignore
        let vals = Seq.map (fun (f:float) -> System.String.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "{0}", f)) (runExp [| (*procTest1; procTest2*) |] load)
        let stoptime = System.DateTime.Now
        let vals2 = seq {
                yield System.String.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "{0}",stoptime.Subtract(starttime).TotalSeconds)
            }
        let vals3 = Seq.append vals vals2
        helper.stop(vals3) |> ignore

    let runCase prog arg = 
        let args = seq {yield arg}
        System.Console.WriteLine(System.String.Format("preparing to run program {0} with arg {1}", prog, arg))
        (helper.experimentCase args) |> ignore
        for i in 1..5 do
            System.Console.WriteLine(System.String.Format("running iteration {0}", i))
            run prog arg
            System.Threading.Thread.Sleep(2000)

    let args = [| "1048576"; "4194304"; "16777216"; "67108864"; "268435456" |]

    let runCycle prog =
        args |> Seq.iter (fun a -> runCase prog a )

    (*

    runCase "quick" args.[3]

    runCycle "quick"
    runCycle "merges"
    runCycle "heap"

    runCase "randMemAccess" args.[3]
    runCase "simpleINT" args.[3]
    runCase "simpleFPU" args.[3]
    *)
    let selectProg() =
        System.Console.WriteLine("select program: quick, merge, heap, randMemAccess, simpleINT, simpleFPU or gibberish to exit")
        let s = System.Console.ReadLine()
        match s with
        | "quick" -> 
            runCycle "quick"
            true
        | "merge" -> 
            runCycle "merges"
            true
        | "heap" -> 
            runCycle "heap"
            true
        | "randMemAccess" -> 
            runCase "randMemAccess" args.[3]
            true
        | "simpleINT" -> 
            runCase "simpleINT" args.[3]
            true
        | "simpleFPU" -> 
            runCase "simpleFPU" args.[3]
            true
        | _ -> 
            false

    while (selectProg()) do ()
    0
