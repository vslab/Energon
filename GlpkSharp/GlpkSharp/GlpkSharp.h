/*
Copyright ou © ou Copr. Lionel BERTON, (2008) 

pas d'adresse mail.

Ce logiciel est un programme informatique servant à appeler la librairie Gnu Linear Programming Kit depuis C#. 

Ce logiciel est régi par la licence CeCILL (v2) soumise au droit français et
respectant les principes de diffusion des logiciels libres. Vous pouvez
utiliser, modifier et/ou redistribuer ce programme sous les conditions
de la licence CeCILL (v2) telle que diffusée par le CEA, le CNRS et l'INRIA 
sur le site "http://www.cecill.info".

En contrepartie de l'accessibilité au code source et des droits de copie,
de modification et de redistribution accordés par cette licence, il n'est
offert aux utilisateurs qu'une garantie limitée.  Pour les mêmes raisons,
seule une responsabilité restreinte pèse sur l'auteur du programme,  le
titulaire des droits patrimoniaux et les concédants successifs.

A cet égard  l'attention de l'utilisateur est attirée sur les risques
associés au chargement,  à l'utilisation,  à la modification et/ou au
développement et à la reproduction du logiciel par l'utilisateur étant 
donné sa spécificité de logiciel libre, qui peut le rendre complexe à 
manipuler et qui le réserve donc à des développeurs et des professionnels
avertis possédant  des  connaissances  informatiques approfondies.  Les
utilisateurs sont donc invités à charger  et  tester  l'adéquation  du
logiciel à leurs besoins dans des conditions permettant d'assurer la
sécurité de leurs systèmes et ou de leurs données et, plus généralement, 
à l'utiliser et l'exploiter dans les mêmes conditions de sécurité. 

Le fait que vous puissiez accéder à cet en-tête signifie que vous avez 
pris connaissance de la licence CeCILL (v2), et que vous en avez accepté les
termes.
*/


#pragma once

extern "C" {
#include "glpk.h"

}

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::IO;


namespace GlpkSharp {

	public enum class OptimisationDirection{
		MINIMISE=GLP_MIN,
		MAXIMISE=GLP_MAX
	};

	public enum class BOUNDSTYPE{
		Free=GLP_FR,
		Lower=GLP_LO,
		Upper=GLP_UP,
		Double=GLP_DB,
		Fixed=GLP_FX
	};

	public enum class BASESTATUS{
		Basic=GLP_BS,
		NonBasicLower=GLP_NL,
		NonBasicUpper=GLP_NU,
		NonBasicFree=GLP_NF,
		NonBasicFixed=GLP_NS
	};

	public enum class SOLVERSTATUS{
		Solved=0,
		InvalidInitialBasis=GLP_EBADB,
		SingularBasisMatrix=GLP_ESING,
		IllConditionedMatrix=GLP_ECOND,
		IncorrectBoundsOnDoubleBoundedVariables=GLP_EBOUND,
		LowerBound=GLP_EOBJLL,
		UpperBound=GLP_EOBJUL,
		IterationLimit=GLP_EITLIM,
		TimeLimit=GLP_ETMLIM,
		SolverFailure=GLP_EFAIL,
		NoPrimalFeasible=GLP_ENOPFS,
		NoDualFeasible=GLP_ENODFS,
		NoLpRelaxation=GLP_EROOT,
		MIPGapToleraneReached=GLP_EMIPGAP,
		UserStop=GLP_ESTOP
	};
	public enum class SOLUTIONSTATUS{
		Optimal=GLP_OPT,
		Feasible=GLP_FEAS,
		Infeasible=GLP_INFEAS,
		NotFeasible=GLP_NOFEAS,
		Unbounded=GLP_UNBND,
		Undefined=GLP_UNDEF
	};
	public enum class WARMUP{
		OK=0,
		BadBasis=GLP_EBADB,
		Singular=GLP_ESING,
		IllConditioned=GLP_ECOND
	};

