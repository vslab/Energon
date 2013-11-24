#include "glpk.h"
#include "GLPKGraphArc.h"

public ref class GLPKGraphArc
{
internal:
	glp_arc* arc;
	GLPKGraphArc(glp_arc* a){
		arc=a;
	}

};
