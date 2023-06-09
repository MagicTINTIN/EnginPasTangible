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
in float CustomToggle;

out vec4 FragColor;
//uniform sampler2D generalTexture;

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

float SDF_Torus( vec3 p, vec2 t )
{
  vec2 q = vec2(length(p.xz)-t.x,p.y);
  return length(q)-t.y;
}

float SDF_DeathStar( in vec3 p2, in float ra, float rb, in float d )
{
  // sampling independent computations (only depend on shape)
  float a = (ra*ra - rb*rb + d*d)/(2.0*d);
  float b = sqrt(max(ra*ra-a*a,0.0));
	
  // sampling dependant computations
  vec2 p = vec2( p2.x, length(p2.yz) );
  if( p.x*b-p.y*a > d*max(b-p.y,0.0) )
    return length(p-vec2(a,b));
  else
    return max( (length(p          )-ra),
               -(length(p-vec2(d,0))-rb));
}


vec3 Twister( in vec3 p )
{
    float k = .2*cos(Time); // or some other amount
    float c = cos(k*p.y);
    float s = sin(k*p.y);
    //mat2  m = mat2(c,-s,s,c);
    vec3  q = vec3(c*p.x-s*p.z,s*p.x+c*p.z,p.y);
    return q;
}

vec3 Bender( in vec3 p )
{
    //float k = 2.*cos(Time); // or some other amount
    //float c = cos(k*p.x);
    //float s = sin(k*p.x);
    //mat2  m = mat2(c,-s,s,c);
    //vec3  q = vec3(m*p.xy,p.z);
	vec3 q = p;
	q.y += .1*sin(2*p.x+5*Time);
    return q;
}


vec3 Mer( in vec3 p )
{
    //float k = 2.*cos(Time); // or some other amount
    //float c = cos(k*p.x);
    //float s = sin(k*p.x);
    //mat2  m = mat2(c,-s,s,c);
    //vec3  q = vec3(m*p.xy,p.z);
	vec3 q = p;
	q.y += .1*sin(2*p.x+5*Time)*sin(1.5*p.z	+4.5*Time);
    return q + .02*sin(3*p.x+5*Time)*sin(4*p.y+3*Time)*sin(5*p.z+4*Time);;
}

vec3 Displacer (in vec3 p)
{
	return p + .02*sin(20*p.x+5*Time)*sin(20*p.y+3*Time)*sin(20*p.z+4*Time);
}

float SDF_CutHollowSphere( vec3 p, float r, float h, float t )
{
  // sampling independent computations (only depend on shape)
  float w = sqrt(r*r-h*h);
  
  // sampling dependant computations
  vec2 q = vec2( length(p.xz), p.y );
  return ((h*q.x<w*q.y) ? length(q-vec2(w,h)) : 
                          abs(length(q)-r) ) - t;
}

float SDF_Prisme( vec3 p, vec2 h )
{
  vec3 q = abs(p);
  return max(q.z-h.y,max(q.x*0.866025+p.y*0.5,-p.y)-h.x*0.5);
}


mat3 opu(mat3 v1, mat3 v2){
	return (v1[0].x<v2[0].x) ? v1 : v2;
}


mat3 SDF_Global(vec3 p){
	//bouée
    mat3 res = mat3(SDF_Torus(Bender(Displacer(p-vec3(0.,.1,0.))), vec2(1.,.2)),0,0,
	0,1,1,
	0,0,0);
    // la fucking étoile de la mort
	res = opu(
		res, mat3(SDF_DeathStar(p-vec3(-5.,0,-5.), 3., 3., 3.),0.,0., 
	60.,1.,1,
	.5,0.,0.)
	);
	//mer
    res = opu(res, mat3(SDF_Box(Mer(p+vec3(0,1,0)),vec3(60.,1,60.)),0,0,
	230,.5,.5,
	0,.5,0));
	//requin
	float rRequing = 12.;
	float vRequing = 0.3;
	res = opu(res, mat3(
		SDF_Prisme(
			rotate(
				p-vec3(rRequing*cos(vRequing*Time),.5,rRequing*sin(vRequing*Time)),
				vec3(0.,vRequing*Time+.5*3.1415+.012*rRequing*sin(20*Time*vRequing),0.))
		,vec2(1,0.1)), //
		0,0,
		340,.05,.2,
		0,.1,0
		));
	return res;
}

mat3 Get_Impact(vec3 origin,vec3 dir){//must have length(dir)==1 
	vec3 pos=origin;
	mat3 dist;
	for(int i=0;i<260;i++){
		dist=SDF_Global(pos);
		pos+=dist[0].x*dir;
		if(dist[0].x<=.01) return mat3(pos,dist[1],dist[2]);;
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
	
	vec3 sunPos=normalize(rotate(vec3(.2,1.,.0),vec3(.4*cos(.2*Time),.6,0)));
	if (CustomToggle == 1) sunPos=normalize(vec3(.5,1.,.5));
	mat3 impact = Get_Impact(origin,dir);
	vec3 impactcolor = impact[1];
	
	float dotdirsun = clamp(dot(sunPos, dir),0.,1.);

  	vec3 skycolor = vec3(.3+0.4*sunPos.y,.1+.7*sunPos.y,.8*sunPos.y);

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
