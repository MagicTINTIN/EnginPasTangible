#version 330 core

in vec2 FragCoord;
in float Time;
in vec3 Ex;
in vec3 Ey;
in vec3 Ez;
in vec3 CamPos;
in float fovValue;
in float FacteurLargeur;
in float OrthoView;

out vec4 FragColor;
//uniform sampler2D generalTexture;

float smin(float a, float b, float k){
	float h=clamp(.5+.5*(b-a)/k,0.,1.0);
	return mix(b,a,h) - k*h*(1.-h);
}

vec3 rotate(vec3 pos, vec3 angles){
	float ca=cos(angles.x);
	float sa=sin(angles.x);
	float cb=cos(angles.y);
	float sb=sin(angles.y);
	float cc=cos(angles.z);
	float sc=sin(angles.z);
	return vec3(cb*cc*pos.x+(sa*sb*cc-ca*sc)*pos.y+(ca*sb*cc+sa*sc)*pos.z,
							cb*sc*pos.x+(sa*sb*sc+ca*cc)*pos.y+(ca*sb*sc-sa*cc)*pos.z,
							-sb*pos.x+sa*cb*pos.y+ca*cb*pos.z);
}

vec3 symetry(vec3 p, vec3 norm,float offset){
	float d=dot(p,norm) + offset;
	return (d>0)?p:p-2.*d*norm;
}

vec3 infinity(vec3 pos,vec3 box){
	return mod(pos+.5*box,box)-.5*box;
}

vec3 repeat(vec3 p, vec3 size, vec3 repet)
{
    return p-size*clamp(vec3(round(p.x/size.x),round(p.y/size.y),round(p.z/size.z)),-repet,repet);
}

float SDF_Box_Frame( vec3 p, vec3 b, float e )
{
       p = abs(p  )-b;
  vec3 q = abs(p+e)-e;
  return min(min(
      length(max(vec3(p.x,q.y,q.z),0.0))+min(max(p.x,max(q.y,q.z)),0.0),
      length(max(vec3(q.x,p.y,q.z),0.0))+min(max(q.x,max(p.y,q.z)),0.0)),
      length(max(vec3(q.x,q.y,p.z),0.0))+min(max(q.x,max(q.y,p.z)),0.0));
}

float Inflate( float b, float r )
{
  return b - r;
}

float SDF_Sphere(vec3 p,float r){
	return length(p)-r;
}

float SDF_Box(vec3 p, vec3 t){
	vec3 q=abs(p)-t;
	return length(max(q,0.0))+ min(max(q.x,max(q.y,q.z)),0.0);
}

float dot2(vec3 v){ return dot(v,v);}

float SDF_Triangle( vec3 p, vec3 a, vec3 b, vec3 c )
{
  vec3 ba = b - a; vec3 pa = p - a;
  vec3 cb = c - b; vec3 pb = p - b;
  vec3 ac = a - c; vec3 pc = p - c;
  vec3 nor = cross( ba, ac );

  return sqrt(
    (sign(dot(cross(ba,nor),pa)) +
     sign(dot(cross(cb,nor),pb)) +
     sign(dot(cross(ac,nor),pc))<2.0)
     ?
     min( min(
     dot2(ba*clamp(dot(ba,pa)/dot2(ba),0.0,1.0)-pa),
     dot2(cb*clamp(dot(cb,pb)/dot2(cb),0.0,1.0)-pb) ),
     dot2(ac*clamp(dot(ac,pc)/dot2(ac),0.0,1.0)-pc) )
     :
     dot(nor,pa)*dot(nor,pa)/dot2(nor) );
}

float SDF_tetra(vec3 p,float h){
	//float hh = h*.5;
	//float c1=h/sqrt(3);
	//vec3 a = vec3(0.,hh,0.);
	//vec3 b = vec3(0.,-hh,hh);
	//vec3 c = vec3(c1,-hh,-hh);
	//vec3 d = vec3(-c1,-hh,-hh);
	float f1=1./sqrt(3.);
	float f2=1./sqrt(6.);
	vec3 a = vec3(0.);
	vec3 o = vec3( 1.,-f1  ,-f2 );
	vec3 b = vec3(-1.,-f1  ,-f2 )-o;
	vec3 c = vec3(0. ,2.*f1,-f2 )-o;
	vec3 d = vec3(0. ,0.  ,3.*f2)-o;
	return min(SDF_Triangle(p,a,b,c),min(SDF_Triangle(p,a,b,d),min(SDF_Triangle(p,a,c,d),SDF_Triangle(p,b,c,d))));
}

