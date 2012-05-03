namespace Energon.Measuring.Remote


open System
open System.Net
open System.Text
open Energon.Measuring
open System.Collections.Generic

type WebListener(startCallback, stopCallback, newCase) =
  let address = "http://+:80/Temporary_Listen_Addresses/"
  let listener = new System.Net.HttpListener()
  let rec GetContextCallback(result:IAsyncResult) =
    let context = listener.EndGetContext(result)
    let request = context.Request
    let relPath = request.Url.PathAndQuery.Substring("/Temporary_Listen_Addresses/".Length )
    let spl = List.toArray ["/"; "?"]
    let tags = relPath.Split(spl, StringSplitOptions.RemoveEmptyEntries) |> Array.map (fun (s:string) -> 
        printf "%s\n" s |> ignore
        System.Web.HttpUtility.UrlDecode(s) 
        )
    let op = tags.[0]
    match op with 
      | "start" -> startCallback()
      | "stop" -> if tags.Length > 1 then stopCallback(tags) else stopCallback([| |])
      | "case" -> if tags.Length > 1 then newCase(tags) else newCase([| |])
      | _ -> printf "undefined op\n"
    let response = context.Response
    let sb = new StringBuilder()
    sb.Append(op) |> ignore
    let buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString())
    response.ContentLength64 = int64( buffer.Length) |> ignore
    use outputStream = response.OutputStream
    outputStream.Write(buffer, 0, buffer.Length);
    listener.BeginGetContext(new AsyncCallback(GetContextCallback), null) |> ignore
  do
    listener.Prefixes.Add(address);

  member x.start() =
    listener.Start()
    listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);

  member x.stop() =
    listener.Stop()


/// <summary>An helper class that handles an experiment where the load is run remotely, with remote sensors</summary>
type RemoteExperimentHelper(e:Experiment) = 
    let start() = 
        // start a new experiment run
        let er = new ExperimentRun(e.Sensors)
        let case = e.Cases.Item (e.Cases.Count - 1)
        case.AddExperimentRun(er)
        er.Start(true)
        printf "received start/n"
    let stop(vals) =
        let vq = new Queue<string>()
        Seq.iter (fun (s:string) -> vq.Enqueue(s)) vals
        vq.Dequeue() |> ignore // first is "stop"
        Seq.iter (fun (s:GenericSensor) -> 
            match s with
            | :? RemoteSensor as rs -> 
                let raw = vq.Dequeue()
                printf "%s\n" raw
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

    let caseCallback(args) =
        let tail = Seq.skip 1 args
        let argsAsObj = tail |> Seq.map (fun (s:string) -> (s :> obj))
        let newCase = new ExperimentCase(e.Sensors, 0, argsAsObj , (fun _ -> ()) )
        e.AddExperimentCase newCase
    let w = new WebListener(start, stop, caseCallback)

    member x.Start() =
        w.start()

    member x.Stop() =
        w.stop()


/// <summary> This class should be used on the remote side, in the case that we are measuring a remote process, with remote sensors </summary>
type RemoteSensorHelper(ip:string) =
    let wc = new WebClient()
    let address = System.String.Format(@"http://{0}/Temporary_Listen_Addresses", ip)
    member x.start() =
        let addr = System.String.Format(@"{0}/{1}", address, "start")
        try
            wc.DownloadString(addr) |> ignore
            true
        with 
            | _ -> false
    member x.stop(par:seq<string>) =
        let sb = new System.Text.StringBuilder()
        sb.AppendFormat(@"{0}/stop", address) |> ignore
        par |> Seq.iter (fun s -> sb.AppendFormat(@"/{0}", System.Web.HttpUtility.UrlEncode(s)) |> ignore)
        let addr = sb.ToString()
        try
            wc.DownloadString(addr) |> ignore
            true
        with 
            | _ -> false
    member x.experimentCase(args:seq<string>) =
        let sb = new System.Text.StringBuilder()
        sb.AppendFormat(@"{0}/case", address) |> ignore
        args |> Seq.iter (fun s -> sb.AppendFormat(@"/{0}", s) |> ignore)
        let addr = sb.ToString()
        try
            wc.DownloadString(addr) |> ignore
            true
        with 
            | _ -> false