	public enum class INTERIORSTATUS{
		Optimal=0,
		Faulty=GLP_EFAIL,
		NotFeasible=GLP_ENOFEAS,
		NoConvergence=GLP_ENOCVG,
		IterationLimit=GLP_EITLIM,
		Instable=GLP_EINSTAB
	};
	public enum class MODELCLASS{
		LP=LPX_LP,
		MIP=LPX_MIP
	};
	public enum class COLKIND{
		Continuous=GLP_CV,
		Integer=GLP_IV,
		Binary=GLP_BV
	};
	public enum class MIPSTATUS{
		Undefined=LPX_I_UNDEF,
		Optimal=LPX_I_OPT,
		Feasible=LPX_I_FEAS,
		NotFeasible=LPX_I_NOFEAS
	};

	public enum class INTEGERPARAM{
		MessageLevel=LPX_K_MSGLEV,
		Scale=LPX_K_SCALE,
		Dual=LPX_K_DUAL,
		Price=LPX_K_PRICE,
		Round=LPX_K_ROUND,
		IterationLimit=LPX_K_ITLIM,
		ITCNT=LPX_K_ITCNT,
		OUTFRQ=LPX_K_OUTFRQ,
		BRANCH=LPX_K_BRANCH,
		BTRACK=LPX_K_BTRACK,
		MPSINFO=LPX_K_MPSINFO,
		MPSOBJ=LPX_K_MPSOBJ,
		MPSORIG=LPX_K_MPSORIG,
		MPSWIDE=LPX_K_MPSWIDE,
		MPSFREE=LPX_K_MPSFREE,
		MPSSKIP=LPX_K_MPSSKIP,
		PRESOL=LPX_K_PRESOL,
		USECUTS=LPX_K_USECUTS
	};
	public enum class REALPARAM{
		Relax=LPX_K_RELAX,
		Tolerance=LPX_K_TOLBND,
		Toldj=LPX_K_TOLDJ,
		Tolpiv=LPX_K_TOLPIV,
		Objll=LPX_K_OBJLL,
		Objul=LPX_K_OBJUL,
		TMLIM=LPX_K_TMLIM,
		OutDelay=LPX_K_OUTDLY,
		TOLINT=LPX_K_TOLINT,
		TOLOBJ=LPX_K_TOLOBJ,
	};
	public enum class CUTS{
		COVER=LPX_C_COVER,
		CLIQUE=LPX_C_CLIQUE,
		GOMORY=LPX_C_GOMORY,
		ALL=LPX_C_ALL
	};
	public enum class SCALINGFLAGS{
		GeometricMeanScaling=GLP_SF_GM,
		EquilibrationScaling=GLP_SF_EQ,
		NearestPowerOf2=GLP_SF_2N,
		Skip=GLP_SF_SKIP,
		Auto=GLP_SF_AUTO
	};

