using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlpkSharp;

namespace GlpkProxy
{
    public class Proxy
    {
        public class bound
        {
            public BOUNDSTYPE BoundType { get; set; }
            public double lower { get; set; }
            public double upper { get; set; }
        }

        public class result
        {
            public double ObjResult { get; set; }
            public double[] Columns { get; set; }
        }

        

        public result Simplex(int cols, int rows, OptimisationDirection direction, bound[] rowbounds, bound[] colbounds, int[] ia, int[] ja, double[] ar, double[] coeff)
        {
            LPProblem p = new LPProblem();
            p.ObjectiveDirection = direction;
            p.AddCols(cols);
            p.AddRows(rows);
            for (int i = 0; i < rowbounds.Length; i++)
            {
                p.SetRowBounds(i + 1, rowbounds[i].BoundType, rowbounds[i].lower, rowbounds[i].upper); 
            }
            for (int i = 0; i < colbounds.Length; i++)
            {
                p.SetColBounds(i + 1, colbounds[i].BoundType, colbounds[i].lower, colbounds[i].upper);
            }
            p.LoadMatrix(ia, ja, ar);
            //p.SetMatRow(1, new int[] {0, 1, 2 }, new double[] {0, 1, 1 });
            for (int i = 0; i < coeff.Length; i++)
            {
                p.SetObjCoef(i+1, coeff[i]);                
            }
            p.SolveSimplex();
            Console.WriteLine("result = {0}, x1 = {1}, x2 = {2}", p.GetObjectiveValue(), p.GetColPrimal(1), p.GetColPrimal(2));
            //Console.In.ReadLine();
            result r = new result();
            r.ObjResult = p.GetObjectiveValue();
            r.Columns = new double[cols];
            for (int i = 0; i < cols; i++)
            {
                r.Columns[i] = p.GetColPrimal(i + 1);
            }
            return r;
        }
        /*
        public double test()
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
            int[] ia = new int[] { 0, 1, 1, 1, 2, 2, 2, 3, 3, 3 };
            int[] ja = new int[] { 0, 1, 2, 3, 1, 2, 3, 1, 2, 3 };
            double[] ar = new double[] { 0, 1, 1, 1, 10, 4, 5, 2, 2, 6 };
            p.LoadMatrix(ia, ja, ar);
            //p.SetMatRow(1, new int[] {0, 1, 2 }, new double[] {0, 1, 1 });
            p.SetObjCoef(1, 10);
            p.SetObjCoef(2, 6);
            p.SetObjCoef(3, 4);
            p.SolveSimplex();
            Console.WriteLine("result = {0}, x1 = {1}, x2 = {2}", p.GetObjectiveValue(), p.GetColPrimal(1), p.GetColPrimal(2));
            //Console.In.ReadLine();
            return p.GetObjectiveValue();
        }

        public double test2()
        {
            bound[] rowbounds = new bound[3];
            rowbounds[0] = new bound() { BoundType = BOUNDSTYPE.Upper, lower = 0, upper = 100 };
            rowbounds[1] = new bound() { BoundType = BOUNDSTYPE.Upper, lower = 0, upper = 600 };
            rowbounds[2] = new bound() { BoundType = BOUNDSTYPE.Upper, lower = 0, upper = 300 };

            bound[] colbounds = new bound[3];
            colbounds[0] = new bound() { BoundType = BOUNDSTYPE.Lower, lower = 0, upper = 0 };
            colbounds[1] = new bound() { BoundType = BOUNDSTYPE.Lower, lower = 0, upper = 0 };
            colbounds[2] = new bound() { BoundType = BOUNDSTYPE.Lower, lower = 0, upper = 0 };

            int[] ia = new int[] { 0, 1, 1, 1, 2, 2, 2, 3, 3, 3 };
            int[] ja = new int[] { 0, 1, 2, 3, 1, 2, 3, 1, 2, 3 };
            double[] ar = new double[] { 0, 1, 1, 1, 10, 4, 5, 2, 2, 6 };

            double[] coeff = new double[] {10, 6, 4 };

            return Simplex(3, 3, OptimisationDirection.MAXIMISE, rowbounds, colbounds, ia, ja, ar, coeff);

        }
         * */

    }
}
