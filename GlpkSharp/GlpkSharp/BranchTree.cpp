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

#include "BranchTree.h"


namespace GlpkSharp {


	char * BranchTree::AllocCharPointer(String^ s){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(s).ToPointer());
			return sp;
		}
		void BranchTree::FreeCharPointer(char * sp){
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		}

	BranchTree::BranchTree(glp_tree* t)
	{
		tree=t;
	}
	
	IOSReason BranchTree::GetReason(){
		return IOSReason(glp_ios_reason(tree));
	}

	LPProblem^ BranchTree::GetProblem(){
		return gcnew LPProblem(glp_ios_get_prob(tree));
	}

	RowAttributes BranchTree::GetRowAttributes(int i){
		glp_attr att;
		glp_ios_row_attr(tree,i,&att);
		RowAttributes rowAtt;
		rowAtt.klass=RowClass(att.klass);
		rowAtt.level=att.level;
		rowAtt.origin=RowOriginFlag(att.origin);
		return rowAtt;
	}

	double BranchTree::GetMIPGap(){
		return glp_ios_mip_gap(tree);
	}

	void BranchTree::SelectNode(int p){
		glp_ios_select_node(tree,p);
	}

	bool BranchTree::SetHeuristicSolution(array<double>^ x){
		double* pval=new double[x->Length];
		IntPtr ipval=*new IntPtr(pval);
		Marshal::Copy(x,(int)0,ipval,x->Length);
		int i=glp_ios_heur_sol(tree,pval);
		return i==0;
	}

	bool BranchTree::CanBranch(int p){
		return glp_ios_can_branch(tree,p)!=0;
	}

	void BranchTree::BranchOn(int j,BranchDirection dir){
		glp_ios_branch_upon(tree,j,int(dir));
	}

	void BranchTree::TerminateSearch(){
		glp_ios_terminate(tree);
	}

	int BranchTree::ActiveNodesCount::get(){
			int a,b,c;
			glp_ios_tree_size(tree,&a,&b,&c);
			return a;
	}
	int BranchTree::CurrentNodesCount::get(){
			int a,b,c;
			glp_ios_tree_size(tree,&a,&b,&c);
			return b;
	}
	int BranchTree::AllNodesCount::get(){
			int a,b,c;
			glp_ios_tree_size(tree,&a,&b,&c);
			return c;
	}

	int BranchTree::CurrentNode::get(){
			
			return glp_ios_curr_node(tree);
	}
	int BranchTree::NextNode(int p){
		return glp_ios_next_node(tree,p);
	}
	int BranchTree::PrevNode(int p){
		return glp_ios_prev_node(tree,p);
	}
	int BranchTree::ParentNode(int p){
		return glp_ios_up_node(tree,p);
	}
	int BranchTree::NodeLevel(int p){
		return glp_ios_node_level(tree,p);
	}
	double BranchTree::NodeBound(int p){
		return glp_ios_node_bound(tree,p);
	}
	int BranchTree::BestNode(){
		return glp_ios_best_node(tree);
	}
	int BranchTree::CutPoolSize::get(){
		if(GetReason()== IOSReason::CutGenRequest){
			return glp_ios_pool_size(tree);
		}else{
			return 0;
		}
	}
//ind and coeff are one based arrays.
	int BranchTree::AddCut(String^ name,int userCutPlaneType,array<int>^ ind,array<double>^ coeff, BOUNDSTYPE bt,double rhs){
		double* pval=new double[coeff->Length];
		IntPtr ipval=*new IntPtr(pval);
		Marshal::Copy(coeff,(int)0,ipval,coeff->Length);
		
		int* pind=new int[ind->Length];
		IntPtr ipind=*new IntPtr(pind);
		Marshal::Copy(ind,(int)0,ipind,ind->Length);
		
		char * p=AllocCharPointer(name);

		int i= glp_ios_add_row(tree,p,userCutPlaneType,0,coeff->Length,pind,pval,int(bt),rhs);
		FreeCharPointer(p);
		delete(pind);
		delete(pval);

		return i;
	}
	void BranchTree::RemoveCut(int p){
		glp_ios_del_row(tree,p);
	}
	void BranchTree::ClearCutPool(){
		glp_ios_clear_pool(tree);
	}

}