float SDF_tetra2(vec3 ps,float s){
	vec3 p=ps+vec3(s);
	return (max(
	    abs(p.x+p.y)-p.z,
	    abs(p.x-p.y)+p.z
	)-s)/sqrt(3.);
}

float SDF_Sierp(vec3 p,float s,int n){
	//float ss=s*.5;
	//float c=s/sqrt(3);
	//float c1=s/sqrt(3);
	float d=pow(2.,n);
	vec3 ps=p/s;
	
	for (int i=0;i<n;i++){
		ps=symetry(ps,normalize(vec3(1,-1./sqrt(3.),-4./sqrt(6.))),d);
		ps=symetry(ps,         (vec3(1.,0.,0.)),d);
		ps=symetry(ps,normalize(vec3(1.,-3./sqrt(3.),0.)),d);
		d*=.5;
		
	}
	
	return SDF_tetra(ps,s);//min(SDF_Sphere(ps,.2),SDF_tetra(ps,s));
}

float SDF_Sierp2(vec3 p,float s,int n){
	//float ss=s*.5;
	//float c=s/sqrt(3);
	//float c1=s/sqrt(3);
	float d=sqrt(2)*pow(2.,n)*s;
	vec3 ps=p;// /s;
	
	for (int i=0;i<n;i++){
		ps=symetry(ps,normalize(vec3(0.,1.,-1.)),0.);
		ps=symetry(ps,normalize(vec3(-1.,1.,0.)),0.);
		ps=symetry(ps,normalize(vec3(1.,0.,1.)),1.*d);
		d*=.5	;
	}
	
	return SDF_tetra2(ps,s);//min(SDF_Sphere(ps,.2),SDF_tetra(ps,s));
}

