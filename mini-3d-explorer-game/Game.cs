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
        private Shader _shader;

        private Texture _texture;

        // The view and projection matrices have been removed as we don't need them here anymore.
        // They can now be found in the new camera class.

        // We need an instance of the new camera class so it can manage the view and projection matrix code.
        // We also need a boolean set to true to detect whether or not the mouse has been moved for the first time.
        // Finally, we add the last position of the mouse so we can calculate the mouse offset easily.
        private Camera _camera;

        private RectangularPrismMesh[] _mesh;
        private RectangularPrismMesh interactive;

        private PointLight[] _lights = Array.Empty<PointLight>();
        private bool canSpawnLight = true;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Console.WriteLine("Press E to toggle no clip");

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            interactive = new RectangularPrismMesh(new Vector3(0, 4, 0), new Vector3(0.5f, 0.5f, 0.5f));
            _mesh = new RectangularPrismMesh[]
            {
                // base
                new RectangularPrismMesh(new Vector3(0, -1, 0), new Vector3(1, 2, 1)), new RectangularPrismMesh(new Vector3(1, -1, 0), new Vector3(1, 1, 1)), new RectangularPrismMesh(new Vector3(2, -1, 0), new Vector3(1, 1, 1)),
                new RectangularPrismMesh(new Vector3(0, -1, 1), new Vector3(1, 1, 1)), new RectangularPrismMesh(new Vector3(2, -1, 1), new Vector3(1, 1, 1)),
                new RectangularPrismMesh(new Vector3(0, -1, 2), new Vector3(1, 1, 1)), new RectangularPrismMesh(new Vector3(1, -1, 2), new Vector3(1, 1, 1)), new RectangularPrismMesh(new Vector3(2, -1, 2), new Vector3(1, 1, 1)),
                interactive, // add interactive object to meshes
            };

            interactive.collisionCallback = () =>
            {
                Console.WriteLine("touching me!");
            };


            //_vertexBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            //GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            //_elementBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            _texture = Texture.LoadFromFile("Assets/column.jpg");
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

            //int index = 0;
            //foreach(PointLight light in _lights)
            //{
            //    string access = $"pointLights[{index}].";
            //    _shader.SetVector3($"{access}objectColor", new Vector3(1.0f, 1.0f, 1.0f));
            //    _shader.SetVector3($"{access}lightColor", light.lightColor);
            //    _shader.SetVector3($"{access}lightPos", light.lightPos);
            //    _shader.SetVector3($"{access}viewPos", _camera.Position);
            //    index++;
            //}
            //_shader.SetInt("lightCount", _lights.Length);

            foreach (RectangularPrismMesh mesh in _mesh)
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
                this._camera.noClip = !this._camera.noClip;
                Console.WriteLine($"No clip = {(this._camera.noClip ? "On" : "Off")}");
            }

            if(input.IsKeyReleased(Keys.E))
            {
                canSpawnLight = true;
            }

            _camera.Update((float)e.Time, KeyboardState, MouseState, _mesh);
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
