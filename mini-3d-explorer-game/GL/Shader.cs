using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace explorer
{
    //public class Shader
    //{
    //    public int shaderProgramHandle;

    //    public static string vertexShaderCode = @"
    //            #version 330 core
    //            layout(location = 0) in vec3 aPosition; // Vertex position input
    //            layout(location = 1) in vec3 aNormal; // Vertex normal input
    //            layout(location = 2) in vec3 aColour; // Vertex colour input
    //            layout(location = 3) in vec2 aTexCoord; // Vertex texture input

    //            uniform mat4 model;
    //            uniform mat4 view;
    //            uniform mat4 projection;

    //            out vec3 fragPos;
    //            out vec3 normal;
    //            out vec3 colour;
    //            out vec2 texCoord;

    //            void main()
    //            {
    //                // gl_Position = view * vec4(aPosition, 1.0);
    //                fragPos = vec3(model * vec4(aPosition, 1.0));
    //                normal = mat3(transpose(inverse(model))) * aNormal;
    //                gl_Position = projection * view * vec4(aPosition, 1.0);
    //                colour = aColour;
    //                texCoord = aTexCoord;
    //            }
    //        ";

    //    // Fragment shader: outputs a single color
    //    public static string fragmentShaderCode = @"
    //            #version 330 core
    //            out vec4 FragColor;
    //            in vec3 fragPos;
    //            in vec3 normal;
    //            in vec3 colour;
    //            in vec2 texCoord;

    //            uniform vec3 lightPos; // Position of the point light
    //            uniform vec3 viewPos;  // Camera position
    //            uniform vec3 lightColor; // Color of the light
    //            uniform vec3 objectColor; // Color of the object

    //            uniform sampler2D ourTexture;

    //            void main()
    //            {
    //                FragColor = vec4(colour.r, colour.g, colour.b, 1.0f) * texture(ourTexture, texCoord);
    //                // FragColor = vec4(colour.r, colour.g, colour.b, 1.0f);

    //                // Ambient
    //                float ambientStrength = 0.15;
    //                vec3 ambient = ambientStrength * lightColor;

    //                // Diffuse
    //                vec3 norm = normalize(normal);
    //                vec3 lightDir = normalize(lightPos - fragPos);
    //                float diff = max(dot(norm, lightDir), 0.0);
    //                vec3 diffuse = diff * lightColor;

    //                // Specular
    //                float specularStrength = 0.9;
    //                vec3 viewDir = normalize(viewPos - fragPos);
    //                vec3 reflectDir = reflect(-lightDir, norm);
    //                float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    //                vec3 specular = specularStrength * spec * lightColor;

    //                // Combine results
    //                vec3 result = (ambient + diffuse + specular) * objectColor;
    //                // FragColor = vec4(result, 1.0) * texture(ourTexture, texCoord);
    //            }
    //        ";

    //    public Shader(string vertexSource, string fragmentSource)
    //    {
    //        // Compile shaders
    //        int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
    //        GL.ShaderSource(vertexShaderHandle, vertexSource);
    //        GL.CompileShader(vertexShaderHandle);
    //        CheckShaderCompile(vertexShaderHandle, "Vertex Shader");

    //        int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
    //        GL.ShaderSource(fragmentShaderHandle, fragmentSource);
    //        GL.CompileShader(fragmentShaderHandle);
    //        CheckShaderCompile(fragmentShaderHandle, "Fragment Shader");

    //        // Create shader program and link shaders
    //        shaderProgramHandle = GL.CreateProgram();
    //        GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
    //        GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
    //        GL.LinkProgram(shaderProgramHandle);
    //    }

    //    private static void CheckShaderCompile(int shaderHandle, string shaderName)
    //    {
    //        GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
    //        if (success == 0)
    //        {
    //            string infoLog = GL.GetShaderInfoLog(shaderHandle);
    //            Console.WriteLine($"Error compiling {shaderName}: {infoLog}");
    //        }
    //    }

    //    public void bind()
    //    {
    //        GL.UseProgram(shaderProgramHandle);
    //    }

    //    public void setUniformVec3(string name, Vector3 value)
    //    {
    //        int loc = GL.GetUniformLocation(shaderProgramHandle, name);
    //        GL.Uniform3(loc, value.X, value.Y, value.Z);
    //    }

    //    public void setUniformMatrix4(string name, Matrix4 value)
    //    {
    //        int loc = GL.GetUniformLocation(shaderProgramHandle, name);
    //        GL.UniformMatrix4(loc, true, ref value);
    //    }
    //}

    public struct PointLight
    {
        public Vector3 objectColor; //The color of the object.
        public Vector3 lightColor; //The color of the light.
        public Vector3 lightPos; //The position of the light.
        public Vector3 viewPos; //The position of the view and/or of the player.
    };

    // A simple class meant to help create shaders.
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert.
        public Shader(string vertPath, string fragPath)
        {
            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.

            // Load vertex shader and compile
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, shaderSource);

            // And then compile
            CompileShader(vertexShader);

            // We do the same for the fragment shader.
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            Handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(loc, data);
        }
    }
}
