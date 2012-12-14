using System;
using System.Collections.Generic;
using System.Text;
using GlpkSharp;
using System.Threading;
using System.IO;
using System.CodeDom.Compiler;

namespace TestGlpkSharp
{
    class Program
    {
        private class NullWriter : TextWriter
        {

            public override Encoding Encoding
            {
                get { throw new Exception("The method or operation is not implemented."); }
            }
        }
        static void Main(string[] args)
        {
            TestDavide();

            TestGeneral();
            TestLoadMatrix();
            TestMPGenerator();
            Console.Out.WriteLine("tapez une touche.");
            Console.In.ReadLine();
        }

        private static void TestDavide()
        {
            LPProblem p = new LPProblem();
            p.ObjectiveDirection = OptimisationDirection.MAXIMISE;
            p.AddCols(3);
            p.AddRows(3);
            p.SetRowBounds(1, BOUNDSTYPE.Upper, 0, 100);
            p.SetRowBounds(2, BOUNDSTYPE.Upper, 0, 600);
            p.SetRowBounds(3, BOUNDSTYPE.Upper, 0, 300);
            p.SetColBounds(1, BOUNDSTYPE.Lower, 0, 0);
            p.SetColBounds(2, BOUNDSTYPE.Lower, 0, 0);
            p.SetColBounds(3, BOUNDSTYPE.Lower, 0, 0);
            int[] ia = new int[] { 0, 1, 1, 1, 2, 2, 2, 3, 3, 3};
            int[] ja = new int[] { 0, 1, 2, 3, 1, 2, 3, 1, 2, 3};
            double[] ar = new double[] { 0, 1, 1, 1, 10, 4, 5, 2, 2, 6 };
            p.LoadMatrix(ia, ja, ar);
            //p.SetMatRow(1, new int[] {0, 1, 2 }, new double[] {0, 1, 1 });
            p.SetObjCoef(1, 10);
            p.SetObjCoef(2, 6);
            p.SetObjCoef(3, 4);
            p.SolveSimplex();
            Console.WriteLine("result = {0}, x1 = {1}, x2 = {2}", p.GetObjectiveValue(), p.GetColPrimal(1), p.GetColPrimal(2));
            Console.In.ReadLine();

        }

