 
// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.

module CPUSpecImporter.Main

open System


[<EntryPoint>]
let main args = 
    Console.WriteLine("---- CPU SPEC results import tool ----")
    Console.WriteLine("this tool will import every CPU SPEC result from the CPU SPEC website, and save them in a local database. It will take several minutes to complete (to avoid DOS attacking CPU SPEC), so please be patient.")

    
    let fetchFile (rel_addr:string) =
      let addr = "http://www.spec.org/cpu2000/results/" + rel_addr
      let wc = System.Net.WebClient()
      let content = wc.DownloadString(addr)
      let righe = content.Split([| '\n' |])
      let filtro (last:string) (b:bool,starr) (c:string) = 
        if b then
          if c.Trim().StartsWith(last) then
            (false,starr)
          else
            let tokens = c.Trim().Split([| ' ' |], System.StringSplitOptions.RemoveEmptyEntries)
            if tokens.Length < 7 then
              (true,starr)
            else
              let n = (tokens.[0], tokens.[5])       
              (true,n::starr)
        else
          if c.Trim().StartsWith("====") then
            (true,starr)
          else
            (false,starr)
      let setup  = 
        let from = content.IndexOf("HARDWARE")
        let upto = content.IndexOf("NOTES")
        content.Substring(from, upto-from)
      let filtroSpecInt = filtro "SPECint_base2000"
      let filtroSpecFPU = filtro "SPECfpu_base2000"
      let filtro =
        if content.Trim().StartsWith("SPEC CINT2000 Summary") then
          filtroSpecInt
        else
          filtroSpecFPU
      let _, misure = Seq.fold filtro (false, []) righe  
      (misure, setup)

//    let addr2 = "res2001q4/cpu2000-20011023-01096.asc"
//
//    let misure, setup = fetchFile addr2


    Console.WriteLine("downloading index...")
    let baseaddr = "http://www.spec.org/cpu2000/results/cpu2000.html" 
    let wc = System.Net.WebClient()
    let content = wc.DownloadString(baseaddr)
    Console.WriteLine("...index downloaded")

    let avanza (testo:string) (dove:string) = 
      let indice = testo.IndexOf(dove)
      if indice + dove.Length < testo.Length then
        let prima = testo.Substring(0, indice + dove.Length)
        prima, testo.Substring(indice + dove.Length)
      else
        ("",testo)
  
    let pre_table, post_pre_table = avanza content "<TABLE"
    let table, post_table = avanza post_pre_table "</TABLE"
    let _, header_1 = avanza table "<TR>"
    let _, from_second = avanza header_1 "<TR>"
    let rec analyse (urls,blob) =
      let second, from_third = avanza blob "<TR>"
      if second.Length > 0 then
        let _, from_href = avanza second "<A HREF=\""
        let link, _ = avanza from_href ".asc"
        if link.Length > 0 then
          analyse (link::urls, from_third)
        else
          urls,blob
      else
        urls,blob

    let urls, blob = analyse ([], from_second)

    Console.WriteLine("index contains " + urls.Length + " elements" )

//    let urls2 = Seq.take 10 urls
//    let runs = Seq.map fetchFile urls2
//    let run_clean = Seq.filter (fun (l:(string*string) list,d:string) -> l.Length > 0) runs
//    let runrs_array = Seq.toArray run_clean
//
//    let l1 = runrs_array.[0]
//    urls.[0]

    Console.WriteLine("hit return to close the program" )
    Console.WriteLine()

    0

