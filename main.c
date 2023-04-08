#define APPNAMEVERSION "EnginPasTangible (alpha 0.4.2)"
#include "./Libraries/glad/glad.h"
#include <stdio.h>
#include <math.h>
#include <stdbool.h>
#include "./Libraries/GLFW/glfw3.h"
#define STB_IMAGE_IMPLEMENTATION
#include "./Libraries/stb/stb_image.h"
#include "headers/shader.h"
/* SCENE LIST 
shaders/default.fs
shaders/immeublesparisiens.fs
shaders/couleurs.fs
shaders/sierp.fs
*/
#define SCENE "shaders/sierp.fs"
//#define SCENE "shaders/immeublesparisiens.fs"
#define FULLSCREEN 0
#define EXPERIMENTAL_FEATURES 0
/* ## DEBUG MODE ##
 * 0 for all
 * 1 for nothing
 * 2 for FPS
 * 3 for cursor position
 * 5 for scroll level and precision
 * 
 * For instance if you want fps and position set the value to 2*3=6
 */
#define DEBUG_MODE 1

GLuint screenWidth = 720, screenHeight = 480;
const GLFWvidmode* mode;
GLFWwindow* window;

bool pause;

void setupVAO();
//GLuint getTextureHandle(char* path);
unsigned int VAO;

float currentTime, deltaTime, lastFrame,startTime;
float mousePosX,mousePosY;

float speedlevel=4.;
float camPosX=2.5;
float camPosY=0.5;
float camPosZ=2.5;
float speed=.04;
float pan=0.;
float multiplicatorFov=1.;
float tilt=0.;
float ez[3] = {0};
float ex[3] = {0};
float ey[3] = {0};

bool upar = false;
bool downar = false;
bool leftar = false;
bool rightar = false;
bool forwardar = false;
bool backwardar = false;

float fovValue=1.0;
//281=3.13/2 * 180
const int maxYmouse = 281;
// more precision means less speed
float camPrecision = 2.;

/*float[3] crossProduct(float vect_A[3], float vect_B[3])
 
{
  float cross_P[3];
  cross_P[0] = vect_A[1] * vect_B[2] - vect_A[2] * vect_B[1];
  cross_P[1] = vect_A[2] * vect_B[0] - vect_A[0] * vect_B[2];
  cross_P[2] = vect_A[0] * vect_B[1] - vect_A[1] * vect_B[0];
  return cross_P;
}*/

static void key_callback(GLFWwindow* window, int key, int scancode, int action, int mods)
{
  int stateShift = glfwGetKey(window, GLFW_KEY_LEFT_SHIFT);
  int stateControl = glfwGetKey(window, GLFW_KEY_LEFT_CONTROL);
  if (stateShift == GLFW_PRESS) {
    speed=.08*speedlevel;
    multiplicatorFov=0.8;
  }
  else {
    speed=.02*speedlevel;
    multiplicatorFov=1.;
  }
  
  if (action == GLFW_PRESS) {
    if (key == GLFW_KEY_ESCAPE)
    {
      //glfwSetWindowShouldClose(window, GLFW_TRUE);
      pause=!pause;
      printf(pause ? "En pause\n" : "En fonctionnement\n");
      if (pause)
      {
        glfwSetWindowTitle(window, "EnginPasTangible (En pause)");
        glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
      }
      else
      {
        glfwSetWindowTitle(window, APPNAMEVERSION);
        glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
      }
    }
    if (key == GLFW_KEY_BACKSPACE)
      glfwSetWindowShouldClose(window, GLFW_TRUE);
    if (key == GLFW_KEY_SPACE)
      upar = true;
    if (key == GLFW_KEY_LEFT_CONTROL)
      downar = true;
    if (key == GLFW_KEY_UP || key == GLFW_KEY_W) {
      forwardar = true;
    }
    if (key == GLFW_KEY_DOWN || key == GLFW_KEY_S ) {
      backwardar = true;
    }
    if (key == GLFW_KEY_RIGHT || key == GLFW_KEY_D) {
      rightar = true;
    }
    if (key == GLFW_KEY_LEFT || key == GLFW_KEY_A ) {
      leftar = true;
    }
  }
  else if (action == GLFW_RELEASE) {
    if (key == GLFW_KEY_SPACE)
      upar = false;
    if (key == GLFW_KEY_LEFT_CONTROL)
      downar = false;
    if (key == GLFW_KEY_UP || key == GLFW_KEY_W) {
      forwardar = false;
    }
    if (key == GLFW_KEY_DOWN || key == GLFW_KEY_S ) {
      backwardar = false;
    }
    if (key == GLFW_KEY_RIGHT || key == GLFW_KEY_D) {
      rightar = false;
    }
    if (key == GLFW_KEY_LEFT || key == GLFW_KEY_A ) {
      leftar = false;
    }
  }
}

