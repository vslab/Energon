module File1

open Energon.Measuring
open System

open Energon.Measuring.Remote


open Energon.Extech380803


[<EntryPoint>]
let main args =



    (*

    #r "FSharp.PowerPack.Linq.dll"
    #r "FSharp.Data.TypeProviders.dll"

    #r "System.Data.DataSetExtensions.dll"
    #r "System.Core.dll"
    *)
    let dbfile = @"C:\Users\root\Desktop\Energon\Measurements.sdf"

    // ------ remote experiment

    //let extechAmp = new Energon.Extech380803.Extech380803Sensor("extechAmp", DataType.Ampere, 1.0)
    let extechWatt = new Energon.Extech380803.Extech380803Sensor("extechWatt", DataType.Watt, 1.0)
    //let extechPF = new Energon.Extech380803.Extech380803Sensor("extechPF", DataType.PowerFactor, 1.0)
    //let extechV = new Energon.Extech380803.Extech380803Sensor("extechV", DataType.Volt, 1.0)
    (*
    extechPF.Close()
    extechAmp.Close()
    extechWatt.Close()
    extechV.Close()
    *)
    let name1 = "completionTime"
    let names = [|  "gpuBusy" ; "aluInsts" ; "fetchInsts" ; "wrInsts" ; "waveFronts" ; "AluBusy" ; "aluFetchRatio" ; 
                "aluPacking" ; "aluPacking" ; "fetchUnitBusy" ; "fetchUnitStalled" ; "fetchSize" ; "cacheHit" ; "writeUnitStalled" ; 
                "ldsFetchInst" ; "aluStalledByLds" ; "ldsBankConfl" ; "fastPath" ; "completePath" ; "pathUtil" |]
    let allNames = seq{
        yield name1
        let n1 = Seq.map (fun (s:string) -> System.String.Format( "{0}_1", s)) names |> Seq.toArray
        for i in n1 do
            yield i
        let n2 = Seq.map (fun (s:string) -> System.String.Format( "{0}_2", s)) names |> Seq.toArray
        for i in n2 do
            yield i
        let n3 = Seq.map (fun (s:string) -> System.String.Format( "{0}_3", s)) names |> Seq.toArray
        for i in n3 do
            yield i
        }
    let sensors = seq {
            yield extechWatt :> GenericSensor
            let namesArray = Seq.toArray allNames
            for n in namesArray do
                yield new RemoteSensor(n, DataType.Unknown) :> GenericSensor
        }

    // declare an experiment
    //let e = new Experiment("saxpy_openCL", (Seq.toArray sensors), 0, [| "mode"; "vector_size"; "samples"; "use_float_4"; "n_thread_host"; "n_device"; "d0_size"; "d0_mode_in"; "d0_mode_out"; "d1_size"; "d1_mode_in"; "d1_mode_out"; "d2_size"; "d2_mode_in"; "d2_mode_out" |], [||], fun _ -> ())
    let e = new Experiment("convolution", (Seq.toArray sensors), 0, [| "mode"; "matrix_w"; "matrix_h"; "filter_size"; "samples"; "n_thread_host"; "n_device"; "d0_size"; "d0_mode_in"; "d0_mode_out"; "d1_size"; "d1_mode_in"; "d1_mode_out"; "d2_size"; "d2_mode_in"; "d2_mode_out" |], [||], fun _ -> ())
    //let e = new Experiment("reduce", (Seq.toArray sensors), 0, [| "mode"; "vector_size"; "samples"; "use_float_4"; "n_thread_host"; "n_device"; "d0_size"; "d0_mode_in"; "d0_mode_out"; "d1_size"; "d1_mode_in"; "d1_mode_out"; "d2_size"; "d2_mode_in"; "d2_mode_out" |], [||], fun _ -> ())
    // db helper
    let saver = new Energon.Storage.ExperimentRuntimeSaver(e, dbfile)

    // the helper makes easy to handle remote loads and remote sensors
    let helper = new RemoteExperimentHelper(e)
    helper.Start()
    Console.WriteLine("press return to stop the test")
    Console.ReadLine()

    try
        helper.Stop()
    with
    | _ -> ()

    Console.ReadLine()
    0