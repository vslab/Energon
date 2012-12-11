// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#I @"C:\Users\root\Desktop\Energon\bin\Debug"
#I @"C:\progetti\Energon\bin\Debug"
#r @"GLPKSharp.dll"

//#I "C:\Users\root\Desktop\Energon\LinearProgramming"
//#I "C:\progetti\Energon\LinearProgramming"
//#r @"GlpkSharp.dll"

let prob = new GLPKSharp.GLPKSharp()

prob.AddCols(3)
prob.AddRows(3)

let ia : int array = Array.zeroCreate 10
let ja : int array = Array.zeroCreate 10
let a : float array = Array.zeroCreate 10
let c = ref 1
let setMatCell i j v =
    ia.[!c] <- i
    ja.[!c] <- j
    a.[!c] <- v
    c := !c + 1

setMatCell 1 1 1.
setMatCell 1 2 1.
setMatCell 1 3 1.

setMatCell 2 1 10.
setMatCell 2 2 4.
setMatCell 2 3 5.

setMatCell 3 1 2.
setMatCell 3 2 2.
setMatCell 3 3 6.

prob.LoadMatrix(ia, ja, a)

prob.SetRowBnds(1, GLPKSharp.Boundary.UpperBound, 0., 100.)
prob.SetRowBnds(2, GLPKSharp.Boundary.UpperBound, 0., 600.)
prob.SetRowBnds(3, GLPKSharp.Boundary.UpperBound, 0., 300.)

prob.SetObjCoef(1, 10.)
prob.SetObjCoef(2, 6.)
prob.SetObjCoef(3, 4.)

prob.SetObjectiveDirection(GLPKSharp.ObjectiveDirection.Minimize)

prob.Simplex()

prob.GetObjVal()

prob.GetColPrim(1)
prob.GetColPrim(2)
prob.GetColPrim(3)




// ---------------------------- 

let prob = new GLPKSharp.GLPKSharp()

prob.AddCols(2)
prob.AddRows(1)

let ia : int array = Array.zeroCreate 3
let ja : int array = Array.zeroCreate 3
let a : float array = Array.zeroCreate 3
let c = ref 1
let setMatCell i j v =
    ia.[!c] <- i
    ja.[!c] <- j
    a.[!c] <- v
    c := !c + 1

setMatCell 1 1 1.
setMatCell 1 2 1.

prob.LoadMatrix(ia, ja, a)

prob.SetRowBnds(1, GLPKSharp.Boundary.UpperBound, 0., 2.)

prob.SetObjCoef(1, 1.)
prob.SetObjCoef(2, 1.)

prob.SetObjectiveDirection(GLPKSharp.ObjectiveDirection.Maximize)

prob.Simplex()

prob.GetObjVal()

prob.GetColPrim(1)
prob.GetColPrim(2)
prob.GetColPrim(3)