float SDF_Cylinder( vec3 p, float h, float r )
{
  vec2 d = abs(vec2(length(p.xz),p.y)) - vec2(r,h);
  return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

float SDF_Prism( vec3 p, vec2 h )
{
  vec3 q = abs(p);
  return max(q.z-h.y,max(q.x*0.866025+p.y*0.5,-p.y)-h.x*0.5);
}

mat3 opu(mat3 v1, mat3 v2){
	return (v1[0].x<v2[0].x) ? v1 : v2;
}

mat3 SDF_Global(vec3 p){
	// mat3 (sdf, 0 , 0,
	//		 hue (0->360),saturation(0->1),value(0->1),
	//		 specular(no shadow on it : 0->1), reflection (0->1), 0)

	float insapos = 0;
	float xoffset = 0; //1.75


	// I
	float ioffset = insapos - 6;
	mat3 res = mat3(
		SDF_Box(p-vec3(xoffset ,0,ioffset),vec3(1.75,1.,.5))
		,0,0,
		0,1.,1.,
		0,0,0);

	// S
	float soffest = 1 + insapos;
    res = opu(res, mat3(
		max(
			SDF_Cylinder(p-vec3(xoffset-.75, 0,soffest),2,1),
			max(
					SDF_Box(p-vec3(xoffset-.75,0,1+soffest),vec3(1.)),
					-SDF_Cylinder(p-vec3(xoffset-.75,0,soffest),2,.5)
				)
			)
		,0,0,
		0,1.,1.,
		0,0,0));
	res = opu(res, mat3(
		max(
			SDF_Cylinder(p-vec3(xoffset+.75, 0,soffest),2,1),
			max(
					SDF_Box(p-vec3(xoffset+.75,0,-1+soffest),vec3(1.)),
					-SDF_Cylinder(p-vec3(xoffset+.75,0,soffest),2,.5)
				)
			)
		,0,0,
		0,1.,1.,
		0,0,0));
	res = opu(res, mat3(
		min(
			SDF_Box(p-vec3(xoffset+1.5,0,0.5+soffest),vec3(.25,1,.5)),
			SDF_Box(p-vec3(xoffset-1.5,0,-.5+soffest),vec3(.25,1,.5))
		)
		,0,0,
		0,1.,1.,
		0,0,0));


	// A
	float aoffset = 4.5+insapos;
	res = opu(res, mat3(
		max (
			max (
				max (
					max (
						max (
							max(
								SDF_Box(p-vec3(xoffset,0,aoffset),vec3(1.7,1,1.7)),
								-SDF_Box(p-vec3(xoffset-1.5,0,aoffset),vec3(.5,2,1)) // creux bas A
							),

							-SDF_Prism(rotate(p-vec3(xoffset,0,aoffset),vec3(0.5*3.14156,0.*3.14156,0.5*3.14156)),vec2(1,2)) // trou A
						),
						-SDF_Prism(rotate(p-vec3(xoffset+1.5,0,aoffset+2),vec3(0.5*3.14156,1.*3.14156,0.5*3.14156)),vec2(3,2)) // /gauche
					),
					-SDF_Prism(rotate(p-vec3(xoffset+1.5,0,aoffset-2),vec3(0.5*3.14156,1.*3.14156,0.5*3.14156)),vec2(3,2)) // \ droit
				),
				-SDF_Prism(rotate(p-vec3(xoffset-1.5,0,aoffset-1),vec3(0.5*3.14156,0.*3.14156,0.5*3.14156)),vec2(.5,2)) 
			),
			-SDF_Prism(rotate(p-vec3(xoffset-1.5,0,aoffset+1),vec3(0.5*3.14156,0.*3.14156,0.5*3.14156)),vec2(.5,2))
		)
		,0,0,
		0,1.,1.,
		0,0,0));



	// N
	float noffset = -2.5+insapos;
	res = opu(res, mat3(
		
		max (
			max (
				-max(
					
					SDF_Prism(rotate(p-vec3(xoffset-1.1,0,noffset-1.),vec3(0.5*3.14156,0.*3.14156,.5*3.14156)),vec2(2.1,2)), // /gauche
					SDF_Box(p-vec3(xoffset-1,0,noffset),vec3(2,2,1.))
				),
				SDF_Box(p-vec3(xoffset,0,noffset),vec3(1.7,1,1.7))
				
			),
			-max(
					
					SDF_Prism(rotate(p-vec3(xoffset+1.1,0,noffset+1.),vec3(0.5*3.14156,1.*3.14156,.5*3.14156)),vec2(2.1,2)), // /gauche
					SDF_Box(p-vec3(xoffset+1.1,0,noffset),vec3(2,2,1.))
				)
		)
		,0,0,
		0,1.,1.,
		0,0,0));

		res = opu(res, mat3(
		SDF_Box(p-vec3(xoffset ,-1,insapos-.25),vec3(3,.1,7.25))
		,0,0,
		0,0.,1.,
		0,0,0));

	return res;
}

mat3 Get_Impact(vec3 origin,vec3 dir){//must have length(dir)==1 
	vec3 pos=origin;
	mat3 dist;
	for(int i=0;i<60;i++){
		dist=SDF_Global(pos);
		pos+=dist[0].x*dir;
		if(dist[0].x<=.01) return mat3(pos,dist[1],dist[2]);
		if(dist[0].x>=200.0) return mat3(pos,vec3(-1,0,0),vec3(0,0,0));
	}
	return mat3(pos,vec3(-1,0,0),vec3(0,0,0));
}

vec3 grad(vec3 p){
	vec2 epsilon = vec2(.01,0.);
	return normalize(vec3(SDF_Global(p+epsilon.xyy)[0].x-SDF_Global(p-epsilon.xyy)[0].x,
		SDF_Global(p+epsilon.yxy)[0].x-SDF_Global(p-epsilon.yxy)[0].x,
		SDF_Global(p+epsilon.yyx)[0].x-SDF_Global(p-epsilon.yyx)[0].x));
}

vec3 HSV(vec3 c){
	// converting hsv to rgb

	// may be we'll be able to modify them in the future
	float value = c.z;
	float sat = c.y;

	float chroma = value * sat;
	float hue = mod(c.x / 60,8.0);
	float interm = chroma*(1-abs(mod(hue, 2) - 1));

	vec3 couleur = vec3(1.);
	     if (hue<=1.0) couleur = vec3(chroma,interm,0.);
	else if (hue<=2.0) couleur = vec3(interm,chroma,0.);
	else if (hue<=3.0) couleur = vec3(0.,chroma,interm);
	else if (hue<=4.0) couleur = vec3(0.,interm,chroma);
	else if (hue<=5.0) couleur = vec3(interm,0.,chroma);
	else if (hue<=6.0) couleur = vec3(chroma,0.,interm);
	else if (hue<=7.0) couleur = vec3(interm,interm,interm);
	return couleur + value - chroma;
}

float speclr(float fact, float val) {
	return fact + (1 - fact) * val;
}

vec3 speclr(float fact, vec3 val) {
	return fact + (1 - fact) * val;
}

vec3 Get_Color(vec3 origin,vec3 dir){
	vec3 sunPos=normalize(rotate(vec3(.2,1.,.0),vec3(.4*cos(.8*Time),.6,0)));
	mat3 impact = Get_Impact(origin,dir);
	vec3 impactcolor = impact[1];
	
	float dotdirsun = clamp(dot(sunPos, dir),0.,1.);

  //vec3 skycolor = vec3(.5,.8,.9)+.5*dir.y+.05*clamp(origin.y-10.,-10.,10.); //
  vec3 skycolor = vec3(.3+0.4*sunPos.y,.1+.8*sunPos.y,1.*sunPos.y);

  // changement du ciel
	if(impactcolor.x<0.) {
		return skycolor+dotdirsun;
  	}
	vec3 normale=grad(impact[0]); //.xyz
	vec3 symetrique = reflect(dir,normale);// <=> dir-2.0*dot(dir,normale)*normale;
	mat3 ombre = Get_Impact(impact[0]+0.02*normale,sunPos);
	float f=ombre[1].x<0.?1.:.5;
	vec3 couleur = HSV(impactcolor);
  	vec3 g=vec3(0.);
  	if (impact[2].y > .0){
	
  		mat3 reflexion = Get_Impact(impact[0]+0.02*normale,normalize(symetrique));
		g=reflexion[1].x<0.?vec3(0.):HSV(reflexion[1])*impact[2].y*skycolor;
		g*=clamp(speclr(reflexion[2].x,dot(sunPos,grad(reflexion[0]))),0.,1.);
	
	}
    
    return couleur*speclr(impact[2].x,skycolor*clamp(dot(sunPos,normale),0.,1.)*f)+g;
	
}

float Mandel(vec2 co){
	vec2 coo = co.xy;
	float limf=100.0;
	float cf=0.0;
	int c;
	for(c=0;c<30;c++){
		coo=vec2(pow(coo.x,2.0)-pow(coo.y,2.0)+co.x,2.*coo.x*coo.y+co.y);
		if(length(coo)>=2.0){
			return cf/limf;
		}
		cf+=1.0;
	}
	return 0.0;
}

void main(){
	vec3 lookingAt = vec3(0.);
	vec3 posCam    = CamPos;//vec3(-3.*sin(Time*.15),.6*cos(Time*.15),3.*cos(Time*.15));
	//vec3 posCam    = vec3(-3.*sin(MousePos.x/400)*(1-abs(atan(MousePos.y/500))),2*atan(MousePos.y/300),3.*cos(MousePos.x/400)*(1-abs(atan(MousePos.y/500))));
	//float pan=-MousePos.x/180.;
	//float tilt=-MousePos.y/180.;
	//tilt=max(min(tilt,3.14*.45),-3.14*.45);

	//vec3 ez = vec3(cos(tilt)*sin(pan),sin(tilt),cos(tilt)*cos(pan));//normalize(lookingAt-posCam);////base orthonormée
	//vec3 ex = normalize(cross(ez,vec3(0.,1.,0.)));
	//vec3 ey = cross(ex,ez);
	
	vec3 dir = normalize(FragCoord.x * normalize(Ex) + FragCoord.y * normalize(Ey) + fovValue * Ez);
	
	if(OrthoView == 1.){
  //float c=Mandel(FragCoord*1.5);
  	FragColor=vec4(Get_Color(posCam+fovValue*FragCoord.x*normalize(Ex)+fovValue*FragCoord.y*normalize(Ey),normalize(Ez)),1.);//c,c,c,1.);
  }else{
  	FragColor=vec4(Get_Color(posCam,dir),1.);//c,c,c,1.);
  }
}
