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


vec3 repeter(vec3 p, vec3 size, vec3 repet)
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

float SDF_Global(vec3 p){
	//return SDF_Box_Frame(rotate(p,vec3(0.,.2*Time,0.)),vec3(.5,.5,.5),.1);
	//return max(SDF_Sphere(p,.5),-SDF_Box_Frame(rotate(p,vec3(0.,.2*Time,0.)),vec3(1.,1.,1.),.3));//min(SDF_Box_Frame(p,vec3(.5,.5,.5),0.1),SDF_Circle(mod(p+vec3(.5),vec3(1.,1.,1.))-vec3(.5),.15));
	return min(SDF_Box_Frame(repeter(p, vec3(4.,2.,10.), vec3(2., 25., 5.)), vec3(1.,1.,1.),.1),p.y); //infinity(p, vec(0.3, 0.2, 0.5))
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
	if(impact.w<0.) return vec3(.9,.5,.2)+.5*(dir.x*sin(Time)+dir.y*cos(Time))+.05*clamp(origin.y-10.,-10.,10.); //(.5,.8,.9)
	vec3 normale=grad(impact.xyz);
	vec3 sunPos=vec3(sin(Time),cos(Time),0);//vec3(3.*sin(Time*1.5),3.*cos(Time*3.),3.*cos(Time*1.5));
	return vec3(clamp(0.,1.,dot(sunPos,normale)));//normale; ---impact.xyz
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
	
	
  //float c=Mandel(FragCoord*1.5);
  FragColor=vec4(Get_Color(posCam,dir),1.);//c,c,c,1.);
}
