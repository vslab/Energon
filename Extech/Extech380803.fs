namespace Energon.Extech380803

open System
open System.Text
open System.IO.Ports
open Energon.Measuring

type Extech380803MultiSensor() as self =
    let dt = 400
    let mutable refCount = 0
    let mutable refCountRunning = 0
    let mutable currW = new Reading(DateTime.Now, DataType.Watt, 0., null) 
    let mutable currA = new Reading(DateTime.Now, DataType.Ampere, 0., null) 
    let mutable currPF = new Reading(DateTime.Now, DataType.PowerFactor, 0., null) 
    let mutable currV = new Reading(DateTime.Now, DataType.Volt, 0., null) 
    let mutable currHz = new Reading(DateTime.Now, DataType.Hz, 0., null) 
    //let b2function = dict [ ("200.0V", DataType.Volt); ("1000V", DataType.Volt); ("Hz", , DataType.Hz) ; ("2.000A", DataType.Ampere ); ("20.00A", DataType.Ampere ); ("200.0W", DataType.Watt ); ("2000W", DataType.Watt ); ("PF", DataType.PowerFactor ); ("HOLD", DataType.Unknown )] 
    //let b2function = dict [ (3, "200.0V"); (4, "1000V"); (5, "Hz") ; (49, "2.000A" ); (33, "20.00A" ); (192, "200.0W" ); (193, "2000W" ); (208, "PF" ); (255, "HOLD" )] 
    let b2function = dict [ (3, DataType.Volt); (4, DataType.Volt); (5, DataType.Hz) ; (49, DataType.Ampere ); (33, DataType.Ampere ); (192, DataType.Watt ); (193, DataType.Watt ); (208, DataType.PowerFactor ); (255, DataType.Unknown )] 
    let rec reversebits len x =
        if len = 0 then
            0
        else
            let msbit = x &&& 1
            let remaining = x >>> 1
            let len = len - 1
            in
                (msbit <<< len) ||| reversebits len remaining
    let rec decodeBCD x =
        if x = 0 then 0
        else
            let lsd = x &&& 0xF
            if lsd > 10 then raise (System.ArgumentException(string lsd + " is not a legal BCD"))
            else decodeBCD (x >>> 4) * 10 + lsd
    let parseValue data =
        match data with
            | 2,b2,b3,b4,3 ->
                let t = b2function.[b2]
                let d = (b4 <<< 8) + b3
                let polarity = d &&& 1
                try
                    let value = reversebits 13 (d >>> 1) |> decodeBCD
                    let divider = pown 10 (reversebits 2 (d >>> 14))
                    let sign = polarity * 2 - 1
                    in
                        t, float (sign *  value) / float divider
                with
                    | _ -> t, nan
            | _ ->  raise (System.ArgumentException(string data + " does not have the correct head/tail"))
    let readData (buffer:int[]) = (buffer.[0], buffer.[1], buffer.[2], buffer.[3], buffer.[4])
    let parse (buf: byte[]) (i) =
        let t,v = buf |> Seq.ofArray |> Seq.skip i |> Seq.take 5 |> Seq.map (fun x -> int x) |> Array.ofSeq |> readData |> parseValue
        in
            //sb.Append(t.ToString() + "(" + v.ToString() + ")") |> ignore
            match (t,v) with
            | DataType.Ampere, value -> currA <- new Reading(DateTime.Now, t, v, buf)
            | DataType.Watt, value -> currW <- new Reading(DateTime.Now, t, v, buf)
            | DataType.Hz, value -> currHz <- new Reading(DateTime.Now, t, v, buf)
            | DataType.PowerFactor, value -> currPF <- new Reading(DateTime.Now, t, v, buf)
            | DataType.Volt , value -> currV <- new Reading(DateTime.Now, t, v, buf)
            | _, _ -> ignore |> ignore
    let mutable running = true
    let port = new SerialPort(BaudRate = 9600, DataBits = 8, DtrEnable = true, Handshake = Handshake.None, Parity = Parity.None, ReadTimeout = 2000, StopBits = StopBits.One)
    let s = 
        port.Open()
        port.ReadTimeout <- 10000
        port.BaseStream
    let rbuf : byte array = Array.zeroCreate 64
    //let setValue(v)=
                
    let cycle =
        async {
            while running do
                s.WriteByte(byte 0x20)
                s.Flush()
                let tstart = DateTime.Now
                //let sb = new StringBuilder()
                let  len = ref 0
                while !len < 20 do
                    try
                        len := !len + port.Read(rbuf, !len, 20 - !len)
                    with 
                    | _ -> ()
                if !len = 20 then
                    for i in 0..5..(!len - 5) do
                        try
                            parse rbuf i
                        with
                        | _ -> ()
                    //System.Console.WriteLine(sb.ToString())
                let elapsed = (DateTime.Now - tstart).Milliseconds
                if dt > elapsed then
                    System.Threading.Thread.Sleep(dt - elapsed) |> ignore
        }
    member x.GetCurr (vType) =
        match vType with
        | DataType.Ampere -> currA 
        | DataType.Watt -> currW 
        | DataType.Hz-> currHz 
        | DataType.PowerFactor -> currPF
        | DataType.Volt -> currV 
        | _ -> new Reading(DateTime.Now, DataType.Unknown, 0., null)
    member x.CurrW 
        with get() = currW
    member x.CurrA 
        with get() = currA
    member x.CurrPF 
        with get() = currPF
    member x.CurrV 
        with get() = currV
    member x.CurrHz 
        with get() = currHz

    member x.Start()=
        running <- true
        Async.Start(cycle)
    member x.Stop()=
        running <- false
    member x.Increment() =
        lock self (fun () ->
            refCount <- refCount + 1  
            refCount )
    member x.Decrement() =
        lock self (fun () ->
            refCount <- refCount - 1    
            refCount )
    member x.GetRefCount() =
        refCount
    member x.IncrementRunning() =
        lock self (fun () ->
            refCountRunning <- refCountRunning + 1  
            refCountRunning )
    member x.DecrementRunning() =
        lock self (fun () ->
            refCountRunning <- refCountRunning - 1    
            refCountRunning )   
    member x.GetRefCountRunning() =
        refCountRunning
    member x.Close() =
        port.Close()

        
type Extech380803Sensor(name, vType, Hz) as self =
    inherit PushSensor(name, vType, Hz)
    static let mutable instance = lazy(new Extech380803MultiSensor())
    let mutable running = false
    do 
        instance.Value.Increment() |> ignore
    let setValue(v)=
        self.PushValue(v)        
    let cycle =
        async {
            while running do
                setValue(instance.Value.GetCurr(vType))
                System.Threading.Thread.Sleep(1000/2) |> ignore
        }
    member x.GetInstance() =
        instance
    override x.Start()=
        if not running then
            if instance.Value.IncrementRunning() = 1 then
                instance.Value.Start()
            running <- true
            Async.Start(cycle)
    override x.Stop()=
        running <- false
        if instance.Value.DecrementRunning() = 0 then
            instance.Value.Stop()
    override x.Close() = 
        if instance.Value.Decrement() = 0 then
            instance.Value.Close()
               


