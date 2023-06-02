#version 330 core
in vec2 FragCoord; //FragCoord = position pixel
in float Time; //parametre du shader

out vec4 FragColor; //FragColor = couleur sortie par le programme (r,g,b,a)

float SDF_Box_Frame( vec3 p, vec3 b, float e ){
    p = abs(p  )-b;
    vec3 q = abs(p+e)-e;
    return min(min(
        length(max(vec3(p.x,q.y,q.z),0.0))+min(max(p.x,max(q.y,q.z)),0.0),
        length(max(vec3(q.x,p.y,q.z),0.0))+min(max(q.x,max(p.y,q.z)),0.0)),
        length(max(vec3(q.x,q.y,p.z),0.0))+min(max(q.x,max(q.y,p.z)),0.0));
}

float SDF_Sphere(vec3 p,float r){
    return length(p)-r;
}

float SDF_Scene(vec3 p){
    return min(
        SDF_Box_Frame(p,vec3(0.5,0.5,0.5),0.1),
        SDF_Sphere(p,0.25));
}
    
vec4 Get_Impact(vec3 origin,vec3 direction){ //must have length(dir)==1 
    vec3 pos=origin;
    float dist;
    int maxStep=60;
    for(int i=0;i<maxStep;i++){
        dist=SDF_Scene(pos);
        pos+=dist*direction;
        if(dist<=0.01) return vec4(pos,1.0);
        if(dist>=20.0) return vec4(pos,-1.0); //-1.0 => rayon a l'infini
    }
    return vec4(pos,-1.0);
}

vec3 grad(vec3 p){
    vec3 dx = vec3(0.01,0.0,0.0);
    vec3 dy = vec3(0.0,0.01,0.0);
    vec3 dz = vec3(0.0,0.0,0.01);
    return normalize(vec3(SDF_Scene(p+dx)-SDF_Scene(p-dx),
                          SDF_Scene(p+dy)-SDF_Scene(p-dy),
                          SDF_Scene(p+dz)-SDF_Scene(p-dz)));
}

vec3 Get_Color(vec3 origin,vec3 direction){
    vec4 impact = Get_Impact(origin,direction);
    if(impact.w<0.) return vec3(.5,.7,1.); //couleur du ciel
    vec3 normale = grad(impact.xyz);
    vec3 SunDirection = normalize(vec3(1.0,2.0,3.0));
    vec3 SunColor = vec3(1.0,0.9,0.5);
    vec3 CouleurObjet = vec3(1.0,1.0,1.0);
    return min(CouleurObjet,
               SunColor*max(dot(normale,SunDirection),0.0));
}

void main(){
    vec3 lookingAt = vec3(0.0,0.0,0.0);
    vec3 posCamera = vec3(2.0*sin(Time*0.5),1.0,2.0*cos(Time*0.5));//rotating camera
    
    vec3 ez = normalize(lookingAt - posCamera); //base orthonormee
    vec3 ex = normalize(cross(ez,vec3(0.,1.,0.)));
    vec3 ey = cross(ex,ez);
    
    vec3 direction = normalize(FragCoord.x * ex + FragCoord.y*ey + 1.0*ez);
    FragColor=vec4(Get_Color(posCamera,direction),1.0);
}