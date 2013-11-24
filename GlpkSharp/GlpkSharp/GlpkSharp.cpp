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

// This is the main DLL file.


#include "GlpkSharp.h"



namespace GlpkSharp{

	int Hook(void *info,const char*s){
		TermHookDelegate^ d=(TermHookDelegate^)Marshal::GetDelegateForFunctionPointer((IntPtr)(info),TermHookDelegate::typeid);
		return d(gcnew String(s));
	};

		LPProblem::LPProblem(glp_prob*p){
			prob=p;
			glp_init_iocp(glpiocp);
			glp_init_smcp(glpsmcp);
		} 
		
		void LPProblem::deleteCurrentProb(){
			if(prob!=0){
				glp_delete_prob(prob);
			}
		}

		LPProblem::!LPProblem(){
		}


		//event BranchAndCutDelegate^ LPProblem::BranchAndCut;

		LPProblem::LPProblem(){
			glpsmcp=new glp_smcp;
			//glpsmcp=(glp_smcp*)xmalloc(sizeof(glp_smcp));
			glpiocp=new glp_iocp;//=(glpiocp*)xmalloc(sizeof(glp_iocp));
			glp_init_iocp(glpiocp);
			glp_init_smcp(glpsmcp);
			prob=glp_create_prob();
		}

		
		//destructeur équivalent à Dispose en C#. Appelé automatiquement.
		LPProblem::~LPProblem(){

		}

		void LPProblem::Destroy(){
			glp_delete_prob(prob);
			TermHook(nullptr);
		}

