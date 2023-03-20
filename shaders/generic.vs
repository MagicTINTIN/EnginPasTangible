#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 2) in vec2 texCoord;

out vec2 FragCoord;
out float Time;
out vec2 MousePos;
out vec3 CamPos;
out vec3 CamDir;
out float FacteurLargeur;

uniform float iTime;
uniform vec2 iMousePos;
uniform vec3 iCamPos;
uniform vec3 iCamDir;
uniform float iFacteurLargeur;


void main()
{
   gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
   FragCoord = texCoord;
   Time=iTime;
   MousePos = iMousePos;
   CamPos = iCamPos;
   CamDir = iCamDir;
   FacteurLargeur=iFacteurLargeur;
}
