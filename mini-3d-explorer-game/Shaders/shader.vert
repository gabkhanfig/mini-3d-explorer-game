#version 330 core

layout(location = 0) in vec3 aPosition; // Vertex position input
layout(location = 1) in vec3 aNormal; // Vertex normal input
layout(location = 2) in vec3 aColour; // Vertex colour input
layout(location = 3) in vec2 aTexCoord; // Vertex texture input

out vec3 fragPos;
out vec3 normal;
out vec3 colour;
out vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    fragPos = vec3(model * vec4(aPosition, 1.0));
    normal = mat3(transpose(inverse(model))) * aNormal;
    colour = aColour;
    texCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}
