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
in float CustomInt;

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
	return p + .02*sin(2*p.x+5*Time)*sin(2*p.y+3*Time)*sin(2*p.z+4*Time);
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


vec2 opu(vec2 v1, vec2 v2){
	return (v1.x<v2.x) ? v1 : v2;
}

//((CustomInt == 1) ? 50 : 260)

vec2 SDF_Global(vec3 p){
    float colorframe = 420;
    if (CustomInt >= 8) colorframe = mod(100*Time,360);

    vec2 res = vec2(SDF_Box_Frame(p,vec3(1.),.1),colorframe);
    if (CustomInt >=10) res = vec2(SDF_Box_Frame(rotate(p,vec3(.4*Time,.5*Time,.6*Time)),vec3(1.),.1),colorframe);
    //vec2 res = vec2(SDF_Torus(Bender(Displacer(p-vec3(0.,.1,0.))), vec2(1.,.2)),1.0);
    if (CustomInt <= 10)
        res = opu(res, vec2(SDF_Box(p+vec3(0,2,0),vec3(3.,.3,3.)),230.+480.));
    else
        res = opu(res, vec2(SDF_Box(Displacer(p)+vec3(0,2,0),vec3(3.,.3,3.)),230.+480.));
    res=opu(res,vec2(SDF_Sphere(repeat(p-vec3(0,10.,0), vec3(4.,2.,10.), vec3(0, 5., 0.)),.3),420));
	
	return res;
}

vec4 Get_Impact(vec3 origin,vec3 dir){//must have length(dir)==1 
	vec3 pos=origin;
	vec2 dist;
	for(int i=0;i<560;i++){
		dist=SDF_Global(pos);
		pos+=dist.x*dir;
		if(dist.x<=.001) return vec4(pos,dist.y);
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
	vec3 sunPos=vec3(1.);
    if (CustomInt >= 2)
        sunPos=normalize(rotate(vec3(.1,1.,.0),vec3(1.3*cos(.4*Time),.6,0)));
	float dotdirsun = clamp(dot(sunPos, dir),0.,1.);

    vec3 skycolor = vec3(.3+0.4*sunPos.y,.1+.7*sunPos.y,.8*sunPos.y);
    // changement du ciel
	if(impact.w<0.) {
        if (CustomInt >= 9)
            return skycolor+dotdirsun;
        if (CustomInt >= 6)
            return vec3(.4,.1+.6*sunPos.y,.8*sunPos.y)+dotdirsun;
        else if (CustomInt >= 3)
            return vec3(.5,.8,.9)+.5*dir.y+.05*clamp(origin.y-10.,-10.,10.);
        else
            return vec3(.4,.6,.8);
    }
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
	else if (hue<=7.0) couleur = vec3(interm,interm,interm);
	
    float g=1.;

    if (hue > 8.) {
        hue-=8.;
        if (hue<=1.0) couleur = vec3(chroma,interm,0.);
        else if (hue<=2.0) couleur = vec3(interm,chroma,0.);
        else if (hue<=3.0) couleur = vec3(0.,chroma,interm);
        else if (hue<=4.0) couleur = vec3(0.,interm,chroma);
        else if (hue<=5.0) couleur = vec3(interm,0.,chroma);
        else if (hue<=6.0) couleur = vec3(chroma,0.,interm);
        else if (hue<=7.0) couleur = vec3(interm,interm,interm);
        
        vec4 reflexion = Get_Impact(impact.xyz+0.02*normale,normalize(symetrique));
	    g=reflexion.w<0.?1.5:1.;
    }
    if (CustomInt <= 0) return normale;
    else if (CustomInt <= 3) return vec3(clamp(dot(sunPos,normale),0.,1.));
    else if (CustomInt <= 4) return vec3(clamp(dot(sunPos,normale),0.,1.))*f;// ombres
    else if (CustomInt <= 6) return vec3(clamp(dot(sunPos,normale),0.,1.))*f*g;// reflections
    else if (CustomInt <= 8) return couleur*clamp(dot(sunPos,normale),0.,1.)*f*g; // couleurs
    else if (CustomInt >= 9) return skycolor*couleur*clamp(dot(sunPos,normale),0.,1.)*f*g; // couleurs + ambiance
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
