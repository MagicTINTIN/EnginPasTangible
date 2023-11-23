cmake_minimum_required(VERSION 3.14)
project(openglTuto)


add_executable(ept EnginPasTangible.c ./Libraries/glad/glad.c ./Libraries/GLFW/Linux/libglfw.so.3.3)

find_package(glfw3 3.3 REQUIRED)
find_package(OpenGL REQUIRED)

target_include_directories(ept PUBLIC
                           $<BUILD_INTERFACE:${CMAKE_CURRENT_LIST_DIR}/include>
                           $<INSTALL_INTERFACE:include>)
target_link_libraries(ept PUBLIC -lm glfw OpenGL::GL ${CMAKE_DL_LIBS})