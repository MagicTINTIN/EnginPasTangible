#define APPNAMEVERSION "EnginPasTangible (alpha 0.2.4)"
#include "./Libraries/glad/glad.h"
#include <stdio.h>
#include <math.h>
#include <stdbool.h>
#include "./Libraries/GLFW/glfw3.h"
#define STB_IMAGE_IMPLEMENTATION
#include "./Libraries/stb/stb_image.h"
#include "headers/shader.h"
#define FULLSCREEN 0
/* ## DEBUG MODE ##
 * 0 for all
 * 1 for nothing
 * 2 for FPS
 * 3 for cursor position
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

static void key_callback(GLFWwindow* window, int key, int scancode, int action, int mods)
{
  if (key == GLFW_KEY_ESCAPE && action == GLFW_PRESS)
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
  /*if (key == GLFW_KEY_SPACE && action == GLFW_PRESS)
    glfwSetCursorPos(window, 640/2, 480/2);*/
}


static void cursor_position_callback(GLFWwindow* window, double xpos, double ypos)
{
  if (DEBUG_MODE % 3 == 0)
    printf("x:%f | y:%f\n",xpos, ypos);
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
  int compilerInfo=buildShaders(quad_shader, "shaders/generic.vs", "shaders/quad.fs");
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

  GLFWimage images[1]; 
  images[0].pixels = stbi_load("./assets/icon.png", &images[0].width, &images[0].height, 0, 4); //rgba channels 
  glfwSetWindowIcon(window, 1, images); 
  stbi_image_free(images[0].pixels);

  int window_width, window_height;
  float mousePosX,mousePosY,camPosX,camPosY,camPosZ,camDirX,camDirY,camDirZ;
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
		glUniform2f(glGetUniformLocation(quad_shader, "iMousePos"), mousePosX,mousePosY);
		glUniform3f(glGetUniformLocation(quad_shader, "iCamPos"), camPosX,camPosY,camPosZ);
		glUniform3f(glGetUniformLocation(quad_shader, "iCamDir"), camDirX,camDirY,camDirZ);
				
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
/*
GLuint getTextureHandle(char* path)
{
    GLuint textureHandle;
    glGenTextures(1, &textureHandle);
    glBindTexture(GL_TEXTURE_2D, textureHandle); // All upcoming GL_TEXTURE_2D operations now have effect on our texture object
    
    // Set our texture parameters
    // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);   // Set texture wrapping to GL_REPEAT
    // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT); 
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER);   // Set texture wrapping to GL_CLAMP_TO_BORDER
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER);
    
    // Set texture filtering
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

    // Load, create texture and generate mipmaps; 
    //
    // Note: image loaders usually think of top left as being (0,0) while in OpenGL I would rather think of bottom left as being (0,0) as OpenGL does that already, so that is why I set the stb library to flip image vertically. There are other workarounds like flipping our texCoords upside down or flipping things in the vs or fs, but that would mean that we are choosing in OpenGL to work with two different coordinate systems, one upside-down from the other. I would rather choose not to do that and simply flip images when loading in. It is a matter of personal choice.
    // 

    int width, height, nrChannels;
    stbi_set_flip_vertically_on_load(1);
    unsigned char *image = stbi_load(path, &width, &height, &nrChannels, 0);
    
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, image);
    glGenerateMipmap(GL_TEXTURE_2D);
   
    // free memory 
    stbi_image_free(image);
    glBindTexture(GL_TEXTURE_2D, 0); // unbind so that we can deal with other textures

    return textureHandle;
}
*/

