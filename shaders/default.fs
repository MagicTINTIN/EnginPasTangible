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

vec2 opu(vec2 v1, vec2 v2){
	return (v1.x<v2.x) ? v1 : v2;
}

vec2 SDF_Global(vec3 p){
    vec2 res = vec2(SDF_Box_Frame(p,vec3(1.),.1),4.5); //deuxième value:couleur
	                ///couleurs dispo (dans l'ordre 1,2,3,...) : Red,Green,Blue,Yellow,Magenta,Cyan,White
    res=opu(res,vec2(SDF_Sphere(p,.2),1.6));
	return res;
}

vec4 Get_Impact(vec3 origin,vec3 dir){//must have length(dir)==1 
	vec3 pos=origin;
	vec2 dist;
	for(int i=0;i<260;i++){
		dist=SDF_Global(pos);
		pos+=dist.x*dir;
		if(dist.x<=.01) return vec4(pos,dist.y);
		if(dist.x>=200.0) return vec4(pos,-1.);
	}
	return vec4(pos,-1.);
}

vec3 grad(vec3 p){
	vec2 epsilon = vec2(.01,0.);
	return normalize(vec3(SDF_Global(p+epsilon.xyy).x-SDF_Global(p-epsilon.xyy).x,
												SDF_Global(p+epsilon.yxy).x-SDF_Global(p-epsilon.yxy).x,
												SDF_Global(p+epsilon.yyx).x-SDF_Global(p-epsilon.yyx).x));
}

vec3 Get_Color(vec3 origin,vec3 dir){
	vec4 impact = Get_Impact(origin,dir);
	vec3 sunPos=vec3(0.,.7,.7);
	//vec3 sunPos=normalize(rotate(vec3(.1,1.,.0),vec3(.2*Time,.6,0)));
	float dotdirsun = clamp(dot(sunPos, dir),0.,1.);
	if(impact.w<0.) return vec3(.5,.8,.9)+.5*dir.y+.05*clamp(origin.y-10.,-10.,10.);
	vec3 normale=grad(impact.xyz);
	vec3 symetrique = reflect(dir,normale);// <=> dir-2.0*dot(dir,normale)*normale;
	vec4 ombre = Get_Impact(impact.xyz+0.02*normale,sunPos);
	float f=ombre.w<0.?1.:.5;
	
	float id=impact.w;
	vec3 couleur = vec3(1.);
	     if (id<=1.0) couleur = vec3(1.,0.,0.)*id;
	else if (id<=2.0) couleur = vec3(0.,1.,0.)*(id-1.);
	else if (id<=3.0) couleur = vec3(0.,0.,1.)*(id-2.);
	else if (id<=4.0) couleur = vec3(1.,1.,0.)*(id-3.);
	else if (id<=5.0) couleur = vec3(1.,0.,1.)*(id-4.);
	else if (id<=6.0) couleur = vec3(0.,1.,1.)*(id-5.);
	else if (id<=7.0) couleur = vec3(1.,1.,1.)*(id-6.);
	
	return couleur*clamp(dot(sunPos,normale),0.,1.)*f;
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
