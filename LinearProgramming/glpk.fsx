#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#I @"C:\progetti\Energon\bin\Debug"
#r @"testClass.dll"

#r @"GlpkSharp.dll"


open GlpkSharp

open testClass

let t = new Class1()
t.test2()

let rowbounds0 = new testClass.Class1.bound(BoundType = BOUNDSTYPE.Upper, lower = 0., upper=100.)
let rowbounds1 = new testClass.Class1.bound(BoundType = BOUNDSTYPE.Upper, lower = 0., upper=600.)
let rowbounds2 = new testClass.Class1.bound(BoundType = BOUNDSTYPE.Upper, lower = 0., upper=300.)
let rowbouds = [| rowbounds0; rowbounds1; rowbounds2 |]

let colbounds0 = new testClass.Class1.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbounds1 = new testClass.Class1.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbounds2 = new testClass.Class1.bound( BoundType = BOUNDSTYPE.Lower, lower = 0., upper = 0. )
let colbouds = [| colbounds0; colbounds1; colbounds2 |]


let ia = [| 0; 1; 1; 1; 2; 2; 2; 3; 3; 3 |]
let ja = [| 0; 1; 2; 3; 1; 2; 3; 1; 2; 3 |]
let ar = [| 0.; 1.; 1.; 1.; 10.; 4.; 5.; 2.; 2.; 6. |]

let coeff = [|10.; 6.; 4. |]

t.Simplex(3, 3, OptimisationDirection.MAXIMISE, rowbouds, colbouds, ia, ja, ar, coeff);




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
