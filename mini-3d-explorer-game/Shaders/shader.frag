#version 330

out vec4 FragColor;

in vec3 fragPos;
in vec3 normal;
in vec3 colour;
in vec2 texCoord;

uniform sampler2D ourTexture;

void main()
{
    FragColor = vec4(colour.r, colour.g, colour.b, 1.0f) * texture(ourTexture, texCoord);
}