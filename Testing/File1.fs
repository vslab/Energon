module File1

open Energon.Measuring
open System

[<EntryPoint>]
let main args =
    let proc = new PerfCounter("Process", "% Processor Time", 1.)
    proc.Instance <- "FSI"
    let proc2 = new PerfCounter("Process", "% User Time", 1.)
    proc2.Instance <- "FSI"

    //define an exepriment
    let exp = new ExperimentRun([| proc; proc2 |])

    // something that uses CPU/mem
    let rec fib(n) = 
        match n with
        | 0 -> 0
        | 1 -> 1
        | x -> fib (x-1) + fib (x-2)

    // do something for some time
    printfn "starting..." 
    // the experiment starts
    exp.Start(true)
    System.Threading.Thread.Sleep(1000)
    printfn "%d" (fib 40)
    System.Threading.Thread.Sleep(1000)
    // experiment is over
    exp.Stop()
    printfn "...finished"

    printfn "exp.Results.[proc2].Length=%d" exp.Results.[proc2].Length

    //exp.Start()
    //System.Threading.Thread.Sleep(3000)
    //exp.Stop()
    //printfn "exp.Results.[proc2].Length=%d" exp.Results.[proc2].Length

    Console.ReadLine()
    0