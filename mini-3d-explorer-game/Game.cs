using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace explorer
{
    public class Game : GameWindow
    {
        private readonly float[] _vertices =
        {
            // Position         Normal   Colour   Texture coordinates
             0.5f,  0.5f, 0.0f, 0, 0, 0, 1, 0, 0, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 0, 0, 0, 0, 1, 0, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0, 0, 0, 0, 0, 1, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0, 0, 0, 1, 1, 1, 0.0f, 1.0f, // top left

             0.5f, 0,  0.5f, 0, 0, 0, 1, 0, 0, 1.0f, 1.0f, // top right
             0.5f, 0, -0.5f, 0, 0, 0, 0, 1, 0, 1.0f, 0.0f, // bottom right
            -0.5f, 0, -0.5f, 0, 0, 0, 0, 0, 1, 0.0f, 0.0f, // bottom left
            -0.5f, 0, 0.5f, 0, 0, 0, 1, 1, 1, 0.0f, 1.0f  // top left
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3,
            4, 5, 7,
            5, 6, 7
        };

        private int _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

        // The view and projection matrices have been removed as we don't need them here anymore.
        // They can now be found in the new camera class.

        // We need an instance of the new camera class so it can manage the view and projection matrix code.
        // We also need a boolean set to true to detect whether or not the mouse has been moved for the first time.
        // Finally, we add the last position of the mouse so we can calculate the mouse offset easily.
        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private CubeMesh[] _mesh;

        private PointLight[] _lights = Array.Empty<PointLight>();
        private bool canSpawnLight = true;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Console.WriteLine("Press E to create a light where you are of a random colour");

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _mesh = new CubeMesh[] 
            {
                // base
                new CubeMesh(new Vector3(0, -1, 0), 1), new CubeMesh(new Vector3(1, -1, 0), 1), new CubeMesh(new Vector3(2, -1, 0), 1),
                new CubeMesh(new Vector3(0, -1, 1), 1), new CubeMesh(new Vector3(2, -1, 1), 1),
                new CubeMesh(new Vector3(0, -1, 2), 1), new CubeMesh(new Vector3(1, -1, 2), 1), new CubeMesh(new Vector3(2, -1, 2), 1),

            };


            //_vertexBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            //GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            //_elementBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag");
            _shader.Use();

            const int totalStride = (3 + 3 + 3 + 2) * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, totalStride, 0); // vertex shader layout location 0 position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, totalStride, 12); // vertex shader layout location 1 normal
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, totalStride, 24); // vertex shader layout location 2 colour
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, totalStride, 36); // vertex shader layout location 2 texture
            GL.EnableVertexAttribArray(3);

            _texture = Texture.LoadFromFile("Assets/wall.jpg");
            _texture.Use(TextureUnit.Texture0);


            _shader.SetInt("ourTexture", 0);

            // We initialize the camera so that it is 3 units back from where the rectangle is.
            // We also give it the proper aspect ratio.
            _camera = new Camera(new Vector3(0, 0.5f, 4), Size.X / (float)Size.Y);

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _texture.Use(TextureUnit.Texture0);
            _shader.Use();

            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            int index = 0;
            foreach(PointLight light in _lights)
            {
                string access = $"pointLights[{index}].";
                _shader.SetVector3($"{access}objectColor", new Vector3(1.0f, 1.0f, 1.0f));
                _shader.SetVector3($"{access}lightColor", light.lightColor);
                _shader.SetVector3($"{access}lightPos", light.lightPos);
                _shader.SetVector3($"{access}viewPos", _camera.Position);
                index++;
            }
            _shader.SetInt("lightCount", _lights.Length);

            foreach (CubeMesh mesh in _mesh)
            {
                GL.BindVertexArray(mesh.vertexArrayObject);
                GL.DrawElements(PrimitiveType.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, 0);
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused) // Check to see if the window is focused
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            if(input.IsKeyPressed(Keys.E))
            {
                if (canSpawnLight == false || _lights.Length == 64) return;
                canSpawnLight = false;

                Random random = new Random();
                Vector3 lightCol = new Vector3(
                    (float)random.NextDouble() * (float)random.NextDouble(),
                    (float)random.NextDouble() * (float)random.NextDouble(),
                    (float)random.NextDouble() * (float)random.NextDouble()
                );

                PointLight light = new PointLight();
                light.lightColor = lightCol;
                light.lightPos = _camera.Position;

                Console.WriteLine($"Creating new light at ({light.lightPos.X}, {light.lightPos.Y}, {light.lightPos.Z}), with colour ({light.lightColor.X}, {light.lightColor.Y}, {light.lightColor.Z})");
                
                AddLight(light);
            }

            if(input.IsKeyReleased(Keys.E))
            {
                canSpawnLight = true;
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            // Get the mouse state
            var mouse = MouseState;

            if (_firstMove) // This bool variable is initially set to true.
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
        }

        //// In the mouse wheel function, we manage all the zooming of the camera.
        //// This is simply done by changing the FOV of the camera.
        //protected override void OnMouseWheel(MouseWheelEventArgs e)
        //{
        //    base.OnMouseWheel(e);

        //    _camera.Fov -= e.OffsetY;
        //}

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            // We need to update the aspect ratio once the window has been resized.
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        private void AddLight(PointLight light)
        {
            Array.Resize(ref _lights, _lights.Length + 1);
            _lights[_lights.Length - 1] = light;
        }
    }
}
