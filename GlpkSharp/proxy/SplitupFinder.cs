using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlpkSharp;


namespace GlpkProxy
{
    public class SplitupFinder
    {
        /// <summary>
        /// the set of programs used as generators of the vector space. The resource consumption of the target program will be expressed as a linear combination of the programs in the testbed. Is the A of the Ax=y, an m by n matrix with n columns (programs) and m rows (resources measured), x is the splitup, y is the vector of the resource consumption of the target program
        /// </summary>
        public Program [] Testbed { get; set; }

        /// <summary>
        /// the target program, we will find its splitup vector using the Testbed. It' s the y in the Ax=y equation
        /// </summary>
        public Program Target { get; set; }

        /// <summary>
        /// The target program as a linear combination of the programs in the Testbed. It' s the x in the Ax=y equation
        /// </summary>
        public double [] Splitup { get; set; }

        /// <summary>
        /// the sum of the elements of the error vector E, defined as E=|Ax-Y|
        /// </summary>
        public double TotalError { get; set; }

        /// <summary>
        /// the error vector E, defined as E=|Ax-Y|
        /// </summary>
        public double [] Errors { get; set; }

        /// <summary>
        /// Finds the splitup vector of the Target program using the programs of the Testbed as generators. Solves the Ax=Y equation using the simplex. Also finds the error vector E, defined as E=|Ax-Y|.
        /// </summary>
        /// <returns>true if the simplex found a solution</returns>
        public bool FindSplitup()
        {
            // do we have at least 1 program in the testbed?
            if (Testbed.Length < 1)
                return false;
            int resources = Testbed[0].Measures.Length;
            // does every program have the same number of measured resources?
            foreach (var item in Testbed)
            {
                if (item.Measures == null || item.Measures.Length != resources)
                    return false;
            }
            // check target program
            if (Target == null || Target.Measures == null)
                return false;
            if (Target.Measures.Length != resources)
                return false;

            // input looks fine.. let' s build the LP problem
            // how many columns?
            // 1 column for every program
            // + 2 for every resource
            // this is a trick to transform the absolute value (non linear function) into a linear function,
            // so we can apply the simplex, see APPLIED MATHEMATICAL PROGRAMMING USING ALGEBRAIC SYSTEMS, CHAPTER IX: Linear Programming Modeling: non linearities and approximation, B. McCarl and T. Spreen
            // URL: http://chentserver.uwaterloo.ca/aelkamel/che720/che725-process-optimization/GAMS-tutorials/Bruce/thebook.pdf
            int programs = Testbed.Length;
            int errorVariables = programs * 2;
            int columns = programs + errorVariables;
            List<int> ia = new List<int>();
            List<int> ja = new List<int>();
            List<double> ar = new List<double>();
            // glpk counts from 1 to n, not from 0 to n-1.. silly boy
            ia.Add(0); ja.Add(0); ar.Add(0);
            for (int column = 0; column < Testbed.Length; column++)
            {
                for (int row = 0; row < resources; row++)
                {
                    // glpk counts from 1 to n, not from 0 to n-1.. silly boy
                    ia.Add(row + 1);
                    ja.Add(column + 1);
                    ar.Add(Testbed[column].Measures[row]);
                }
            }
            // now add epsilon+ and epsilon-
            for (int i = 0; i < resources; i++)
            {
                // epsilon+
                ia.Add(i + 1);
                ja.Add(programs + 2*i + 1);
                ar.Add(1);
                // epsilon-
                ia.Add(i + 1);
                ja.Add(programs + 2 * i + 1 + 1);
                ar.Add(-1);
            }
            // the coefficient array, this does not start from 1
            List<double> coeff = new List<double>();
            for (int i = 0; i < Testbed.Length; i++)
            {
                coeff.Add(0);
            }
            for (int i = 0; i < errorVariables; i++)
            {
                coeff.Add(1);
            }
            // the Y array
            List<Proxy.bound> rowBounds = new List<Proxy.bound>();
            foreach (var item in Target.Measures)
	        {
                Proxy.bound b = new Proxy.bound();
                b.BoundType = BOUNDSTYPE.Fixed;
                b.lower = item;
                b.upper = item;
                rowBounds.Add(b);
	        }
            List<Proxy.bound> colBounds = new List<Proxy.bound>();
            for (int i = 0; i < columns; i++)
            {
                Proxy.bound b = new Proxy.bound();
                b.BoundType = BOUNDSTYPE.Lower;
                b.lower = 0;
                b.upper = 0;
                colBounds.Add(b);
            }
            Proxy p = new Proxy();
            Proxy.result result = p.Simplex(
                columns,
                resources,
                OptimisationDirection.MINIMISE,
                rowBounds.ToArray(),
                colBounds.ToArray(),
                ia.ToArray(),
                ja.ToArray(),
                ar.ToArray(),
                coeff.ToArray());
            List<double> splitup = new List<double>();
            for (int i = 0; i < Testbed.Length; i++)
            {
                splitup.Add(result.Columns[i]);
            }
            Splitup = splitup.ToArray();
            TotalError = result.ObjResult;
            List<double> errors = new List<double>();
            for (int i = 0; i < resources; i++)
            {
                double epsilon_plus = result.Columns[programs + i * 2];
                double epsilon_minus = result.Columns[programs + i * 2 + 1];
                errors.Add(epsilon_plus - epsilon_minus);
            }
            Errors = errors.ToArray();
            return true;
        }

    }
}
