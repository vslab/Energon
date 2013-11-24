/*
Copyright ou � ou Copr. Lionel BERTON, (2008) 

pas d'adresse mail.

Ce logiciel est un programme informatique servant � appeler la librairie Gnu Linear Programming Kit depuis C#. 

Ce logiciel est r�gi par la licence CeCILL (v2) soumise au droit fran�ais et
respectant les principes de diffusion des logiciels libres. Vous pouvez
utiliser, modifier et/ou redistribuer ce programme sous les conditions
de la licence CeCILL (v2) telle que diffus�e par le CEA, le CNRS et l'INRIA 
sur le site "http://www.cecill.info".

En contrepartie de l'accessibilit� au code source et des droits de copie,
de modification et de redistribution accord�s par cette licence, il n'est
offert aux utilisateurs qu'une garantie limit�e.  Pour les m�mes raisons,
seule une responsabilit� restreinte p�se sur l'auteur du programme,  le
titulaire des droits patrimoniaux et les conc�dants successifs.

A cet �gard  l'attention de l'utilisateur est attir�e sur les risques
associ�s au chargement,  � l'utilisation,  � la modification et/ou au
d�veloppement et � la reproduction du logiciel par l'utilisateur �tant 
donn� sa sp�cificit� de logiciel libre, qui peut le rendre complexe � 
manipuler et qui le r�serve donc � des d�veloppeurs et des professionnels
avertis poss�dant  des  connaissances  informatiques approfondies.  Les
utilisateurs sont donc invit�s � charger  et  tester  l'ad�quation  du
logiciel � leurs besoins dans des conditions permettant d'assurer la
s�curit� de leurs syst�mes et ou de leurs donn�es et, plus g�n�ralement, 
� l'utiliser et l'exploiter dans les m�mes conditions de s�curit�. 

Le fait que vous puissiez acc�der � cet en-t�te signifie que vous avez 
pris connaissance de la licence CeCILL (v2), et que vous en avez accept� les
termes.

*/

#include "MPTranslator.h"
#include "GlpkSharp.h"

namespace GlpkSharp{
public ref class MPTranslator
{
	private:
		glp_tran* tran;

		char * AllocCharPointer(String^ s){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(s).ToPointer());
			return sp;
		}
		void FreeCharPointer(char * sp){
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		}
	protected:
		!MPTranslator(){
			glp_mpl_free_wksp(tran);
		}

	public:
		MPTranslator()
		{
			tran=glp_mpl_alloc_wksp();
		}
		~MPTranslator(){
			glp_mpl_free_wksp(tran);
		}

		bool ReadModel(String^ fname,bool skipData){
			char * p= AllocCharPointer(fname);
			int i=glp_mpl_read_model(tran,p,skipData);
			FreeCharPointer(p);
			return i==0;
		}
		bool ReadData(String^ fname){
			char * p=AllocCharPointer(fname);
			int i=glp_mpl_read_data(tran,p);
			FreeCharPointer(p);
			return i==0;
		}

		bool GenerateModel(String^ fname){
			char * p=AllocCharPointer(fname);
			int i= glp_mpl_generate(tran,p);
			FreeCharPointer(p);
			return i==0;
		}
		LPProblem^ GenerateProblem(){
			LPProblem^ p=gcnew LPProblem();
			glp_mpl_build_prob(tran,p->prob);
			return p;
		}

		bool PostSolve(LPProblem^ p,SOLUTIONTYPE sol){
			int i=glp_mpl_postsolve(tran,p->prob,int(sol));
			return i==0;
		}
};

}
