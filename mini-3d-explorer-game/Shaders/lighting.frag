#version 330 core
out vec4 FragColor;

struct PointLight {
    vec3 objectColor; //The color of the object.
    vec3 lightColor; //The color of the light.
    vec3 lightPos; //The position of the light.
    vec3 viewPos; //The position of the view and/or of the player.
};
uniform PointLight pointLights[64];
uniform int lightCount;

in vec3 fragPos;
in vec3 normal;
in vec3 colour;
in vec2 texCoord;

uniform sampler2D ourTexture;

vec3 CalcPointLight(PointLight light, vec3 theNormal, vec3 pos)
{
    //The ambient color is the color where the light does not directly hit the object.
    //You can think of it as an underlying tone throughout the object. Or the light coming from the scene/the sky (not the sun).
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * light.lightColor;

    //We calculate the light direction, and make sure the normal is normalized.
    vec3 norm = normalize(theNormal);
    vec3 lightDir = normalize(light.lightPos - pos); //Note: The light is pointing from the light to the fragment

    //The diffuse part of the phong model.
    //This is the part of the light that gives the most, it is the color of the object where it is hit by light.
    float diff = max(dot(norm, lightDir), 0.0); //We make sure the value is non negative with the max function.
    vec3 diffuse = diff * light.lightColor;


    //The specular light is the light that shines from the object, like light hitting metal.
    //The calculations are explained much more detailed in the web version of the tutorials.
    float specularStrength = 0.5;
    vec3 viewDir = normalize(light.viewPos - pos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32); //The 32 is the shininess of the material.
    vec3 specular = specularStrength * spec * light.lightColor;

    //At last we add all the light components together and multiply with the color of the object. Then we set the color
    //and makes sure the alpha value is 1
    vec3 result = (ambient + diffuse + specular) * light.objectColor;
    return result;
} 

void main()
{
    vec3 result = vec3(0);
    if(lightCount == 0) { // ifs on uniforms are considered ok
        result = vec3(1);
    }
    for(int i = 0; i < lightCount; i++)
        result += CalcPointLight(pointLights[i], normal, fragPos);

    FragColor = vec4(result, 1.0) * texture(ourTexture, texCoord);
    // FragColor = texture(ourTexture, texCoord);
}