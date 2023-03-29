#version 330 core

in vec2 FragCoord;
in float Time;
in vec3 Ex;
in vec3 Ey;
in vec3 Ez;
in vec3 CamPos;
in float fovValue;
in float FacteurLargeur;

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

vec3 infinity(vec3 pos,vec3 box){
	return mod(pos+.5*box,box)-.5*box;
}


vec3 repeter(vec3 p, float size, vec3 repet)
{
    return p-size*clamp(round(p/size),-repet,repet);
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

vec4 SDF_Global(vec3 p){
	float box = SDF_Box(p,vec3(2.,.5,2.));
	vec3 c1= vec3(1.,.8,0.);
	float mini = box;
	vec3 cFinale=c1;
	
	float sphere =SDF_Sphere(p+vec3(0.,2.5*cos(Time*.4),0.),1.);
	vec3 c2=vec3(1.,0.,0.);
	mini=smin(box,sphere,1.);
	cFinale=mix(c2,c1,abs(sphere-mini));
	
	return vec4(mini,cFinale);
}

vec4 Get_Impact(vec3 origin,vec3 dir){//must have length(dir)==1 
	vec3 pos=origin;
	vec4 dist;
	for(int i=0;i<60;i++){
		dist=SDF_Global(pos);
		pos+=dist.x*dir;
		if(dist.x<=.01) return vec4(pos,1.);
		if(dist.x>=20.0) return vec4(pos,-1.);
	}
	return vec4(pos,1.);
}

vec3 grad(vec3 p){
	vec2 epsilon = vec2(.01,0.);
	return normalize(vec3(SDF_Global(p+epsilon.xyy).x-SDF_Global(p-epsilon.xyy).x,
												SDF_Global(p+epsilon.yxy).x-SDF_Global(p-epsilon.yxy).x,
												SDF_Global(p+epsilon.yyx).x-SDF_Global(p-epsilon.yyx).x));
}

vec3 Get_Color(vec3 origin,vec3 dir){
	vec4 impact = Get_Impact(origin,dir);
	if(impact.w<0.) return vec3(.5,.8,.9)+.5*dir.y+.05*clamp(origin.y-10.,-10.,10.);//(impact.y+1.)*.05*vec3(.5,.7,1.);
	vec3 normale=grad(impact.xyz);
	vec3 symetrique= dir-2.0*dot(dir,normale)*normale;
	//vec4 reflexion = Get_Impact(impact.xyz+0.02*normale,normalize(symetrique));
	//float g=reflexion.w<0.?1.5:1.;
	vec3 sunPos=vec3(0.,.7,.7);//vec3(3.,3.5,.5);//vec3(3.*sin(Time*1.5),3.*cos(Time*3.),3.*cos(Time*1.5));
	float specular=clamp(dot(symetrique,sunPos)*.5,0.,1.);
	vec4 ombre = Get_Impact(impact.xyz+0.02*normale,sunPos);
	float f=ombre.w<0.?1.:.5;
	vec3 couleur=SDF_Global(impact.xyz).yzw;
	
	return couleur*clamp(dot(sunPos,normale),0.,1.)*f+vec3(specular);
}

float Mandel(vec2 co){
	vec2 coo = co.xy;
	float limf=100.0;
	float cf=0.0;
	int c;
	for(c=0;c<300;c++){
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
	
	
  FragColor =vec4(vec3(Mandel(vec2(-.75,-.1)+FragCoord*2.0/pow(Time,Time))),1.);//vec2(.3885959955,.133913)
  //FragColor=vec4(Get_Color(posCam,dir),1.);//c,c,c,1.);
}