void scroll_callback(GLFWwindow* window, double xoffset, double yoffset)
{
  int stateControl = glfwGetKey(window, GLFW_KEY_LEFT_CONTROL);
  
  // we only use yoffset as it is present on normal mice
  if (DEBUG_MODE % 5 == 0) {
    printf("scroll value : %f | precision : %f | ",yoffset, camPrecision);
    printf((stateControl == GLFW_PRESS) ? "ctrl -> speed" : "Zooming");
    printf("\n");
  }
  if (stateControl == GLFW_PRESS) {
    camPrecision += yoffset/2;
    if (camPrecision <= 1)
      camPrecision=1;
  }
  else {
    fovValue += yoffset/5;
    if (fovValue <= 0.2 && EXPERIMENTAL_FEATURES == 0)
      fovValue=0.2;
  }
  
}

static void cursor_position_callback(GLFWwindow* window, double xpos, double ypos)
{
  /*
  //if (xpos>283){ //////283=3.14/2 * 180
  //	//xpos=283
  //}
  //if (xpos<-283){
  //	//xpos=-283
  //}*/
  int maxYcorrected = maxYmouse*camPrecision;
  if (ypos>maxYcorrected){
    glfwSetCursorPos(window, xpos, maxYcorrected);
  }
  if (ypos<-maxYcorrected){
  	glfwSetCursorPos(window, xpos, -maxYcorrected);
  }
  
  if (DEBUG_MODE % 3 == 0)
    printf("x:%f | y:%f\n",xpos, ypos);
  mousePosX = xpos/camPrecision;
  mousePosY = ypos/camPrecision;
}

int main (){

  // Window setup
  GLint glfwStatus = glfwInit();

  //glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
  //glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
  //glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
  //glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
  //// glfwWindowHint(GLFW_SAMPLES, 4);
  glfwWindowHint(GLFW_RESIZABLE,GL_FALSE);//#####
	//glfwWindowHint(GLFW_DECORATED,GL_FALSE);
	//glfwWindowHint(GLFW_CONTEXT_NO_ERROR,GL_FALSE);
  pause = false;
  window = glfwCreateWindow(screenWidth, screenHeight, APPNAMEVERSION, NULL, NULL);
    
  if (window == NULL)
  {
    printf("Window failed to create");
    glfwTerminate();
  }

  glfwMakeContextCurrent(window);
  //glfwSwapInterval(1); // To my knowledge, this turns on vsync on macOS

  // If Windows or Linux: load all OpenGL function pointers with GLAD
  if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
  {
    printf("Failed to initialize GLAD");
    return -1;
  }
  // END Window setup

  // set up Vertex Array Object that contains our vertices and bind it
  setupVAO();
  glBindVertexArray(VAO); // seeing as we only have a single VAO there's no need to unbind in the setupVertexArray function and then bind here, but we'll do so for clarity, organization, and avoiding possible bugs in future
    
  GLuint quad_shader = glCreateProgram();
  int compilerInfo=buildShaders(quad_shader, "shaders/generic.vs", SCENE);
  if(compilerInfo>0){
  	return compilerInfo;
  }
  glUseProgram(quad_shader);
		
  //GLuint channel_logo = getTextureHandle("assets/logo.png");
  //glBindTexture(GL_TEXTURE_2D, channel_logo);

  // for alpha (opacity)
  //glEnable (GL_BLEND); glBlendFunc (GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

  glfwSetKeyCallback(window, key_callback);
 	glfwSetCursorPosCallback(window, cursor_position_callback);
  glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
  glfwSetScrollCallback(window, scroll_callback);

  GLFWimage images[1]; 
  images[0].pixels = stbi_load("./assets/icon.png", &images[0].width, &images[0].height, 0, 4); //rgba channels 
  glfwSetWindowIcon(window, 1, images); 
  stbi_image_free(images[0].pixels);

  int window_width, window_height;
	char FPS[20];
	startTime = glfwGetTime();
  while (!glfwWindowShouldClose(window))
  {
    // glfw: swap buffers and poll IO events (keys pressed/released, mouse moved etc.)
    // -------------------------------------------------------------------------------
    glfwSwapBuffers(window);
    glfwPollEvents();

    if (pause)
    {
      glClearColor(.1f, .2f, 0.3f, 1.0f);
      glClear(GL_COLOR_BUFFER_BIT);
      continue;
    }

    pan=-mousePosX/180.;
	  tilt=-mousePosY/180.;
    //ez
    ez[0] = cos(tilt)*sin(pan);
    ez[1] = sin(tilt);
    ez[2] = cos(tilt)*cos(pan);//normalize(lookingAt-posCam);////base orthonormée
	  //ex
    ex[0] = -ez[2];//crossProduct(ez,{0.,1.,0.});
    ex[2] = ez[0];
	  // ey
    ey[0] = ex[1] * ez[2] - ex[2] * ez[1];
    ey[1] = ex[2] * ez[0] - ex[0] * ez[2];
    ey[2] = ex[0] * ez[1] - ex[1] * ez[0];
    //crossProduct(ex,ez);

  if (upar)
    camPosY += speed;
  if (downar)
    camPosY -= speed;
  if (forwardar) {
    camPosX += speed*ez[0];
    camPosY += speed*ez[1];
    camPosZ += speed*ez[2];
  }
  if (backwardar) {
    camPosX -= speed*ez[0];
    camPosY -= speed*ez[1];
    camPosZ -= speed*ez[2];
  }
  if (rightar) {
    camPosX += speed*ex[0];
    camPosY += speed*ex[1];
    camPosZ += speed*ex[2];
  }
  if (leftar) {
    camPosX -= speed*ex[0];
    camPosY -= speed*ex[1];
    camPosZ -= speed*ex[2];
  }

    currentTime = glfwGetTime();
    deltaTime = currentTime - lastFrame;
    lastFrame = currentTime;
    gcvt(1/deltaTime,4,FPS);
    if (DEBUG_MODE % 2 == 0)
      printf("FPS : %s\n",FPS);

    glfwGetWindowSize(window, &window_width, &window_height);
    glViewport(0, 0, window_width, window_height);
    // printf("%d\n", window_width);
        
    glClearColor(0.9f, 0.9f, 0.2f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT);

    //glDrawArrays(GL_TRIANGLES, 0, 6);
    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);
    glUniform1f(glGetUniformLocation(quad_shader, "iTime"), currentTime-startTime);
		glUniform3f(glGetUniformLocation(quad_shader, "iEx"), ex[0],ex[1],ex[2]);
    glUniform3f(glGetUniformLocation(quad_shader, "iEy"), ey[0],ey[1],ey[2]);
    glUniform3f(glGetUniformLocation(quad_shader, "iEz"), ez[0],ez[1],ez[2]);
		glUniform3f(glGetUniformLocation(quad_shader, "iCamPos"), camPosX,camPosY,camPosZ);
		glUniform1f(glGetUniformLocation(quad_shader, "iFovValue"), fovValue*fovValue*multiplicatorFov);
    // glBindVertexArray(0); // no need to unbind it every time 
  }

  // Optional cleaning up bc OS will likely do it for us, but is a good practice. Note that shaders are deleted in shader.h

  glDeleteProgram(quad_shader);

  // glfw: terminate, clearing all previously allocated GLFW resources.
  glfwTerminate();

  return 0;

}

