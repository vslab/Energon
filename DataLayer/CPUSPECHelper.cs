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

        private long getExp(SPECCPUDB_Mine db, string url, long EnvID, long ProgID, decimal BaseRefTime, decimal BaseRunTime)
        {
            var exps = db.Experiments.Where(e => e.url == url && e.EnvID == EnvID && e.ProgID == ProgID && e.BaseRefTime == BaseRefTime && e.BaseRunTime == BaseRunTime);
            int expsCount = exps.Count();
            if (expsCount > 0)
            {
                var e = exps.ElementAt(0);
                return e.ID;
            }
            else
                return -1;
        }

        public void SaveNewExperiment(string dbFile, string url, string CpuName, string CpuMHz, string FPU, string Memory, string OS, string Compiler, string ProgramName, decimal BaseRefTime, decimal BaseRunTime)
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
                var expID = getExp(db, url, EnvID, ProgID, BaseRefTime, BaseRunTime);
                if (expID < 0)
                {
                    var exp = new DataModel.Experiment();
                    exp.url = url;
                    exp.EnvID = EnvID;
                    exp.ProgID = ProgID;
                    exp.BaseRefTime = BaseRefTime;
                    exp.BaseRunTime = BaseRunTime;
                    db.Insert(exp);
                }
            }
        }

        public SPECCPUDB_Mine getDB(string dbFile)
        {
            return new SPECCPUDB_Mine(new LinqToDB.DataProvider.SQLite.SQLiteDataProvider(), String.Format(@"Data Source={0}", dbFile));
        }

        public Experiment[] getExperimentsOfEnvironment(SPECCPUDB_Mine db, long ID)
        {
            return db.Experiments.Where(e => e.EnvID == ID).OrderBy(e => e.ProgID).ToArray();
        }

        public Program getProgram(SPECCPUDB_Mine db, long ID)
        {
            return db.Programs.First(p => p.ID == ID);
        }

        public Program[] getPrograms(SPECCPUDB_Mine db)
        {
            return db.Programs.Where(p => true).ToArray();
        }

        public DataModel.Environment[] getEnvironments(SPECCPUDB_Mine db)
        {
            return db.Environments.Where(p => true).ToArray();
        }

        public Experiment[] getExperimentOfEnvironment(SPECCPUDB_Mine db, long EnvID)
        {
            var exps = 
        }

    }

}