	public enum class MPSFORMAT{
		FixedMPS=GLP_MPS_DECK,
		FreeMPS=GLP_MPS_FILE
	};

	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	public delegate int TermHookDelegate(String^ s);
				
	
	public value struct KKT
{     /* this structure contains results reported by the routines which
         checks Karush-Kuhn-Tucker conditions (for details see comments
         to those routines) */
      /*--------------------------------------------------------------*/
      /* xR - A * xS = 0 (KKT.PE) */
      double pe_ae_max;
      /* largest absolute error */
      int    pe_ae_row;
      /* number of row with largest absolute error */
      double pe_re_max;
      /* largest relative error */
      int    pe_re_row;
      /* number of row with largest relative error */
      int    pe_quality;
      /* quality of primal solution:
         'H' - high
         'M' - medium
         'L' - low
         '?' - primal solution is wrong */
      /*--------------------------------------------------------------*/
      /* l[k] <= x[k] <= u[k] (KKT.PB) */
      double pb_ae_max;
      /* largest absolute error */
      int    pb_ae_ind;
      /* number of variable with largest absolute error */
      double pb_re_max;
      /* largest relative error */
      int    pb_re_ind;
      /* number of variable with largest relative error */
      int    pb_quality;
      /* quality of primal feasibility:
         'H' - high
         'M' - medium
         'L' - low
         '?' - primal solution is infeasible */
      /*--------------------------------------------------------------*/
      /* A' * (dR - cR) + (dS - cS) = 0 (KKT.DE) */
      double de_ae_max;
      /* largest absolute error */
      int    de_ae_col;
      /* number of column with largest absolute error */
      double de_re_max;
      /* largest relative error */
      int    de_re_col;
      /* number of column with largest relative error */
      int    de_quality;
      /* quality of dual solution:
         'H' - high
         'M' - medium
         'L' - low
         '?' - dual solution is wrong */
      /*--------------------------------------------------------------*/
      /* d[k] >= 0 or d[k] <= 0 (KKT.DB) */
      double db_ae_max;
      /* largest absolute error */
      int    db_ae_ind;
      /* number of variable with largest absolute error */
      double db_re_max;
      /* largest relative error */
      int    db_re_ind;
      /* number of variable with largest relative error */
      int    db_quality;
      /* quality of dual feasibility:
         'H' - high
         'M' - medium
         'L' - low
         '?' - dual solution is infeasible */
      /*--------------------------------------------------------------*/
      /* (x[k] - bound of x[k]) * d[k] = 0 (KKT.CS) */
      double cs_ae_max;
      /* largest absolute error */
      int    cs_ae_ind;
      /* number of variable with largest absolute error */
      double cs_re_max;
      /* largest relative error */
      int    cs_re_ind;
      /* number of variable with largest relative error */
      int    cs_quality;
      /* quality of complementary slackness:
         'H' - high
         'M' - medium
         'L' - low
         '?' - primal and dual solutions are not complementary */
};

