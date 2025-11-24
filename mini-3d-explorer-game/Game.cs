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

        private Texture wallTexture;
        private Texture columnTexture;
        private Texture shinglesTexture;
        private Texture woodTexture;
        private Texture crateTexture;

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
        private float crateCollisionTime = 0;
        private int score = 0;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Console.WriteLine("Press E to toggle no clip");
            Console.WriteLine("Press Q near the crate to increase your score");

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            wallTexture = Texture.LoadFromFile("Assets/wall.jpg");
            columnTexture = Texture.LoadFromFile("Assets/column.jpg");
            shinglesTexture = Texture.LoadFromFile("Assets/shingles.jpg");
            woodTexture = Texture.LoadFromFile("Assets/wood.jpg");
            crateTexture = Texture.LoadFromFile("Assets/crate.jpg");

            _shader.SetInt("ourTexture", 0);



            interactive = new RectangularPrismMesh(new Vector3(1.25f, 0, 1.25f), new Vector3(0.5f, 0.5f, 0.5f), crateTexture);
            _mesh = new RectangularPrismMesh[]
            {
                // base
                new RectangularPrismMesh(new Vector3(0, -1, 0), new Vector3(3, 1, 3), woodTexture),
                
                // columns
                new RectangularPrismMesh(new Vector3(0, 0, 0), new Vector3(1, 2, 1), columnTexture),
                new RectangularPrismMesh(new Vector3(2, 0, 0), new Vector3(1, 2, 1), columnTexture),
                new RectangularPrismMesh(new Vector3(2, 0, 2), new Vector3(1, 2, 1), columnTexture),
                new RectangularPrismMesh(new Vector3(0, 0, 2), new Vector3(1, 2, 1), columnTexture),


                // roof
                new RectangularPrismMesh(new Vector3(0, 2, 0), new Vector3(1, 1, 1), shinglesTexture),
                new RectangularPrismMesh(new Vector3(1, 2, 0), new Vector3(1, 1, 1), shinglesTexture),
                new RectangularPrismMesh(new Vector3(2, 2, 0), new Vector3(1, 1, 1), shinglesTexture),
                new RectangularPrismMesh(new Vector3(0, 2, 1), new Vector3(1, 1, 1), shinglesTexture),
                new RectangularPrismMesh(new Vector3(1, 3, 1), new Vector3(1, 1, 1), shinglesTexture),
                new RectangularPrismMesh(new Vector3(2, 2, 1), new Vector3(1, 1, 1), shinglesTexture),
                new RectangularPrismMesh(new Vector3(0, 2, 2), new Vector3(1, 1, 1), shinglesTexture),
                new RectangularPrismMesh(new Vector3(1, 2, 2), new Vector3(1, 1, 1), shinglesTexture),
                new RectangularPrismMesh(new Vector3(2, 2, 2), new Vector3(1, 1, 1), shinglesTexture),

                interactive, // add interactive object to meshes
            };

            interactive.collisionCallback = this.crateCollide;


            _camera = new Camera(new Vector3(0, 0.5f, 4), Size.X / (float)Size.Y);
            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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
                mesh._tex.Use(TextureUnit.Texture0);
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

            if(crateCollisionTime > 0)
            {
                crateCollisionTime -= (float)e.Time;
                if (input.IsKeyPressed(Keys.Q))
                {
                    this.score += 1;
                    Console.WriteLine($"Your score is {this.score}");
                }
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

        private void crateCollide()
        {
            crateCollisionTime = 0.1f;
        }
    }
}
