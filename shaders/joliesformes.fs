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

float SDF_CutHollowSphere( vec3 p, float r, float h, float t )
{
  // sampling independent computations (only depend on shape)
  float w = sqrt(r*r-h*h);
  
  // sampling dependant computations
  vec2 q = vec2( length(p.xz), p.y );
  return ((h*q.x<w*q.y) ? length(q-vec2(w,h)) : 
                          abs(length(q)-r) ) - t;
}

vec2 opu(vec2 v1, vec2 v2){
	return (v1.x<v2.x) ? v1 : v2;
}

vec2 SDF_Global(vec3 p){
    vec2 res = vec2(SDF_Torus(p, vec2(1.,0.2)),1.0);
    res = opu(res, vec2(SDF_DeathStar(p-vec3(-5.,0,-5.), 3., 2., 4.),2.));
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
	vec3 sunPos=normalize(rotate(vec3(.1,1.,.0),vec3(.2*Time,.6,0)));
	float dotdirsun = clamp(dot(sunPos, dir),0.,1.);
	if(impact.w<0.) return vec3(.4,.1+.6*sunPos.y,.8*sunPos.y)+dotdirsun;
	vec3 normale=grad(impact.xyz);
	vec3 symetrique = reflect(dir,normale);// <=> dir-2.0*dot(dir,normale)*normale;
	vec4 ombre = Get_Impact(impact.xyz+0.02*normale,sunPos);
	float f=ombre.w<0.?1.:.5;
	
	// converting hsv to rgb

	// may be we'll be able to modify them in the future
	float value = 1.;
	float sat = 1.;

	float chroma = value * sat;
	float hue = impact.w / 60;
	float interm = chroma*(1-abs(mod(hue, 2) - 1));

	vec3 couleur = vec3(1.);
	     if (hue<=1.0) couleur = vec3(chroma,interm,0.);
	else if (hue<=2.0) couleur = vec3(interm,chroma,0.);
	else if (hue<=3.0) couleur = vec3(0.,chroma,interm);
	else if (hue<=4.0) couleur = vec3(0.,interm,chroma);
	else if (hue<=5.0) couleur = vec3(interm,0.,chroma);
	else if (hue<=6.0) couleur = vec3(chroma,0.,interm);
	
	return couleur*clamp(dot(sunPos,normale),0.,1.)*f; //*
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