	public enum class FactorisationType{
		ForrestTomlin=GLP_BF_FT,
		BarterlsGolub=GLP_BF_BG,
		GivensRotation=GLP_BF_GR
	};

public value struct BasisFactorisationCP
{     /* basis factorization control parameters */
      int msg_lev;            /* (reserved) */
      FactorisationType type;               /* factorization type: */
      int lu_size;            /* luf.sv_size */
      double piv_tol;         /* luf.piv_tol */
      int piv_lim;            /* luf.piv_lim */
      int suhl;               /* luf.suhl */
      double eps_tol;         /* luf.eps_tol */
      double max_gro;         /* luf.max_gro */
      int nfs_max;            /* fhv.hh_max */
      double upd_tol;         /* fhv.upd_tol */
      int nrs_max;            /* lpf.n_max */
      int rs_size;            /* lpf.v_size */
      //array<double>^ foo_bar=gcnew array<double>(38);     /* (reserved) */
};

public enum class IOCPMsg{
	None=GLP_MSG_OFF,
	WarningsErrors=GLP_MSG_ERR,
	Normal=GLP_MSG_ON,
	All=GLP_MSG_ALL,
	Debug=GLP_MSG_DBG
};

public enum class IOCPBranchingTechnique{
	FirstFractionalVariable=GLP_BR_FFV,
	LastFractionalVariable=GLP_BR_LFV,
	MostFractionalVariable=GLP_BR_MFV,
	HeuristicDriebeck=GLP_BR_DTH,
	HybridPseudocost=GLP_BR_PCH
};

public enum class IOCPBacktrackingTechnique{
	DepthFirstSearch=GLP_BT_DFS,
	BreadthFirstSearch=GLP_BT_BFS,
	BestLocalBound=GLP_BT_BLB,
	BestProjection=GLP_BT_BPH
};
public enum class IOCPPreprocessingTechnique{
	None=GLP_PP_NONE,
	RootLevelOnly=GLP_PP_ROOT,
	AllLevels=GLP_PP_ALL
};
public enum class GLPOnOff{
	On=GLP_ON,
	Off=GLP_OFF
};
public value struct IOCP
{     /* integer optimizer control parameters */
      IOCPMsg msg_lev;            /* message level: */
//#define GLP_MSG_OFF        0  /* no output */
//#define GLP_MSG_ERR        1  /* warning and error messages only */
//#define GLP_MSG_ON         2  /* normal output */
//#define GLP_MSG_ALL        3  /* full output */
//#define GLP_MSG_DBG        4  /* debug output */
      IOCPBranchingTechnique br_tech;            /* branching technique: */
//#define GLP_BR_FFV         1  /* first fractional variable */
//#define GLP_BR_LFV         2  /* last fractional variable */
//#define GLP_BR_MFV         3  /* most fractional variable */
//#define GLP_BR_DTH         4  /* heuristic by Driebeck and Tomlin */
//#define GLP_BR_HPC         5  /* hybrid pseudocost */
      IOCPBacktrackingTechnique bt_tech;            /* backtracking technique: */
//#define GLP_BT_DFS         1  /* depth first search */
//#define GLP_BT_BFS         2  /* breadth first search */
//#define GLP_BT_BLB         3  /* best local bound */
//#define GLP_BT_BPH         4  /* best projection heuristic */
      double tol_int;         /* mip.tol_int */
      double tol_obj;         /* mip.tol_obj */
      int tm_lim;             /* mip.tm_lim (milliseconds) */
      int out_frq;            /* mip.out_frq (milliseconds) */
      int out_dly;            /* mip.out_dly (milliseconds) */
      //void (*cb_func)(glp_tree *tree, void *info);
                              /* mip.cb_func */
      //void *cb_info;          /* mip.cb_info */
      //int cb_size;            /* mip.cb_size */
      IOCPPreprocessingTechnique pp_tech;            /* preprocessing technique: */
//#define GLP_PP_NONE        0  /* disable preprocessing */
//#define GLP_PP_ROOT        1  /* preprocessing only on root level */
//#define GLP_PP_ALL         2  /* preprocessing on all levels */
      double mip_gap;         /* relative MIP gap tolerance */
      GLPOnOff mir_cuts;           /* MIR cuts       (GLP_ON/GLP_OFF) */
      GLPOnOff gmi_cuts;           /* Gomory's cuts  (GLP_ON/GLP_OFF) */
      GLPOnOff cov_cuts;           /* cover cuts     (GLP_ON/GLP_OFF) */
      GLPOnOff clq_cuts;           /* clique cuts    (GLP_ON/GLP_OFF) */
      GLPOnOff presolve;           /* enable/disable using MIP presolver On/Off*/
      GLPOnOff binarize;           /* try to binarize integer variables On/OFF*/
	  GLPOnOff fp_heur;				/* feasibility pump heuristic ON/OFF*/
      //double foo_bar[30];     /* (reserved) */
//#if 1 /* not yet available */
//      //char *fn_sol;           /* file name to write solution found */
//#endif
};

public enum class SimplexMethod{
	Primal=GLP_PRIMAL,
	DualPrimal=GLP_DUALP,
	Dual=GLP_DUAL
};

public enum class Pricing{
	Standard=GLP_PT_STD,
	ProjectedSteepest=GLP_PT_PSE
};
public enum class RatioTest{
	Standard=GLP_RT_STD,
	Harris=GLP_RT_HAR
};

public value struct SMCP
{     /* simplex method control parameters */
      IOCPMsg msg_lev;            /* message level: */
//#define GLP_MSG_OFF        0  /* no output */
//#define GLP_MSG_ERR        1  /* warning and error messages only */
//#define GLP_MSG_ON         2  /* normal output */
//#define GLP_MSG_ALL        3  /* full output */
//#define GLP_MSG_DBG        4  /* debug output */
      SimplexMethod meth;               /* simplex method option: */
//#define GLP_PRIMAL         1  /* use primal simplex */
//#define GLP_DUALP          2  /* use dual; if it fails, use primal */
//#define GLP_DUAL           3  /* use dual simplex */
      Pricing pricing;            /* pricing technique: */
//#define GLP_PT_STD      0x11  /* standard (Dantzig rule) */
//#define GLP_PT_PSE      0x22  /* projected steepest edge */
      RatioTest r_test;             /* ratio test technique: */
//#define GLP_RT_STD      0x11  /* standard (textbook) */
//#define GLP_RT_HAR      0x22  /* two-pass Harris' ratio test */
      double tol_bnd;         /* spx.tol_bnd */
      double tol_dj;          /* spx.tol_dj */
      double tol_piv;         /* spx.tol_piv */
      double obj_ll;          /* spx.obj_ll */
      double obj_ul;          /* spx.obj_ul */
      int it_lim;             /* spx.it_lim */
      int tm_lim;             /* spx.tm_lim (milliseconds) */
      int out_frq;            /* spx.out_frq */
      int out_dly;            /* spx.out_dly (milliseconds) */
      int presolve;           /* enable/disable using LP presolver */
      //double foo_bar[36];     /* (reserved) */
};

public ref class LPProblem{
private:
	char * AllocCharPointer(String^ s){
		char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(s).ToPointer());
		return sp;
	}
	void FreeCharPointer(char * sp){
		Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
	}

internal:
	glp_iocp* glpiocp;
	glp_smcp* glpsmcp;
	glp_prob* prob;


