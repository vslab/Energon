#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#I @"C:\progetti\Energon\bin\Debug"
#r @"GlpkProxy.dll"

open GlpkProxy

let p1 = new Program()
p1.Name <- "p1"
p1.Measures <- [| 2.; 1. |]

let p2 = new Program()
p2.Name <- "p2"
p2.Measures <- [| 1.; 2. |]

let t = new Program()
t.Name <- "target"
t.Measures <- [| 1.; 1. |]

let finder = new SplitupFinder()
finder.Target <- t
finder.Testbed <- [| p1; p2 |]
finder.FindSplitup()
finder.Splitup
finder.Errors
finder.TotalError

#r @"GlpkSharp.dll"


open GlpkSharp


let t = new Proxy()

let rowbounds0 = new GlpkProxy.Proxy.bound(BoundType = BOUNDSTYPE.Fixed, lower = 1., upper=100.)
let rowbounds1 = new GlpkProxy.Proxy.bound(BoundType = BOUNDSTYPE.Fixed, lower = 1., upper=600.)
let rowbouds = [| rowbounds0; rowbounds1|]

let colbounds0 = new GlpkProxy.Proxy.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbounds1 = new GlpkProxy.Proxy.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbounds2 = new GlpkProxy.Proxy.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbounds3 = new GlpkProxy.Proxy.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbounds4 = new GlpkProxy.Proxy.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbounds5 = new GlpkProxy.Proxy.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbouds = [| colbounds0; colbounds1; colbounds2; colbounds3; colbounds4; colbounds5 |]


let ia = [| 0; 1; 1; 1; 1; 2; 2; 2; 2 |]
let ja = [| 0; 1; 2; 3; 4; 1; 2; 5; 6 |]
let ar = [| 0.; 2.; 1.; 1.; -1.; 1.; 2.; 1.; -1. |]

let coeff = [| 0.; 0.; 1.; 1.; 1.; 1. |]

let r = t.Simplex(6, 2, OptimisationDirection.MINIMISE, rowbouds, colbouds, ia, ja, ar, coeff);

r.ObjResult
r.Columns




let p = new LPProblem()
p.ObjectiveDirection = OptimisationDirection.MAXIMISE
p.AddCols(3)
p.AddRows(3)
p.SetRowBounds(1, BOUNDSTYPE.Upper, 0., 100.)
p.SetRowBounds(2, BOUNDSTYPE.Upper, 0., 600.)
p.SetRowBounds(3, BOUNDSTYPE.Upper, 0., 300.)
p.SetColBounds(1, BOUNDSTYPE.Lower, 0., 0.)
p.SetColBounds(2, BOUNDSTYPE.Lower, 0., 0.)
p.SetColBounds(3, BOUNDSTYPE.Lower, 0., 0.)
let ia = [| 0; 1; 1; 1; 2; 2; 2; 3; 3; 3|]
let ja = [| 0; 1; 2; 3; 1; 2; 3; 1; 2; 3|]
let ar = [| 0.; 1.; 1.; 1.; 10.; 4.; 5.; 2.; 2.; 6. |]
p.LoadMatrix(ia, ja, ar)
//p.SetMatRow(1, new int[] {0, 1, 2 }, new double[] {0, 1, 1 });
p.SetObjCoef(1, 10.)
p.SetObjCoef(2, 6.)
p.SetObjCoef(3, 4.)
p.SolveSimplex()

open System
Console.WriteLine("result = {0}, x1 = {1}, x2 = {2}", p.GetObjectiveValue(), p.GetColPrimal(1), p.GetColPrimal(2))
Console.In.ReadLine()
