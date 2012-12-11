
#include "glpk.h"
#include "GLPKsharp.h"
#include "GLPKGraphArc.cpp"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::IO;

namespace GlpkSharp{
public ref class GLPKGraph
{
	private:
		glp_graph* graph;

		char * AllocCharPointer(String^ s){
			char * sp=static_cast<char *>(Marshal::StringToHGlobalAnsi(s).ToPointer());
			return sp;
		}
		void FreeCharPointer(char * sp){
			Marshal::FreeHGlobal(safe_cast<IntPtr>(sp));
		}
	protected:
		!GLPKGraph(){
		//	glp_mpl_free_wksp(tran);
			glp_delete_graph(graph);
		}

	public:
		GLPKGraph(int v, int a)
		{
			graph=glp_create_graph(v,a);
		}
		~GLPKGraph(){
//			glp_mpl_free_wksp(tran);
			glp_delete_v_index(graph);
			glp_delete_graph(graph);
		}

		property String^ Name{
			//String^ get(){

			//}
			void set(String^ s){
				char* n=AllocCharPointer(s);
				glp_set_graph_name(graph,n);
				FreeCharPointer(n);
			};
		}

		int AddVertices(int n){
			return glp_add_vertices(graph,n);
		}
		//zero based array.
		void DeleteVertices(array<int>^ vertices){
			int num=vertices->Length;
			int* vert=new int[num+1];
			Marshal::Copy(vertices,0,*new IntPtr(vert+1),num);//api is expecting a one based array.
			glp_del_vertices(graph,num,vert);
		}
		void SetVertexName(int n, String^ name){
			char * sp=AllocCharPointer(name);
			glp_set_vertex_name(graph,n,sp);
			FreeCharPointer(sp);
		}
		int FindVertex(String^ name){
			glp_create_v_index(graph);
			char* p=AllocCharPointer(name);
			int v=glp_find_vertex(graph,p);
			FreeCharPointer(p);
			return v;
		}
		GLPKGraphArc^ AddArc(int start, int stop){
			return gcnew GLPKGraphArc(glp_add_arc(graph,start,stop));
		}
		void DeleteArc(GLPKGraphArc^ arc){
			glp_del_arc(graph,arc->arc);
		}
		void Clear(int vsize, int asize){
			glp_erase_graph(graph,vsize,asize);
		}
		bool LoadFromFile(String^ filename){
			char* s=AllocCharPointer(filename);
			bool ret=false;
			if(glp_read_graph(graph,s)==0){
				ret=true;
			}else{
				ret=false;
			}
			FreeCharPointer(s);
			return ret;
		}
		
		bool WriteToFile(String^ filename){
			char* s=AllocCharPointer(filename);
			bool ret=false;
			if(glp_read_graph(graph,s)==0){
				ret=true;
			}else{
				ret=false;
			}
			FreeCharPointer(s);
			return ret;
		}

		int ComputeWeaklyConnectedComponents(int offset){
			return glp_weak_comp(graph,offset);
		}
		int ComputeStronglyConnectedComponent(int offset){
			return glp_weak_comp(graph,offset);
		}

		int ReadASNProb(int offset_vset,int offset_cost,String^ filename){
			char*p=AllocCharPointer(filename);
			int e= glp_read_asnprob(graph,offset_vset,offset_cost,p);
			FreeCharPointer(p);
			return e;
		}
		int WriteASNProb(int offset_vset,int offset_cost,String^ filename){
			char*p=AllocCharPointer(filename);
			int e= glp_write_asnprob(graph,offset_vset,offset_cost,p);
			FreeCharPointer(p);
			return e;
		}
		int CheckASNProb(int offset_vset){
			return glp_check_asnprob(graph,offset_vset);
		}
		int ASNProb2LP(int form,GLPOnOff withNames,int offset_vset,int offset_cost,[Out] LPProblem^% lp){
			lp=gcnew LPProblem();
			int e=glp_asnprob_lp(lp->prob,form,graph,(int)withNames,offset_vset,offset_cost);
			return e;
		}

};

}