        private static void TestGeneral()
        {
            TextWriter tout = new NullWriter();


            LPProblem p = new LPProblem();

            tout.WriteLine(p.AddRows(4));
            tout.WriteLine(p.AddCols(5));


            p.Name = "yoyoyo";
            tout.WriteLine(p.Name);
            p.ObjectiveName = "monobj";
            tout.WriteLine(p.ObjectiveName);
            p.ObjectiveDirection = OptimisationDirection.MINIMISE;
            tout.WriteLine(p.ObjectiveDirection);
            p.ObjectiveDirection = OptimisationDirection.MAXIMISE;
            tout.WriteLine(p.ObjectiveDirection);


            p.SetRowName(1, "ert");
            tout.WriteLine(p.GetRowName(1));
            tout.WriteLine(p.GetRowName(2));
            p.SetRowName(1, null);
            tout.WriteLine(p.GetRowName(1));

            p.SetColName(1, "colk");
            tout.WriteLine(p.GetColName(1));
            p.SetColName(1, null);
            tout.WriteLine(p.GetColName(1));
            tout.WriteLine(p.GetColName(2));

            p.SetRowBounds(1, BOUNDSTYPE.Upper, 4, 6);
            p.SetColBounds(2, BOUNDSTYPE.Upper, 3, 7);
            p.SetMatRow(1, new int[] { 0, 1, 2, 4 }, new double[] { 0, 2.2, 3.3, 4.4 });
            int[] ind;
            double[] val;
            p.GetMatRow(1, out ind, out val);
            tout.WriteLine(ind.Length + " " + ind[1]);
            tout.WriteLine(val.Length + " " + val[1]);

            p.SetObjCoef(2, 3.6);
            tout.WriteLine(p.GetObjCoef(2));

            tout.WriteLine(p.GetNonZeroCount());
            tout.WriteLine(p.GetColBoundType(2));
            tout.WriteLine(p.GetColLb(2));
            tout.WriteLine(p.GetColUb(2));
            p.SetColKind(1, COLKIND.Integer);

            tout.WriteLine(p.GetColsCount());
            tout.WriteLine(p.GetRowsCount());
            Console.Out.WriteLine(p.GetVersion());
            p.TermHook(new TermHookDelegate(Hook));
            p.ModelClass = MODELCLASS.MIP;
            BnCCallback.SetCallback(p, 
                new BranchAndCutDelegate(
                    delegate(BranchTree t, Object o) 
                    {
                        Console.WriteLine(t.GetReason().ToString());
                    }
                    ));
            p.SolveSimplex();
            p.SolveInteger();
            p.WriteSol("c:\\sol.txt");
            KKT kkt = p.CheckKKT(0);
            int count;
            int cpeak;
            double total;
            double tpeak;
            p.GetMemoryUsage(out count, out cpeak, out total, out tpeak);
            Console.Out.WriteLine(count);
            Console.Out.WriteLine(cpeak);
            Console.Out.WriteLine(total);
            Console.Out.WriteLine(tpeak);

            LPProblem copy = p.Clone(true);
            p.Clear();

            Console.Out.WriteLine("tapez une touche.");
            Console.In.ReadLine();
            //Console.In.Read();
        }
        private static void TestLoadMatrix()
        {
            int[] ia = new int[10];
            int[] ja = new int[10];
            double[] ar = new double[10];

            ia[1] = 1; ja[1] = 1; ar[1] = 1.0;   /* a[1,1] =  1 */
            ia[2] = 1; ja[2] = 2; ar[2] = 1.0;   /* a[1,2] =  1 */
            ia[3] = 1; ja[3] = 3; ar[3] = 1.0;   /* a[1,3] =  1 */
            ia[4] = 2; ja[4] = 1; ar[4] = 10.0;   /* a[2,1] = 10 */
            ia[5] = 3; ja[5] = 1; ar[5] = 2.0;   /* a[3,1] =  2 */
            ia[6] = 2; ja[6] = 2; ar[6] = 4.0;   /* a[2,2] =  4 */
            ia[7] = 3; ja[7] = 2; ar[7] = 2.0;   /* a[3,2] =  2 */
            ia[8] = 2; ja[8] = 3; ar[8] = 5.0;   /* a[2,3] =  5 */
            ia[9] = 3; ja[9] = 3; ar[9] = 6.0;   /* a[3,3] =  6 */

            LPProblem lp = new LPProblem();
            lp.AddCols(3);
            lp.AddRows(3);
            lp.LoadMatrix(ia, ja, ar);
            lp.SetRowBounds(1, BOUNDSTYPE.Upper, 0, 100);
            lp.SetRowBounds(2, BOUNDSTYPE.Upper, 0, 600);
            lp.SetRowBounds(3, BOUNDSTYPE.Upper, 0, 300);
            lp.SetObjCoef(1, 10);
            lp.SetObjCoef(2, 6);
            lp.SetObjCoef(3, 4);
            lp.ObjectiveDirection = OptimisationDirection.MAXIMISE;
            //par défaut, les bornes sont à 0.
            lp.SetColBounds(1, BOUNDSTYPE.Lower, 0, 0);
            lp.SetColBounds(2, BOUNDSTYPE.Lower, 0, 0);
            lp.SetColBounds(3, BOUNDSTYPE.Lower, 0, 0);
            //lp.WriteCPLEX("yo.lp");
            Console.Out.WriteLine(lp.SolveSimplex().ToString());
            
        }

        static int Hook(String s)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Out.Write(s.ToUpper());
            Console.ForegroundColor = ConsoleColor.Gray;
            return 1;
        }

        private static void TestMPGenerator()
        {
            //il faut suivre cet ordre.
            var test = new MPTranslator();
            test.ReadModel("dist.mod",false);
            test.GenerateModel(null);
            var lp = test.GenerateProblem();
            lp.SolveSimplex();
            test.PostSolve(lp, SOLUTIONTYPE.BasicSolution);
            test.Dispose();
        }
    }
}
