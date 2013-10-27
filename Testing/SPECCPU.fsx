
open System
open System.Text

type Environment(cpuName:string, cpuMhz:string, fpu:string, memory:string, os:string, compiler:string) =
  let sanityze (insane:string) =
    insane.Replace(",", ".") 
  member x.CpuName
    with get() = cpuName
  member x.CpuMHz
    with get() = cpuMhz
  member x.FPU
    with get() = fpu
  member x.Memory
    with get() = memory
  member x.OS
    with get() = os
  member x.Compiler
    with get() = compiler
  member x.printFields(sb:StringBuilder) =
    sb.AppendFormat("{0},{1},{2},{3},{4},{5},", sanityze cpuName, sanityze cpuMhz, sanityze fpu, sanityze memory, sanityze os, sanityze compiler)
  member x.Equals(env2:Environment) =
    cpuName.Equals(env2.CpuName) && cpuMhz.Equals(env2.CpuMHz) && fpu.Equals(env2.FPU) && memory.Equals(env2.Memory) && os.Equals(env2.OS) && compiler.Equals(env2.Compiler)

type Program(name:string) =
  member x.Name
    with get() = name
  member x.printFields(sb:StringBuilder) =
    sb.AppendFormat("{0},", name)
  member x.Equals(p2:Program) =
    name.Equals(p2.Name)

type Experiment(environmnet:Environment, program:Program, completionTime:double) =
  member x.CompletionTime
    with get() = completionTime
  member x.Env
    with get() = environmnet
  member x.Prog
    with get() = program
  member x.printFields(sb:StringBuilder) =
    environmnet.printFields(sb) |> ignore
    program.printFields(sb) |> ignore
    sb.AppendFormat("{0}\n", completionTime)
    
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

let firstCsv = urls |> Seq.head
let addr = "http://www.spec.org/cpu2006/results/" + firstCsv
printfn "fetching %s" addr
//System.Threading.Thread.Sleep(1000)
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



let env = new Environment(getItem hw "CPU Name", getItem hw "CPU MHz", getItem hw "FPU", getItem hw "Memory", getItem sw "Operating System", getItem sw "Compiler")

let createExp (n:string,v:double) =
    let prog = new Program(n)
    let exp = new Experiment(env, prog, v)
    exp

let exps = progs |> Seq.map createExp
let sb = new StringBuilder()
exps |> Seq.iter (fun e -> e.printFields(sb) |> ignore)

sb.ToString()
sw
hw
getItem hw "CPU MHz"
getItem hw "CPU Characteristics"
getItem sw "Operating System"

hw
sw

stuff.Count


content.Split([| '\n' |]) |> Seq.skip 100 |> Seq.take 40 |> Seq.iter (fun l -> System.Console.WriteLine(l))


let fetchFile (rel_addr:string) =
    let addr = "http://www.spec.org/cpu2006/results/" + rel_addr
    printfn "fetching %s" addr
    //System.Threading.Thread.Sleep(1000)
    let wc = new System.Net.WebClient()
    let content = wc.DownloadString(addr)
    if content.StartsWith("valid,0") then
        ([], "", addr)
    else
        let makerInizio = "\"Selected Results Table\""
        let datiECoda = content.Substring(content.IndexOf(makerInizio) + makerInizio.Length)
        let soloDati = datiECoda.Substring(0, datiECoda.IndexOf("SPECfp"))
        let righe = soloDati.Split([| '\n' |])

        let readLine (line:string) =
            let tags = line.Split([| ',' |], System.StringSplitOptions.RemoveEmptyEntries)
            if (tags.Length = 0) then
                (false, "", "")
            else
                if tags.[0] = "Benchmark" then
                    (false, "", "")
                else
                    (true, tags.[0], tags.[2])
        let valoriDaFiltrare = Seq.map readLine righe
        let misure = Seq.filter (fun (b:bool,_,_) -> b) valoriDaFiltrare |> Seq.map (fun (_,a,b) -> a,b) |> Seq.toList
        (misure, "", addr)
