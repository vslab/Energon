// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#I "C:\Users\root\Desktop\Energon\LinearProgramming"
#r @"GlpkSharp.dll"

open GlpkSharp

let prob = new LPProblem()
prob.AddCols(2)
prob.AddRows(2)
prob.ObjectiveDirection = OptimisationDirection.MAXIMISE

let ia : int array = Array.zeroCreate 4
let ja : int array = Array.zeroCreate 4
let a : float array = Array.zeroCreate 4
let c = ref 0
let setMatCell i j v =
    ia.[!c] <- i
    ja.[!c] <- j
    a.[!c] <- v
    c := !c + 1

setMatCell 1 1 1.
setMatCell 1 2 1.
setMatCell 2 1 1.
setMatCell 2 2 2.

//prob.SetMatRow(1, [| 1; 2; |], [| 2.; 2. |]);
//prob.SetMatRow(2, [| 1; 2; |], [| 3.; 1. |]);

prob.LoadMatrix(ia, ja, a)
prob.SetRowBounds(1, BOUNDSTYPE.Fixed, 2., 2.)
prob.SetRowBounds(2, BOUNDSTYPE.Fixed, 4., 4.)
prob.SetColBounds(1, BOUNDSTYPE.Double, 0., 2.)
prob.SetColBounds(2, BOUNDSTYPE.Double, 2., 4.)
prob.SetColName(1, "pippo")
prob.SetColName(2, "pluto")
prob.SetObjCoef(1, 1.)
prob.SetObjCoef(2, 1.)
prob.SolveSimplex()

prob.GetObjectiveValue()
prob.GetColPrimal(1)
prob.GetColPrimal(2)

prob.WriteSol("c:\\test.txt")


