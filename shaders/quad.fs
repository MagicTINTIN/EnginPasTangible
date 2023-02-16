#version 330 core

in vec2 FragCoord;
in float Time;
in vec2 MousePos;
in vec3 CamPos;
in vec3 CamDir;

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

float SDF_Box_Frame( vec3 p, vec3 b, float e )
{
       p = abs(p  )-b;
  vec3 q = abs(p+e)-e;
  return min(min(
      length(max(vec3(p.x,q.y,q.z),0.0))+min(max(p.x,max(q.y,q.z)),0.0),
      length(max(vec3(q.x,p.y,q.z),0.0))+min(max(q.x,max(p.y,q.z)),0.0)),
      length(max(vec3(q.x,q.y,p.z),0.0))+min(max(q.x,max(q.y,p.z)),0.0));
}

float SDF_Circle(vec3 p,float r){
	return length(p)-r;
}

float SDF_Global(vec3 p){
	return SDF_Box_Frame(rotate(p,vec3(1.*Time,2.*Time,3.*Time)),vec3(1.5,1.5,1.5),.5);//min(SDF_Box_Frame(p,vec3(.5,.5,.5),0.1),SDF_Circle(mod(p+vec3(.5),vec3(1.,1.,1.))-vec3(.5),.15));
}

vec4 Get_Impact(vec3 origin,vec3 dir){//must have length(dir)==1 
	vec3 pos=origin;
	float dist;
	for(int i=0;i<30;i++){
		dist=SDF_Global(pos);
		pos+=dist*dir;
		if(dist<=.01) return vec4(pos,1.);
		if(dist>=20.0) return vec4(pos,-1.);
	}
	return vec4(pos,-1.);
}

vec3 grad(vec3 p){
	vec2 epsilon = vec2(.01,0.);
	return normalize(vec3(SDF_Global(p+epsilon.xyy)-SDF_Global(p-epsilon.xyy),
												SDF_Global(p+epsilon.yxy)-SDF_Global(p-epsilon.yxy),
												SDF_Global(p+epsilon.yyx)-SDF_Global(p-epsilon.yyx)));
}

vec3 Get_Color(vec3 origin,vec3 dir){
	vec4 impact = Get_Impact(origin,dir);
	if(impact.w<0.) return vec3(.5,.7,1.);
	vec3 normale=grad(impact.xyz);
	vec3 sunPos=vec3(3.,3.5,.5);//vec3(3.*sin(Time*1.5),3.*cos(Time*3.),3.*cos(Time*1.5));
	return vec3(clamp(0.,1.,dot(sunPos-impact.xyz,normale)));//normale;
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
	vec3 posCam    = vec3(2.5,2.5,2.5);//vec3(-3.*sin(Time*.15),.6*cos(Time*.15),3.*cos(Time*.15));
	
	vec3 ez = normalize(lookingAt-posCam);////base orthonormée
	vec3 ex = normalize(cross(ez,vec3(0.,1.,0.)));
	vec3 ey = cross(ex,ez);
	
	vec3 dir = normalize(FragCoord.x * ex + FragCoord.y*ey + 1.*ez);
	
	
  //float c=Mandel(FragCoord*1.5);
  FragColor=vec4(Get_Color(posCam,dir),1.);//c,c,c,1.);
}
