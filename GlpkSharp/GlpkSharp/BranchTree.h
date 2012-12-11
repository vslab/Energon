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

#include "GlpkSharp.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::IO;

namespace GlpkSharp {

	public enum class IOSReason{
		SubProbSelectionRequest=GLP_ISELECT,
		PreprocessingRequest=GLP_IPREPRO,
		RowGenRequest=GLP_IROWGEN,
		HeuristicSolRequest=GLP_IHEUR,
		CutGenRequest=GLP_ICUTGEN,
		BranchingRequest=GLP_IBRANCH,
		BetterSolFound=GLP_IBINGO
	};

	public enum class RowOriginFlag{
		Regular=GLP_RF_REG,
		Lazy=GLP_RF_LAZY,
		Cut=GLP_RF_CUT
	};
	public enum class RowClass{
		GomoryMIPCut=GLP_RF_GMI,
		MIPRoundingCut=GLP_RF_MIR,
		CoverCut=GLP_RF_COV,
		CliqueCut=GLP_RF_CLQ
	};

	public struct RowAttributes
	{     /* additional row attributes */
		int level;
		/* subproblem level at which the row was added */
		RowOriginFlag origin;
		/* the row origin flag: */
		//#define GLP_RF_REG         0  /* regular constraint */
		//#define GLP_RF_LAZY        1  /* "lazy" constraint */
		//#define GLP_RF_CUT         2  /* cutting plane constraint */
		RowClass klass;
		/* the row class descriptor: */
		//#define GLP_RF_GMI         1  /* Gomory's mixed integer cut */
		//#define GLP_RF_MIR         2  /* mixed integer rounding cut */
		//#define GLP_RF_COV         3  /* mixed cover cut */
		//#define GLP_RF_CLQ         4  /* clique cut */
		//double foo_bar[7];
	};

	public enum class BranchDirection{
		Down=GLP_DN_BRNCH,
		Up=GLP_UP_BRNCH,
		General=GLP_NO_BRNCH
	};

	public ref class BranchTree{
	private:
		glp_tree * tree;
		char * AllocCharPointer(String^ s);
		void FreeCharPointer(char * sp);

	internal:
		BranchTree(glp_tree* t) ;
	public:
		IOSReason GetReason(void);
		LPProblem^ GetProblem(void) ;
		RowAttributes GetRowAttributes(int i) ;
		double GetMIPGap() ;
		void SelectNode(int p) ;
		bool SetHeuristicSolution(array<double>^ x) ;
		bool CanBranch(int p) ;
		void BranchOn(int j,BranchDirection dir) ;
		void TerminateSearch() ;
		property int ActiveNodesCount {int get();};
		property int CurrentNodesCount{int get();};
		property int AllNodesCount{ int get();};
		property int CurrentNode { int get();};

		int NextNode(int p);
		int PrevNode(int p);
		int ParentNode(int p);
		int NodeLevel(int p);
		double NodeBound(int p);
		int BestNode();
		property int CutPoolSize{int get();};

		int AddCut(String^ name,int userCutPlaneType,array<int>^ ind,array<double>^ coeff, BOUNDSTYPE bt,double rhs);
		void RemoveCut(int p) ;
		void ClearCutPool();

	};
}
