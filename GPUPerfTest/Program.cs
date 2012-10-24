using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Energon.Measuring;
using Energon.Measuring.Remote;
using KeyPi;

namespace GPUPerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RemoteSensorHelper remote = new RemoteSensorHelper(args[0]);
            KeyPiProfiler profiler;
            
            //Parameters
            int min_vector_size = 2 << 10;
            int max_vector_size = 64 << 20;
            int threads = 4;
            float a = 2.0f;
            List<int[]> device_indexes = new List<int[]>();
            device_indexes.Add(new int[] { 0 });
            device_indexes.Add(new int[] { 1 });
            device_indexes.Add(new int[] { 2 });
            device_indexes.Add(new int[] { 0, 1 });
            device_indexes.Add(new int[] { 0, 2 });
            device_indexes.Add(new int[] { 1, 2 });
            device_indexes.Add(new int[] { 0, 1, 2 });
            int test_duration = 10000;

            //Test host sequential
            for (int vector_size = min_vector_size; vector_size <= max_vector_size; vector_size *= 2)
            {
                bool done = false;
                int samples = 1;
                String[] dllArgs = null;
                KeyPiDllKernelExecutionInfo info = null;
                while (!done)
                {
                    //Profile to check how much samples needed to fill test duration
                    dllArgs = new String[] {
		                vector_size.ToString(), 
		                samples.ToString(), 
		                "1", 
		                a.ToString(), 
		                "0", 
		                "0", 
		                "1", 
		                "0", 
		                @"C:\Users\gabriele\APUBenchmarks\Debug\saxpy.cl",
		                "",
		                "0", "128", "1", "1", "saxpy4"
                    };
                    profiler = new KeyPiProfiler("Saxpy", "SaxpyLib.dll", dllArgs);
                    try
                    {
                        profiler.OpenKernel();
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }
                    info = profiler.Run();
                    try
                    {
                        profiler.CloseKernel();
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }
                    if ((info.EndTime - info.StartTime).TotalMilliseconds > 0)
                    {
                        samples = (int)Math.Round((float)(test_duration * samples) / (float)(info.EndTime - info.StartTime).TotalMilliseconds);
                        done = true;
                    }
                    else 
                        samples *= 100;
                }

                Console.WriteLine("Testing host sequential with " + vector_size + " elements (" + samples + " samples)...");
                dllArgs = new String[] {
		            vector_size.ToString(), 
		            samples.ToString(), 
		            "1", 
		            a.ToString(), 
		            "0", 
		            "0", 
		            "1", 
		            "0", 
		            @"C:\Users\gabriele\APUBenchmarks\Debug\saxpy.cl",
		            "",
		            "0", "128", "1", "1", "saxpy4"
                };
                String[] experimentArgs = new String[] {
                    "HOST_SEQ",
		            vector_size.ToString(), 
		            samples.ToString(), 
		            "1",  
		            "1", 
		            "0", 
		            "0", "0", "0",
		            "0", "0", "0",
		            "0", "0", "0"
                };
                remote.experimentCase(experimentArgs);

                profiler = new KeyPiProfiler(
                    "Saxpy",
                    "SaxpyLib.dll",
                    dllArgs);
                try
                {
                    profiler.OpenKernel();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                }
                remote.start();
                info = profiler.Run();
                remote.stop(new String[] { (((float)(info.EndTime - info.StartTime).TotalMilliseconds) / (float)samples).ToString() });
                try
                {
                    profiler.CloseKernel();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                }

                Console.WriteLine("Result: " + (((float)(info.EndTime - info.StartTime).TotalMilliseconds) / (float)samples).ToString() + "ms per iteration");
            }
           
            //Test host native threads
            for (int vector_size = min_vector_size; vector_size <= max_vector_size; vector_size *= 2)
            {
                bool done = false;
                int samples = 1;
                String[] dllArgs = null;
                KeyPiDllKernelExecutionInfo info = null;
                while (!done)
                {
                    //Profile to check how much samples needed to fill test duration
                    dllArgs = new String[] {
		                vector_size.ToString(), 
		                samples.ToString(), 
		                "1", 
		                a.ToString(), 
		                "0", 
		                "0", 
		                threads.ToString(), 
		                "0", 
		                @"C:\Users\gabriele\APUBenchmarks\Debug\saxpy.cl",
		                "",
		                "0", "128", "1", "1", "saxpy4"
                    };
                    profiler = new KeyPiProfiler("Saxpy", "SaxpyLib.dll", dllArgs);
                    try
                    {
                        profiler.OpenKernel();
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }
                    info = profiler.Run();
                    try
                    {
                        profiler.CloseKernel();
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }
                    if ((info.EndTime - info.StartTime).TotalMilliseconds > 0)
                    {
                        samples = (int)Math.Round((float)(test_duration * samples) / (float)(info.EndTime - info.StartTime).TotalMilliseconds);
                        done = true;
                    }
                    else
                        samples *= 100;
                }

                Console.WriteLine("Testing host native threads with " + vector_size + " elements (" + samples + " samples)...");
                dllArgs = new String[] {
		            vector_size.ToString(), 
		            samples.ToString(), 
		            "1", 
		            a.ToString(), 
		            "0", 
		            "0", 
		            threads.ToString(), 
		            "0", 
		            @"C:\Users\gabriele\APUBenchmarks\Debug\saxpy.cl",
		            "",
		            "0", "128", "1", "1", "saxpy4"
                };
                String[] experimentArgs = new String[] {
                    "HOST_THREADS",
		            vector_size.ToString(), 
		            samples.ToString(), 
		            "1",  
		            threads.ToString(), 
		            "0", 
		            "0", "0", "0",
		            "0", "0", "0",
		            "0", "0", "0"
                };
                remote.experimentCase(experimentArgs);

                profiler = new KeyPiProfiler(
                    "Saxpy",
                    "SaxpyLib.dll",
                    dllArgs);
                try
                {
                    profiler.OpenKernel();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                }
                                  
                remote.start();                
                info = profiler.Run();
                remote.stop(new String[] { (((float)(info.EndTime - info.StartTime).TotalMilliseconds) / (float)samples).ToString() });

                try
                {
                    profiler.CloseKernel();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                }

                Console.WriteLine("Result: " + (((float)(info.EndTime - info.StartTime).TotalMilliseconds) / (float)samples).ToString() + "ms per iteration");
            }
        }
    }
}