void setupVAO(){
  // set up vertex data (and buffer(s)) and configure vertex attributes
  // ------------------------------------------------------------------
  float vertices[] = {
        // positions            // textures
         1.f,  1.0f, 0.0f,     1.5f, 1.0f, // top right
         1.f, -1.f, 0.0f,     1.5f, -1.0f, // bottom right/////-1 => 0.
        -1.f, -1.f, 0.0f,     -1.5f, -1.0f, // bottom left
        -1.f,  1.f, 0.0f,     -1.5f, 1.0f // top left 
  };

  unsigned int indices[] = {  
        0, 1, 3,  // first Triangle
        1, 2, 3   // second Triangle
  };

  unsigned int VBO, EBO;
  glGenVertexArrays(1, &VAO);
  glGenBuffers(1, &VBO);
  glGenBuffers(1, &EBO);
  // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
  glBindVertexArray(VAO);

  glBindBuffer(GL_ARRAY_BUFFER, VBO);
  glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

  glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
  glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);
    
  // Position attribute
  glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 5 * sizeof(GLfloat), (GLvoid*)0);
  glEnableVertexAttribArray(0);
  // TexCoord attribute
  glVertexAttribPointer(2, 2, GL_FLOAT, GL_TRUE, 5 * sizeof(GLfloat), (GLvoid*)(3 * sizeof(GLfloat)));
  glEnableVertexAttribArray(2);


  // note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
  glBindBuffer(GL_ARRAY_BUFFER, 0);

  // remember: do NOT unbind the EBO while a VAO is active as the bound element buffer object IS stored in the VAO; keep the EBO bound.
  //glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

  // You can unbind the VAO afterwards so other VAO calls won't accidentally modify this VAO, but this rarely happens. Modifying other
  // VAOs requires a call to glBindVertexArray anyways so we generally don't unbind VAOs (nor VBOs) when it's not directly necessary.
  glBindVertexArray(0);
}


