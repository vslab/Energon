#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#I @"C:\Users\davide\Documents\GitHub\Energon\bin\Debug"

#r @"Energon.Measuring.dll"
#r @"Energon.Storage.dll"
#r "System.Data.Linq.dll"
#r "System.Linq.dll"

#r "FSharp.PowerPack.Linq.dll"
//#r "FSharp.Data.TypeProviders.dll"

#r "System.Data.DataSetExtensions.dll"
#r "System.Core.dll"
#r "SQLExpress.dll"

open Energon.Storage
open System
//open Microsoft.FSharp.Data.TypeProviders
open System.Data.Linq.SqlClient
open System.Linq
open Microsoft.FSharp.Linq
open System.Data.Linq
open Energon.Measuring
open Energon.Measuring.Database
open System.Text
open System.Collections.Generic
open SQLExpress


let system = "ImportedFromSpecWebsite"

// db helper
//let server = "HPLAB\SQLEXPRESS"
let server = "MANDARINO\MISURATORE"
let dbname = "Measures"

let sensors = [| new RemoteSensor("seconds", DataType.Unknown) :> GenericSensor|]

let createExp (prog:string) (system:string) =
  new Experiment(System.String.Format("{0}_{1}", prog, system), sensors, 0, [| "size" |], [||], fun _ -> ())

let createAndStartExp prog sys env =
  let e = createExp prog sys
  e.Note <- env
  let saver = new Energon.Storage.ExperimentRuntimeSaverExpress(e, server, dbname )
  new Energon.Measuring.Helper.ExperimentHelper(e)

let saveCpuSpecRun sys env prog time =
  let test = createAndStartExp prog sys env
  test.caseCallback([| "1.0" |])
  test.start()
  test.stop([| "ignore"; time |])


Console.WriteLine("---- CPU SPEC results import tool ----")
Console.WriteLine("this tool will import every CPU SPEC result from the CPU SPEC website, and save them in a local database. It will take several minutes to complete (to avoid DOS attacking CPU SPEC), so please be patient.")

    
let fetchFile (rel_addr:string) =
    let addr = "http://www.spec.org/cpu2006/results/" + rel_addr
    let wc = System.Net.WebClient()
    let content = wc.DownloadString(addr)
    let righe = content.Split([| '\n' |])
    let filtro (last:string) (b:bool,starr) (c:string) = 
        if b then
            if c.Trim().StartsWith(last) then
            (false,starr)
            else
            let tokens = c.Trim().Split([| ' ' |], System.StringSplitOptions.RemoveEmptyEntries)
            if tokens.Length < 8 then
                (true,starr)
            else
                let n = (tokens.[0], tokens.[6])       
                (true,n::starr)
        else
            if c.Trim().StartsWith("====") then
            (true,starr)
            else
            (false,starr)
    let setup  = 
        let from = content.IndexOf("HARDWARE")
        let upto = content.IndexOf("Operating System Notes")
        content.Substring(from, upto-from)
    let filtroSpecInt = filtro "SPECint(R)_base2006"
    let filtroSpecFPU = filtro "SPECfpu(R)_base2006"
    let filtro =
        if content.Trim().StartsWith("SPEC(R) CINT2006 Summary") then
            filtroSpecInt
        else
            filtroSpecFPU
    let _, misure = Seq.fold filtro (false, []) righe  
    (misure, setup, addr)

//    let addr2 = "res2001q4/cpu2000-20011023-01096.asc"
//
//    let misure, setup = fetchFile addr2


Console.WriteLine("downloading index...")
let baseaddr = "http://www.spec.org/cpu2006/results/cpu2006.html" 
let wc = new System.Net.WebClient()
let content = wc.DownloadString(baseaddr)
Console.WriteLine("...index downloaded")

let avanza (testo:string) (dove:string) = 
    let indice = testo.IndexOf(dove)
    if indice + dove.Length < testo.Length then
      let prima = testo.Substring(0, indice + dove.Length)
      prima, testo.Substring(indice + dove.Length)
    else
      ("",testo)
  
let pre_table, post_pre_table = avanza content "<table"
let table, post_table = avanza post_pre_table "</table"
let _, body = avanza table "<tbody>"
let _, from_second = avanza body "<tr>"
let rec analyse (urls,blob) =
    let second, from_third = avanza blob "<tr>"
    if second.Length > 0 then
        let _, from_href = avanza second "<a href=\""
        let _, from_href2 = avanza from_href "<a href=\""
        let _, from_href3 = avanza from_href2 "<a href=\""
        let link, _ = avanza from_href3 ".txt"
        if link.Length > 0 then
            analyse (link::urls, from_third)
        else
            urls,blob
    else
        urls,blob

let urls, blob = analyse ([], from_second)

System.Console.WriteLine("index contains " + urls.Length.ToString() + " elements" )

//let urls2 = Seq.skip 2352 urls
//let runs = Seq.map fetchFile urls2
//urls.Length

let runs = Seq.map fetchFile urls
let run_clean = Seq.filter (fun (l:(string*string) list,d:string,e:string) -> l.Length > 0) runs
//let runs_array = Seq.toArray run_clean

let save prog time system note  =
  saveCpuSpecRun system note prog time

let savePrograms ((l:(string*string) list),systemDiscard,note) =
  Seq.iter (fun (prog,time) -> save prog time system note ) l
  System.Console.WriteLine(note + " saved ")
  System.Threading.Thread.Sleep(2000)
      
Seq.iter savePrograms run_clean


runs_array.Length

let l1,s1,e1 = runrs_array.[0]
l1
s1

content.Count
urls.Length
urls.[2353]




