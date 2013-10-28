
open System
open System.Text

#I @"C:\Users\davidemorelli\Documents\GitHub\Energon\DataLayer"
#I @"C:\Users\davidemorelli\Documents\GitHub\Energon\DataLayer\bin\Debug"
#I @"C:\Users\davidemorelli\Documents\GitHub\Energon\DataLayer\bin\Debug\x86"
#r @"DataLayer.dll"
open DataLayer


let helper = new SPECCPUHelper()

    
Console.WriteLine("downloading index...")
let baseaddr = "http://www.spec.org/cpu2006/results/cfp2006.html" 
let wc = new System.Net.WebClient()
let content = wc.DownloadString(baseaddr)
Console.WriteLine("...index downloaded")

open System.Text.RegularExpressions

let urls = seq {
  let urlsL = Regex.Matches(content, @"href=""(.*\.csv)""")
  for i in 0..(urlsL.Count-1) do
    yield urlsL.Item(i).Groups.Item(1).Value
  }

//let firstCsv = urls |> Seq.head

let processUrl firstCsv =
    let addr = "http://www.spec.org/cpu2006/results/" + firstCsv
    printfn "fetching %s" addr
    System.Threading.Thread.Sleep(500)
    let wc = new System.Net.WebClient()
    let content = wc.DownloadString(addr)
    let righe = content.Split([| '\n' |])
    let stuff = Regex.Matches(content, @"Selected.Results.Table([\s\S]*)HARDWARE([\s\S]*)SOFTWARE([\s\S]*)Base.Compiler.Invocation")
    let results = stuff.Item(0).Groups.Item(1).Value
    let hw = stuff.Item(0).Groups.Item(2).Value
    let sw = stuff.Item(0).Groups.Item(3).Value
    let resultsLines = results.Split([|'\n'|])
    let foldProg (progs: (string*double) list) (line:string) =
        let tags = line.Split([|','|])
        let progName = tags.[0]
        if (Regex.Match(line, @"^\d\d\d[\S\s]*").Captures.Count > 0) then
            let tagT = tags.[1]
            System.Console.WriteLine(@"{0} {1}", tags.[0], tags.[2])
            (progName,double(tagT))::progs
        else
            progs
    let progs = resultsLines |>  Seq.fold foldProg (List.empty<string*double>)
    let getItem (l:string) (item:string) =
        let filterForItem (line:string) =        
            let tags = line.Split([|','|])
            let name = tags.[0].Replace("\"","")
            if (name.Equals(item)) then
                true
            else
                false
        let res = l.Split([|'\n'|]) |> Seq.find filterForItem
        res.Substring(res.IndexOf(",")+1).Replace("\"","")
    let createExp (n:string,v:double) = helper.SaveNewExperiment(@"C:\Users\davidemorelli\Documents\GitHub\Energon\DataLayer\SPECCPU.sqlite", getItem hw "CPU Name", getItem hw "CPU MHz", getItem hw "FPU", getItem hw "Memory", getItem sw "Operating System", getItem sw "Compiler", n, v);
    progs |> Seq.iter createExp

urls |> Seq.take 2 |> Seq.iter processUrl

urls |> Seq.iter processUrl


