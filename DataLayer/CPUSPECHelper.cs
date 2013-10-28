using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Mapping;
using DataModel;

namespace DataLayer
{
    
    public class SPECCPUHelper
    {
        public partial class SPECCPUDB_Mine : LinqToDB.Data.DataConnection
        {
            public ITable<DataModel.Environment> Environments { get { return this.GetTable<DataModel.Environment>(); } }
            public ITable<Experiment> Experiments { get { return this.GetTable<Experiment>(); } }
            public ITable<Program> Programs { get { return this.GetTable<Program>(); } }

            public SPECCPUDB_Mine()
            {
            }

            public SPECCPUDB_Mine(string configuration)
                : base(configuration)
            {
            }

            public SPECCPUDB_Mine(LinqToDB.DataProvider.IDataProvider provider, string conString)
                : base(provider, conString)
            {
            }

        }

        private long getEnv(SPECCPUDB_Mine db, string CpuName, string CpuMHz, string FPU, string Memory, string OS, string Compiler )
        {
                var envs = from e in db.Environments
                           where e.Compiler == Compiler
                           && e.CpuMHz == CpuMHz
                           && e.CpuName == CpuName
                           && e.FPU == FPU
                           && e.Memory == Memory
                           && e.OS == OS
                           select e;
                int envsCount = envs.Count();
                if (envsCount > 0)
                {
                    var firstEnv = envs.ElementAt(0);
                    return firstEnv.ID;
                } else
                    return -1;
        }

        private long getProg(SPECCPUDB_Mine db, string ProgramName)
        {
            var progs = db.Programs.Where(p => p.Name == ProgramName);
            int progCount = progs.Count();
            if (progCount > 0)
            {
                var p = progs.ElementAt(0);
                return p.ID;
            }
            else
                return -1;
        }

        public void SaveNewExperiment(string dbFile, string CpuName, string CpuMHz, string FPU, string Memory, string OS, string Compiler, string ProgramName, double CompletionTime)
        {
            using (var db = new SPECCPUDB_Mine(new LinqToDB.DataProvider.SQLite.SQLiteDataProvider(), String.Format(@"Data Source={0}", dbFile)))
            {

                long EnvID = getEnv(db, CpuName, CpuMHz, FPU, Memory, OS, Compiler);
                if (EnvID < 0)
                {
                    var newEnv = new DataModel.Environment();
                    newEnv.Compiler = Compiler;
                    newEnv.CpuMHz = CpuMHz;
                    newEnv.CpuName = CpuName;
                    newEnv.FPU = FPU;
                    newEnv.Memory = Memory;
                    newEnv.OS = OS;
                    db.Insert(newEnv);
                    EnvID = getEnv(db, CpuName, CpuMHz, FPU, Memory, OS, Compiler);
                }
                long ProgID = getProg(db, ProgramName);
                if (ProgID<0)
                {
                    var p = new DataModel.Program();
                    p.Name = ProgramName;
                    db.Insert(p);
                    ProgID = getProg(db, ProgramName);
                }
                var exp = new DataModel.Experiment();
                exp.EnvID = EnvID;
                exp.ProgID = ProgID;
                exp.CompletionTime = new decimal(CompletionTime);
                db.Insert(exp);
            }
        }

    }

}
