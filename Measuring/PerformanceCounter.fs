namespace Energon.Measuring

open System
open System.Diagnostics

type PerfCounter(cat, name, Hz) as self =
    inherit PushSensor((sprintf "%s:%s" (cat) (name)), DataType.Unknown,Hz)
    let c = cat
    let n = name
    let p = new PerformanceCounter(cat, name)
    let mutable running = false
    let mutable instance = "."   
    let loop =
        async {
            while running do
                let v = p.NextValue()
                self.PushValue(new Reading(DateTime.Now, DataType.Unknown, float(v), p.RawValue))
                System.Threading.Thread.Sleep(500)
        }

    member x.Instance 
        with get() = instance
        and set(v) = 
            instance <- v
            p.InstanceName <- v

    override x.Start() =
        if not running then
            running <- true
            Async.Start(loop)
            base.Start()

    override x.Stop() = 
        running <- false
        base.Stop()