	TermHookDelegate^ _termHook;
	LPProblem(glp_prob*p);

public:
LPProblem(); 
//destructeur équivalent à Dispose en C#. Appelé automatiquement. 
~LPProblem();
 
void deleteCurrentProb();
!LPProblem();

void Destroy(); 
LPProblem^ Clone(bool copySymbols); 
void Clear();

//glp_set_prob_name, glp_get_prob_name
property String^ Name{String^ get();void set(String^ s);}

property String^ ObjectiveName{ String^ get(); void set(String^ s); }

property OptimisationDirection ObjectiveDirection{OptimisationDirection get();void set(OptimisationDirection od);}  
int AddRows(int number);
void DeleteRows(array<int>^ rowsId);
int GetRowsCount();
int AddCols(int number);
void DeleteCols(array<int>^ colsId);
int GetColsCount();
void SetRowName(int i,String^ name);
String^ GetRowName(int i);
void SetColName(int i,String^ name);
String^ GetColName(int i);
void SetRowBounds(int i,BOUNDSTYPE bt, double lb, double ub);
BOUNDSTYPE GetRowBoundType(int i);
double GetRowLb(int i);
double GetRowUb(int i);
void SetColBounds(int i,BOUNDSTYPE bt, double lb, double ub);
BOUNDSTYPE GetColBoundType(int i);
double GetColLb(int i);
double GetColUb(int i);

void SetObjCoef(int i, double coeff);
double GetObjCoef(int i);
void SetMatRow(int i, array<int>^ ind, array<double>^ val);
int GetMatRow(int i,[Out]array<int>^ %ind,[Out]array<double>^ %val);
void SetMatCol(int i, array<int>^ ind, array<double>^ val);
int GetMatCol(int i,[Out]array<int>^ %ind,[Out]array<double>^ %val);
void LoadMatrix(array<int>^ rind,array<int>^ cind, array<double>^ val);
int GetNonZeroCount();
void CreateIndex();
void DeleteIndex();
int FindRow(String^ name);
int FindCol(String^ name);
void SetRowScaleFactor(int i, double rii);
double GetRowScaleFactor(int i);
void SetColScaleFactor(int i, double sjj);
double GetColScaleFactor(int i);
void ScaleProb(SCALINGFLAGS flags);
void UnscaleProb();
void BuildStdBasis();
void BuildAdvBasis();
void BuildBixbyBasis();
void SetRowStatus(int i, BASESTATUS s);
void SetColStatus(int i, BASESTATUS s);

property SMCP SimplexControlParams{SMCP get(); void set(SMCP smcp);};

SOLVERSTATUS SolveSimplex();
SOLVERSTATUS SolveSimplexExact();
SOLUTIONSTATUS GetStatus();
SOLUTIONSTATUS GetPrimalStatus();
SOLUTIONSTATUS GetDualStatus();
double GetObjectiveValue();
BASESTATUS GetRowStatus(int i);
double GetRowPrimal(int i);
double GetRowDual(int i);
BASESTATUS GetColStatus(int i);
double GetColPrimal(int i);
double GetColDual(int i);
int GetUnboundedVariableIndex();
KKT CheckKKT(int scaled);
WARMUP WarmUp();
int EvalTabRow(int i,array<int>^ ind,array<double>^ val);
int EvalTabCol(int i,array<int>^ ind,array<double>^ val);
int TransformRow(int i,array<int>^ ind,array<double>^ val);
int TransformCol(int i,array<int>^ ind,array<double>^ val);
int PrimalRatioTest(int i,array<int>^ ind,array<double>^ val,int how, double tol);
int DualRatioTest(int i,array<int>^ ind,array<double>^ val,int how, double tol);
INTERIORSTATUS InteriorPoint();
SOLUTIONSTATUS IPGetStatus();
double IPGetObjectiveValue();
double IPGetRowPrimal(int i);
double IPGetRowDual(int i);
double IPGetColPrimal(int i);
double IPGetColDual(int i);
property MODELCLASS ModelClass{
MODELCLASS get();
void set(MODELCLASS c);
}
void SetColKind(int i, COLKIND k);
COLKIND GetColKind(int i);
int GetIntegerColCount();
int GetBinaryColCount();

property IOCP IntegerOptControlParams{IOCP get();void set(IOCP iocp);};

SOLVERSTATUS SolveInteger();
MIPSTATUS GetMIPStatus();
double GetMIPObjectiveValue();
double GetMIPRowVal(int i);
double GetMIPColVal(int i);
void ResetParams();
void SetIntControlParam(int p, int val);
int GetIntControlParam(int p);
void SetRealControlParam(int p, double val);
double GetRealControlParam(int p);
bool ReadMPS(String^ fname,MPSFORMAT format);
bool WriteMPS(String^ fname,MPSFORMAT format);

bool ReadCPLEX(String^ fname);
bool WriteCPLEX(String^ fname);
bool ReadGLPK(String^ filename,int flags);
bool WriteGLPK(String^ filename,int flags);

bool WriteSol(String^ fname);
bool WriteBoundsSensitivity(String^ fname);
bool WriteIPS(String^ fname);
bool WriteMIP(String^ fname);
bool WriteRawSol(String^ fname);
bool WriteRawIPS(String^ fname);
bool WriteRawMIP(String^ fname);
bool ReadRawSol(String^ fname);
bool ReadRawIPS(String^ fname);
bool ReadRawMIP(String^ fname);


bool BasisFactorisationExists();
SOLVERSTATUS Factorise();
bool BasisFactorisationUpdated();
BasisFactorisationCP GetBFCP();
void SetBFCP(BasisFactorisationCP BFCP);
int GetBasisHeader(int k);
int GetRowIndexInBasisHeader(int i);
int GetColIndexInBasisheader(int i);
void ForwardTransformation(array<double>^ x);
void BackWardTransformation(array<double>^ x);
String^ GetVersion();
void GetMemoryUsage([Out] int% count,[Out] int% cpeak,[Out] double% total,[Out] double% tpeak);
void LimitMemory(int limit);
void TermHook(TermHookDelegate^ writer);
};

}
