// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#load "Module1.fs"
open Module1

#r @"C:\Users\Davide\Desktop\Projects\energon\energon\EnergonFramework\Charts\MSDN.FSharpChart.dll"

open MSDN.FSharp.Charting


let S = seq { let r = System.Random(100) in while true do yield r.NextDouble() }
let prob0 = seq { while true do yield 0 }
 
//S |> Seq.take 10
 
let bernoulli p x = x <= p
 
 
S |> Seq.take 1000000 |> Seq.map (bernoulli 0.5) |> Seq.filter (fun v -> v) |> Seq.length
 
let binomial p n0 (s:System.Collections.Generic.IEnumerator<float>) =
  let bernoullip = bernoulli p
  let rec binomialp n =
    if n = 0 then 0.
    else
      let x = binomialp (n - 1)
      let b = bernoullip s.Current
      s.MoveNext() |> ignore
      if b then 1. + x else x
  binomialp n0
 
let ss = S.GetEnumerator()
ss.MoveNext() |> ignore
 
seq { while true do yield ss |> binomial 0.5 20 } |> Seq.take 1000000 |> Seq.countBy (fun v -> v) |> Seq.toList |> List.sort |> FSharpChart.Line |> FSharpChart.Create

let gaussianBoxMuller m sigma (s:System.Collections.Generic.IEnumerator<float>) =
  let u = s.Current
  s.MoveNext() |> ignore
  let v = s.Current
  s.MoveNext() |> ignore
  m + sigma * sqrt(-2. * log(u)) * cos(2. * System.Math.PI * v)
 
FSharpChart.Rows [
  seq { while true do yield (ss |> gaussianBoxMuller 0. 1.) * 2. } |> Seq.take 1000000 |> Seq.countBy (fun v -> floor(v * 100.) / 100.)|> Seq.toList |> FSharpChart.Column;
  seq { while true do yield (ss |> gaussianBoxMuller 0. 1.) + (ss |> gaussianBoxMuller 0. 1.) } |> Seq.take 1000000 |> Seq.countBy (fun v -> floor(v * 100.) / 100.)|> Seq.toList |> FSharpChart.Column
  seq {
   let s1 = S.GetEnumerator()
   let s2 = S.GetEnumerator()
   s1.MoveNext() |> ignore
   s2.MoveNext() |> ignore
   while true do yield (s1 |> gaussianBoxMuller 0. 1.) + (s2 |> gaussianBoxMuller 0. 1.) } |> Seq.take 1000000 |> Seq.countBy (fun v -> floor(v * 100.) / 100.)|> Seq.toList |> FSharpChart.Column
] |> FSharpChart.Create

 