		LPProblem^ LPProblem::Clone(bool copySymbols){
			int copyNames=(copySymbols?GLP_ON:GLP_OFF);
			LPProblem^ p=gcnew LPProblem();
			glp_copy_prob(p->prob,prob,copyNames);
			return p;
		}
		void LPProblem::Clear(){
			glp_erase_prob(prob);
		}
		String^ LPProblem::Name::get(){
				return gcnew String(glp_get_prob_name(prob));
			}
		void LPProblem::Name::set(String^ s){
				char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(s).ToPointer());
				glp_set_prob_name(prob,sp);
				Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		}

		String^ LPProblem::ObjectiveName::get(){
				return gcnew String(glp_get_obj_name(prob));
			}
		void LPProblem::ObjectiveName::set(String^ s){
				char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(s).ToPointer());
				glp_set_obj_name(prob,sp);
				Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		}

		OptimisationDirection LPProblem::ObjectiveDirection::get(){
				return OptimisationDirection( glp_get_obj_dir(prob));
			}
		void LPProblem::ObjectiveDirection::set(OptimisationDirection dir){
				glp_set_obj_dir(prob,int(dir));
		}

		/*
		Add rows to the problem and return the index of the first row added.
		*/
		int LPProblem::AddRows(int number){
			return glp_add_rows(prob,number);
		}
		void LPProblem::DeleteRows(array<int>^ rowsId){
			pin_ptr<int> pint=&rowsId[0];
			glp_del_rows(prob,rowsId->Length-1,pint);
		}
		int LPProblem::GetRowsCount(){
			return glp_get_num_rows(prob);
		}

		int LPProblem::AddCols(int number){
			return glp_add_cols(prob,number);
		}
		void LPProblem::DeleteCols(array<int>^ colsId){
			pin_ptr<int> pint=&colsId[0];
			glp_del_cols(prob,colsId->Length-1,pint);
		}
		int LPProblem::GetColsCount(){
			return glp_get_num_cols(prob);
		}

		void LPProblem::SetRowName(int i,String^ name){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(name).ToPointer());
			glp_set_row_name(prob,i,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		}

		String^ LPProblem::GetRowName(int i){
			return gcnew String(glp_get_row_name(prob,i));
		}

		void LPProblem::SetColName(int i,String^ name){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(name).ToPointer());
			glp_set_col_name(prob,i,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		}

		String^ LPProblem::GetColName(int i){
			return gcnew String(glp_get_col_name(prob,i));
		}

		void LPProblem::SetRowBounds(int i,BOUNDSTYPE bt, double lb, double ub){
			glp_set_row_bnds(prob,i,int(bt),lb,ub);
		}

		BOUNDSTYPE LPProblem::GetRowBoundType(int i){
			return BOUNDSTYPE(glp_get_row_type(prob,i));
		}
		double LPProblem::GetRowLb(int i){
			return glp_get_row_lb(prob,i);
		}
		double LPProblem::GetRowUb(int i){
			return glp_get_row_ub(prob,i);
		}

		void LPProblem::SetColBounds(int i,BOUNDSTYPE bt, double lb, double ub){
			glp_set_col_bnds(prob,i,int(bt),lb,ub);
		}
		BOUNDSTYPE LPProblem::GetColBoundType(int i){
			return BOUNDSTYPE(glp_get_col_type(prob,i));
		}
		double LPProblem::GetColLb(int i){
			return glp_get_col_lb(prob,i);
		}
		double LPProblem::GetColUb(int i){
			return glp_get_col_ub(prob,i);
		}

		/*
		if i=0 then it sets teh constant term of the objective function.
		*/
		void LPProblem::SetObjCoef(int i, double coeff){
			glp_set_obj_coef(prob,i,coeff);
		}
		double LPProblem::GetObjCoef(int i){
			return glp_get_obj_coef(prob,i);
		}

		void LPProblem::SetMatRow(int i, array<int>^ ind, array<double>^ val){
			pin_ptr<int> pint=&ind[0];
			pin_ptr<double> pdouble=&val[0];
			glp_set_mat_row(prob,i,ind->Length-1,pint,pdouble);
		}
		int LPProblem::GetMatRow(int i,[Out]array<int>^ %ind,[Out]array<double>^ %val){
			int numcols=GetColsCount();
			int* pind=new int[numcols+1];
			double* pval=new double[numcols+1];
			int num=glp_get_mat_row(prob,i,pind,pval);
			ind=gcnew array<int>(num+1);
			val=gcnew array<double>(num+1);
			IntPtr ipind=*new IntPtr(pind);
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(ipind,ind,(int)0,num+1);
			Marshal::Copy(ipval,val,(int)0,num+1);
			delete(pind);
			delete(pval);
			return num;
		}

		void LPProblem::SetMatCol(int i, array<int>^ ind, array<double>^ val){
			pin_ptr<int> pint=&ind[0];
			pin_ptr<double> pdouble=&val[0];
			glp_set_mat_col(prob,i,ind->Length-1,pint,pdouble);
		}
		int LPProblem::GetMatCol(int i,[Out]array<int>^ %ind,[Out]array<double>^ %val){
			int numrows=GetRowsCount();
			int* pind=new int[numrows+1];
			double* pval=new double[numrows+1];
			int num=glp_get_mat_col(prob,i,pind,pval);
			ind=gcnew array<int>(num+1);
			val=gcnew array<double>(num+1);
			IntPtr ipind=*new IntPtr(pind);
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(ipind,ind,(int)0,num+1);
			Marshal::Copy(ipval,val,(int)0,num+1);
			delete(pind);
			delete(pval);
			return num;
		}

		void LPProblem::LoadMatrix(array<int>^ rind,array<int>^ cind, array<double>^ val){
			pin_ptr<int> rpint=&rind[0];
			pin_ptr<int> cpint=&cind[0];
			pin_ptr<double> pdouble=&val[0];
			glp_load_matrix(prob,rind->Length-1,rpint,cpint,pdouble);
		}

		int LPProblem::GetNonZeroCount(){
			return glp_get_num_nz(prob);
		}
		/*create an index for searching row or column names in logarithmic time*/
		void LPProblem::CreateIndex(){
			glp_create_index(prob);
		}
		void LPProblem::DeleteIndex(){
			glp_delete_index(prob);
		}
		int LPProblem::FindRow(String^ name){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(name).ToPointer());
			int i=glp_find_row(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i;
		}
		int LPProblem::FindCol(String^ name){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(name).ToPointer());
			int i=glp_find_col(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i;
		}



		void LPProblem::SetRowScaleFactor(int i, double rii){
			glp_set_rii(prob,i,rii);
		}
		double LPProblem::GetRowScaleFactor(int i){
			return glp_get_rii(prob,i);
		}
		void LPProblem::SetColScaleFactor(int i, double sjj){
			glp_set_sjj(prob,i,sjj);
		}
		double LPProblem::GetColScaleFactor(int i){
			return glp_get_sjj(prob,i);
		}

		void LPProblem::ScaleProb(SCALINGFLAGS flags){
			glp_scale_prob(prob,int(flags));
		}
		void LPProblem::UnscaleProb(){
			glp_unscale_prob(prob);
		}
		void LPProblem::BuildStdBasis(){
			glp_std_basis(prob);
		}
		void LPProblem::BuildAdvBasis(){
			glp_adv_basis(prob,0);
		}
		void LPProblem::BuildBixbyBasis(){
			glp_cpx_basis(prob);
		}
		void LPProblem::SetRowStatus(int i, BASESTATUS s){
			glp_set_row_stat(prob,i,(int)s);
		}
		void LPProblem::SetColStatus(int i, BASESTATUS s){
			glp_set_col_stat(prob,i,(int)s);
		}

		SMCP LPProblem::SimplexControlParams::get(){
			SMCP smcp;

			smcp.it_lim=glpsmcp->it_lim;
			smcp.meth=SimplexMethod(glpsmcp->meth);
			smcp.msg_lev=IOCPMsg(glpsmcp->msg_lev);
			smcp.obj_ll=glpsmcp->obj_ll;
			smcp.obj_ul=glpsmcp->obj_ul;
			smcp.out_dly=glpsmcp->out_dly;
			smcp.out_frq=glpsmcp->out_frq;
			smcp.presolve=glpsmcp->presolve;
			smcp.pricing=Pricing(glpsmcp->pricing);
			smcp.r_test=RatioTest(glpsmcp->r_test);
			smcp.tm_lim=glpsmcp->tm_lim;
			smcp.tol_bnd=glpsmcp->tol_bnd;
			smcp.tol_dj=glpsmcp->tol_dj;
			smcp.tol_piv=glpsmcp->tol_piv;

			return smcp;
		}
		void LPProblem::SimplexControlParams::set(SMCP smcp){
			glpsmcp->it_lim=smcp.it_lim;
			glpsmcp->meth=int(smcp.meth);
			glpsmcp->msg_lev=int(smcp.msg_lev);
			glpsmcp->obj_ll=smcp.obj_ll;
			glpsmcp->obj_ul=smcp.obj_ul;
			glpsmcp->out_dly=smcp.out_dly;
			glpsmcp->out_frq=smcp.out_frq;
			glpsmcp->presolve=smcp.presolve;
			glpsmcp->pricing=int(smcp.pricing);
			glpsmcp->r_test=int(smcp.r_test);
			glpsmcp->tm_lim=smcp.tm_lim;
			glpsmcp->tol_bnd=smcp.tol_bnd;
			glpsmcp->tol_dj=smcp.tol_dj;
			glpsmcp->tol_piv=smcp.tol_piv;
		}

		SOLVERSTATUS LPProblem::SolveSimplex(){
			return SOLVERSTATUS(glp_simplex(prob,glpsmcp));
		}
		SOLVERSTATUS LPProblem::SolveSimplexExact(){
			return SOLVERSTATUS(glp_exact(prob,glpsmcp));
		}
		SOLUTIONSTATUS LPProblem::GetStatus(){
			return SOLUTIONSTATUS(glp_get_status(prob));
		}
		SOLUTIONSTATUS LPProblem::GetPrimalStatus(){
			return SOLUTIONSTATUS(glp_get_prim_stat(prob));
		}
		SOLUTIONSTATUS LPProblem::GetDualStatus(){
			return SOLUTIONSTATUS(glp_get_dual_stat(prob));
		}

		double LPProblem::GetObjectiveValue(){
			return glp_get_obj_val(prob);
		}

		BASESTATUS LPProblem::GetRowStatus(int i){
			return BASESTATUS(glp_get_row_stat(prob,i));
		}
		double LPProblem::GetRowPrimal(int i){
			return glp_get_row_prim(prob,i);
		}
		double LPProblem::GetRowDual(int i){
			return glp_get_row_dual(prob,i);
		}

		BASESTATUS LPProblem::GetColStatus(int i){
			return BASESTATUS(glp_get_col_stat(prob,i));
		}
		double LPProblem::GetColPrimal(int i){
			return glp_get_col_prim(prob,i);
		}
		double LPProblem::GetColDual(int i){
			return glp_get_col_dual(prob,i);
		}
		int LPProblem::GetUnboundedVariableIndex(){
			return glp_get_unbnd_ray(prob);
		}
		KKT LPProblem::CheckKKT(int scaled){
			LPXKKT lpxkkt;
			lpx_check_kkt(prob,scaled,&lpxkkt);
			KKT kkt;

			kkt.cs_ae_ind=lpxkkt.cs_ae_ind;
			kkt.cs_ae_max=lpxkkt.cs_ae_max;
			kkt.cs_quality=lpxkkt.cs_quality;
			kkt.cs_re_ind=lpxkkt.cs_re_ind;
			kkt.cs_re_max=lpxkkt.cs_re_max;
			kkt.db_ae_ind=lpxkkt.db_ae_ind;
			kkt.db_ae_max=lpxkkt.db_ae_max;
			kkt.db_quality=lpxkkt.db_quality;
			kkt.db_re_ind=lpxkkt.db_re_ind;
			kkt.db_re_max=lpxkkt.db_re_max;
			kkt.de_ae_col=lpxkkt.de_ae_col;
			kkt.de_ae_max=lpxkkt.de_ae_max;
			kkt.de_quality=lpxkkt.de_quality;
			kkt.de_re_col=lpxkkt.de_re_col;
			kkt.de_re_max=lpxkkt.de_re_max;
			kkt.pb_ae_ind=lpxkkt.pb_ae_ind;
			kkt.pb_ae_max=lpxkkt.pb_ae_max;
			kkt.pb_quality=lpxkkt.pb_quality;
			kkt.pb_re_ind=lpxkkt.pb_re_ind;
			kkt.pb_re_max=lpxkkt.pb_re_max;
			kkt.pe_ae_max=lpxkkt.pe_ae_max;
			kkt.pe_ae_row=lpxkkt.pe_ae_row;
			kkt.pe_quality=lpxkkt.pe_quality;
			kkt.pe_re_max=lpxkkt.pe_re_max;
			kkt.pe_re_row=lpxkkt.pe_re_row;

			return kkt;
		}
		WARMUP LPProblem::WarmUp(){
			return WARMUP(glp_warm_up(prob));
		}

		int LPProblem::EvalTabRow(int i,array<int>^ ind,array<double>^ val){
			int numcols=GetColsCount();
			int* pind=new int[numcols+1];
			double* pval=new double[numcols+1];
			int num=lpx_eval_tab_row(prob,i,pind,pval);
			ind=gcnew array<int>(num+1);
			val=gcnew array<double>(num+1);
			IntPtr ipind=*new IntPtr(pind);
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(ipind,ind,(int)0,num+1);
			Marshal::Copy(ipval,val,(int)0,num+1);
			delete(pind);
			delete(pval);
			return num;
		}

		int LPProblem::EvalTabCol(int i,array<int>^ ind,array<double>^ val){
			int numcols=GetRowsCount();
			int* pind=new int[numcols+1];
			double* pval=new double[numcols+1];
			int num=lpx_eval_tab_col(prob,i,pind,pval);
			ind=gcnew array<int>(num+1);
			val=gcnew array<double>(num+1);
			IntPtr ipind=*new IntPtr(pind);
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(ipind,ind,(int)0,num+1);
			Marshal::Copy(ipval,val,(int)0,num+1);
			delete(pind);
			delete(pval);
			return num;
		}

		int LPProblem::TransformRow(int i,array<int>^ ind,array<double>^ val){
			int numcols=i;
			int* pind=new int[numcols];
			IntPtr ipind=*new IntPtr(pind);
			Marshal::Copy(ind,(int)0,ipind,numcols);
			double* pval=new double[numcols];
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(val,(int)0,ipval,numcols);

			int num=glp_transform_row(prob,i,pind,pval);

			Marshal::Copy(ipind,ind,(int)0,num+1);
			Marshal::Copy(ipval,val,(int)0,num+1);
			delete(pind);
			delete(pval);
			return num;
		}

		int LPProblem::TransformCol(int i,array<int>^ ind,array<double>^ val){
			int numcols=i;
			int* pind=new int[numcols];
			IntPtr ipind=*new IntPtr(pind);
			Marshal::Copy(ind,(int)0,ipind,numcols);
			double* pval=new double[numcols];
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(val,(int)0,ipval,numcols);

			int num=glp_transform_col(prob,i,pind,pval);

			Marshal::Copy(ipind,ind,(int)0,num+1);
			Marshal::Copy(ipval,val,(int)0,num+1);
			delete(pind);
			delete(pval);
			return num;
		}
		int LPProblem::PrimalRatioTest(int i,array<int>^ ind,array<double>^ val,int how, double tol){
			int numcols=i;
			int* pind=new int[numcols];
			IntPtr ipind=*new IntPtr(pind);
			Marshal::Copy(ind,(int)0,ipind,numcols);
			double* pval=new double[numcols];
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(val,(int)0,ipval,numcols);

			int num=glp_prim_rtest(prob,i,pind,pval,how,tol);

			delete(pind);
			delete(pval);
			return num;
		}
		int LPProblem::DualRatioTest(int i,array<int>^ ind,array<double>^ val,int how, double tol){
			int numcols=i;
			int* pind=new int[numcols];
			IntPtr ipind=*new IntPtr(pind);
			Marshal::Copy(ind,(int)0,ipind,numcols);
			double* pval=new double[numcols];
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(val,(int)0,ipval,numcols);

			int num=glp_dual_rtest(prob,i,pind,pval,how,tol);

			delete(pind);
			delete(pval);
			return num;
		}


		INTERIORSTATUS LPProblem::InteriorPoint(){
			return INTERIORSTATUS(glp_interior(prob,0));
		}
		SOLUTIONSTATUS LPProblem::IPGetStatus(){
			return SOLUTIONSTATUS(glp_ipt_status(prob));
		}

		double LPProblem::IPGetObjectiveValue(){
			return glp_ipt_obj_val(prob);
		}
		double LPProblem::IPGetRowPrimal(int i){
			return glp_ipt_row_prim(prob,i);
		}
		double LPProblem::IPGetRowDual(int i){
			return glp_ipt_row_dual(prob,i);
		}
		double LPProblem::IPGetColPrimal(int i){
			return glp_ipt_col_prim(prob,i);
		}
		double LPProblem::IPGetColDual(int i){
			return glp_ipt_col_dual(prob,i);
		}

		MODELCLASS LPProblem::ModelClass::get(){
				return MODELCLASS(lpx_get_class(prob));
		}
		void LPProblem::ModelClass::set(MODELCLASS c){
				lpx_set_class(prob,(int)c);
		}

		void LPProblem::SetColKind(int i, COLKIND k){
			glp_set_col_kind(prob,i,(int)k);
		}
		COLKIND LPProblem::GetColKind(int i){
			return COLKIND(glp_get_col_kind(prob, i));
		}
		int LPProblem::GetIntegerColCount(){
			return glp_get_num_int(prob);
		}
		int LPProblem::GetBinaryColCount(){
			return glp_get_num_bin(prob);
		}

		//SOLVERSTATUS SolveInteger(){
		//	return SOLVERSTATUS(lpx_integer(prob));
		//}
		IOCP LPProblem::IntegerOptControlParams::get(){
			IOCP iocp;
			iocp.binarize=GLPOnOff(glpiocp->binarize);
			iocp.br_tech=IOCPBranchingTechnique( glpiocp->br_tech);
			iocp.bt_tech=IOCPBacktrackingTechnique( glpiocp->bt_tech);
			//iocp.cb_func=glpiocp->cb_func;
			//iocp.cb_info=glpiocp->cb_info;
			//iocp.cb_size=glpiocp->cb_size;
			iocp.clq_cuts=GLPOnOff(glpiocp->clq_cuts);
			iocp.cov_cuts=GLPOnOff(glpiocp->cov_cuts);
			//iocp.fn_sol=glpiocp->fn_sol;
			//iocp.foo_bar=glpiocp->foo_bar;
			iocp.gmi_cuts=GLPOnOff(glpiocp->gmi_cuts);
			iocp.mip_gap=glpiocp->mip_gap;
			iocp.mir_cuts=GLPOnOff(glpiocp->mir_cuts);
			iocp.msg_lev=IOCPMsg( glpiocp->msg_lev);
			iocp.out_dly=glpiocp->out_dly;
			iocp.out_frq=glpiocp->out_frq;
			iocp.pp_tech=IOCPPreprocessingTechnique( glpiocp->pp_tech);
			iocp.presolve=GLPOnOff(glpiocp->presolve);
			iocp.tm_lim=glpiocp->tm_lim;
			iocp.tol_int=glpiocp->tol_int;
			iocp.tol_obj=glpiocp->tol_obj;
			iocp.fp_heur=GLPOnOff(glpiocp->fp_heur);
			return iocp;
		}
		void LPProblem::IntegerOptControlParams::set(IOCP iocp){
			glpiocp->binarize = int(iocp.binarize);
			glpiocp->br_tech = int(iocp.br_tech);
			glpiocp->bt_tech = int(iocp.bt_tech);
			//glpiocp->cb_func = iocp.cb_func;
			//glpiocp->cb_info = iocp.cb_info;
			//glpiocp->cb_size = iocp.cb_size;
			glpiocp->clq_cuts = int(iocp.clq_cuts);
			glpiocp->cov_cuts = int(iocp.cov_cuts);
			//glpiocp->fn_sol = iocp.fn_sol;
			//glpiocp->foo_bar = iocp.foo_bar;
			glpiocp->gmi_cuts = int(iocp.gmi_cuts);
			glpiocp->mip_gap = iocp.mip_gap;
			glpiocp->mir_cuts = int(iocp.mir_cuts);
			glpiocp->msg_lev = int(iocp.msg_lev);
			glpiocp->out_dly = iocp.out_dly;
			glpiocp->out_frq = iocp.out_frq;
			glpiocp->pp_tech = int(iocp.pp_tech);
			glpiocp->presolve = int(iocp.presolve);
			glpiocp->tm_lim = iocp.tm_lim;
			glpiocp->tol_int = iocp.tol_int;
			glpiocp->tol_obj = iocp.tol_obj;
			glpiocp->fp_heur=int(iocp.fp_heur);
		}
		SOLVERSTATUS LPProblem::SolveInteger(){
			
			return SOLVERSTATUS(glp_intopt(prob,glpiocp));
		}
		MIPSTATUS LPProblem::GetMIPStatus(){
			return MIPSTATUS(glp_mip_status(prob));
		}
		double LPProblem::GetMIPObjectiveValue(){
			return glp_mip_obj_val(prob);
		}
		double LPProblem::GetMIPRowVal(int i){
			return glp_mip_row_val(prob,i);
		}
		double LPProblem::GetMIPColVal(int i){
			return glp_mip_col_val(prob,i);
		}

		void LPProblem::ResetParams(){
			lpx_reset_parms(prob);
		}
		void LPProblem::SetIntControlParam(int p, int val){
			lpx_set_int_parm(prob,p,val);
		}
		int LPProblem::GetIntControlParam(int p){
			return lpx_get_int_parm(prob,p);
		}
		void LPProblem::SetRealControlParam(int p, double val){
			lpx_set_real_parm(prob,p,val);
		}
		double LPProblem::GetRealControlParam(int p){
			return lpx_get_real_parm(prob,p);
		}

		bool LPProblem::ReadMPS(String^ fname,MPSFORMAT format){
			//LPProblem^ p=gcnew LPProblem(1);
			//deleteCurrentProb();
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_read_mps(prob,int(format),0,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}

		bool LPProblem::WriteMPS(String^ fname,MPSFORMAT format){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_write_mps(prob,int(format),0,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}
		//int ReadLPBasis(String^ fname){
		//	char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
		//	int i=lpx_read_bas(prob,sp);
		//	Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		//	return i;
		//}
		//int WriteLPBasis(String^ fname){
		//	char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
		//	int i=lpx_write_bas(prob,sp);
		//	Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		//	return i;
		//}
		//bool ReadFreeMPS(String^ fname){
		//	//LPProblem^ p=gcnew LPProblem(1);
		//	deleteCurrentProb();
		//	char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
		//	prob=lpx_read_freemps(sp);
		//	Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		//	return prob!=0;
		//}

		//int WriteFreeMPS(String^ fname){
		//	char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
		//	int i=lpx_write_freemps(prob,sp);
		//	Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		//	return i;
		//}
		bool LPProblem::ReadCPLEX(String^ fname){
			//LPProblem^ p=gcnew LPProblem(1);
			//deleteCurrentProb();
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_read_lp(prob,0,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}

		bool LPProblem::WriteCPLEX(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_write_lp(prob,0,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}
		//bool ReadModel(String^ model,String^ data, String^ output){
		//	//LPProblem^ p=gcnew LPProblem(1);
		//	deleteCurrentProb();
		//	char * spmodel=static_cast<char *>(Marshal::StringToHGlobalAnsi(model).ToPointer());
		//	char * spdata=static_cast<char *>(Marshal::StringToHGlobalAnsi(data).ToPointer());
		//	char * spoutput=static_cast<char *>(Marshal::StringToHGlobalAnsi(output).ToPointer());
		//	prob=lpx_read_model(spmodel,spdata,spoutput);
		//	Marshal::FreeHGlobal(safe_cast<IntPtr>(spmodel));
		//	Marshal::FreeHGlobal(safe_cast<IntPtr>(spdata));
		//	Marshal::FreeHGlobal(safe_cast<IntPtr>(spoutput));
		//	return prob!=0;
		//}
		bool LPProblem::ReadGLPK(String^ filename,int flags){
			char*p=AllocCharPointer(filename);
			int i=glp_read_prob(prob,flags,p);
			FreeCharPointer(p);
			return i==0;
		}
		bool LPProblem::WriteGLPK(String^ filename,int flags){
			char*p=AllocCharPointer(filename);
			int i=glp_write_prob(prob,flags,p);
			FreeCharPointer(p);
			return i==0;
		}
		bool LPProblem::WriteSol(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_print_sol(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}
		bool LPProblem::WriteBoundsSensitivity(String^ fname){
			char * sp=AllocCharPointer(fname);;
			int i=glp_print_ranges(prob,0,NULL,0,sp);
			FreeCharPointer(sp);
			return i==0;
		}
		bool LPProblem::WriteIPS(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_print_ipt(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}
		bool LPProblem::WriteMIP(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_print_mip(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}


		bool LPProblem::WriteRawSol(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_write_sol(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}
		bool LPProblem::WriteRawIPS(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_write_ipt(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}
		bool LPProblem::WriteRawMIP(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_write_mip(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}

		bool LPProblem::ReadRawSol(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_read_sol(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}
		bool LPProblem::ReadRawIPS(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_read_ipt(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}
		bool LPProblem::ReadRawMIP(String^ fname){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(fname).ToPointer());
			int i=glp_read_mip(prob,sp);
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
			return i==0;
		}

		/*advanced LP*/
		bool LPProblem::BasisFactorisationExists(){
			return glp_bf_exists(prob)!=0;
		}

		SOLVERSTATUS LPProblem::Factorise(){
			return SOLVERSTATUS(glp_factorize(prob));
		}
		bool LPProblem::BasisFactorisationUpdated(){
			return glp_bf_updated(prob)!=0;
		}

		BasisFactorisationCP LPProblem::GetBFCP(){
			glp_bfcp  bfcp;
			glp_get_bfcp(prob,&bfcp);

			BasisFactorisationCP BFCP;

			BFCP.eps_tol=bfcp.eps_tol;
			BFCP.lu_size=bfcp.lu_size;
			BFCP.max_gro=bfcp.max_gro;
			BFCP.msg_lev=bfcp.msg_lev;
			BFCP.nfs_max=bfcp.nfs_max;
			BFCP.nrs_max=bfcp.nrs_max;
			BFCP.piv_lim=bfcp.piv_lim;
			BFCP.piv_tol=bfcp.piv_tol;
			BFCP.rs_size=bfcp.rs_size;
			BFCP.suhl=bfcp.suhl;
			BFCP.type=FactorisationType(bfcp.type);
			BFCP.upd_tol=bfcp.upd_tol;
			return BFCP;
		}

		void LPProblem::SetBFCP(BasisFactorisationCP BFCP){
			glp_bfcp bfcp;

			bfcp.eps_tol=BFCP.eps_tol;
			bfcp.lu_size=BFCP.lu_size;
			bfcp.max_gro=BFCP.max_gro;
			bfcp.msg_lev=BFCP.msg_lev;
			bfcp.nfs_max=BFCP.nfs_max;
			bfcp.nrs_max=BFCP.nrs_max;
			bfcp.piv_lim=BFCP.piv_lim;
			bfcp.piv_tol=BFCP.piv_tol;
			bfcp.rs_size=BFCP.rs_size;
			bfcp.suhl=BFCP.suhl;
			bfcp.type=int(BFCP.type);
			bfcp.upd_tol=BFCP.upd_tol;

			glp_set_bfcp(prob,&bfcp);
		}

		int LPProblem::GetBasisHeader(int k){
			return glp_get_bhead(prob,k);
		}
		int LPProblem::GetRowIndexInBasisHeader(int i){
			return glp_get_row_bind(prob,i);
		}
		int LPProblem::GetColIndexInBasisheader(int i){
			return glp_get_col_bind(prob,i);
		}
		void LPProblem::ForwardTransformation(array<double>^ x){
			int num=x->Length;
			double* pval=new double[num];
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(x,(int)0,ipval,num);
			glp_ftran(prob,pval);
			Marshal::Copy(ipval,x,(int)0,num);
			delete(pval);
		}
		void LPProblem::BackWardTransformation(array<double>^ x){
			int num=x->Length;
			double* pval=new double[num];
			IntPtr ipval=*new IntPtr(pval);
			Marshal::Copy(x,(int)0,ipval,num);
			glp_btran(prob,pval);
			Marshal::Copy(ipval,x,(int)0,num);
			delete(pval);
		}

		String^ LPProblem::GetVersion(){
			return gcnew String(glp_version());
		}
		void LPProblem::GetMemoryUsage([Out] int% count,[Out] int% cpeak,[Out] double% total,[Out] double% tpeak){
			glp_long * _total=new glp_long();
			glp_long* _tpeak=new glp_long();
			int a;
			int b;
			glp_mem_usage((int*)&a,(int*)&b,_total,_tpeak);
			total=_total->hi*long::MaxValue + _total->lo;
			tpeak=_tpeak->hi*long::MaxValue + _tpeak->lo;
			count=a;
			cpeak=b;
		}

		void LPProblem::LimitMemory(int limit){
			glp_mem_limit(limit);
		}

		void LPProblem::TermHook(TermHookDelegate^ writer){
			if(writer!=nullptr){
				if(_termHook!=nullptr){
					glp_term_hook(0,0);
				}
				_termHook=writer;
				//TermHookDelegate^ d=gcnew TermHookDelegate(Hook);
				//IntPtr^ p=Marshal::GetFunctionPointerForDelegate(d);
				glp_term_hook(Hook,Marshal::GetFunctionPointerForDelegate(writer).ToPointer());
			}else{
				glp_term_hook(0,0);
			}
		}

}
