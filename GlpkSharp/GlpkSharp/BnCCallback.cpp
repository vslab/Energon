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

#include "BnCCallback.h"

namespace GlpkSharp{

		void BnCHook(glp_tree *tree, void *info){
			BranchAndCutDelegate^ d=
				(BranchAndCutDelegate^)Marshal::GetDelegateForFunctionPointer((IntPtr)(info),BranchAndCutDelegate::typeid);
			d(gcnew BranchTree(tree),nullptr);
		}

		void BnCCallback::SetCallback(LPProblem^ p, BranchAndCutDelegate^ d)
		{
			p->glpiocp->cb_func=BnCHook;
			p->glpiocp->cb_info=Marshal::GetFunctionPointerForDelegate(d).ToPointer();
		}

}
