#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 2) in vec2 texCoord;

out vec2 FragCoord;
out float Time;
out vec3 Ex;
out vec3 Ey;
out vec3 Ez;
out vec3 CamPos;
out float fovValue;
out float FacteurLargeur;
out float OrthoView;
out float CustomToggle;
out float CustomInt;

uniform float iTime;
uniform vec3 iEx;
uniform vec3 iEy;
uniform vec3 iEz;
uniform vec3 iCamPos;
uniform float iFovValue;
uniform float iFacteurLargeur;
uniform int iOrthoView;
uniform int iCustomToggle;
uniform int iCustomInt;

void main()
{
   gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
   FragCoord = texCoord;
   Time=iTime;
   Ex = iEx;
   Ey = iEy;
   Ez = iEz;
   CamPos = iCamPos;
   fovValue = iFovValue;
   FacteurLargeur=iFacteurLargeur;
   OrthoView = iOrthoView;
   CustomToggle = iCustomToggle;
   CustomInt = iCustomInt;
}
