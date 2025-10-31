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
    //internal class Game : GameWindow
    //{
    //    public Shader? shader;
    //    public Camera camera;
    //    public CubeMesh[] cubes;
    //    public Texture tex;
    //    public int vertexArrayHandle;

    //    public Game()
    //        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    //    {
    //        this.Size = new Vector2i(768, 768);

    //        this.camera = new Camera(new Vector3(0, 0, 10), 1);

    //        this.CenterWindow(this.Size);
    //    }

    //    protected override void OnResize(ResizeEventArgs e)
    //    {
    //        GL.Viewport(0, 0, e.Width, e.Height);
    //        base.OnResize(e);
    //    }

    //    protected override void OnLoad()
    //    {
    //        base.OnLoad();

    //        GL.ClearColor(new Color4(0.5f, 0.7f, 0.8f, 1f));
    //        //GL.Enable(EnableCap.CullFace);
    //        //GL.CullFace(CullFaceMode.Front);

    //        cubes = new CubeMesh[] { 
    //            new CubeMesh(new Vector3(-0.25f, -0.25f, -0.25f), 0.5f),
    //            new CubeMesh(new Vector3(-10, -5, 0), -.5f)
    //        };

    //        vertexArrayHandle = GL.GenVertexArray();

    //        shader = new Shader(Shader.vertexShaderCode, Shader.fragmentShaderCode);

    //        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    //        string projectDirectory = Directory.GetParent(baseDir).Parent.Parent.Parent.FullName;
    //        Console.WriteLine(projectDirectory);
    //        string texturePath = Path.Combine(projectDirectory, "Assets", "wall.jpg");
    //        Console.WriteLine(texturePath);
    //        tex = new Texture(texturePath);
    //    }

    //    protected override void OnUpdateFrame(FrameEventArgs args)
    //    {

    //        camera.Update((float)args.Time, KeyboardState, MouseState);

    //        // Console.WriteLine($"Camera Position: {camera.cameraPos.X}, {camera.cameraPos.Y}, {camera.cameraPos.Z}");

    //        base.OnUpdateFrame(args);
    //        // Handle input, animations, physics, AI, etc.
    //    }

    //    protected override void OnRenderFrame(FrameEventArgs args)
    //    {
    //        base.OnRenderFrame(args);

    //        Matrix4 model = Matrix4.Identity;
    //        Matrix4 view = camera.getView();
    //        Matrix4 projection = camera.getProjection((float)this.Size.X, (float)this.Size.Y);

    //        Console.Clear();
    //        Console.WriteLine($"[{projection.M11,10:F5} {projection.M12,10:F5} {projection.M13,10:F5} {projection.M14,10:F5}]");
    //        Console.WriteLine($"[{projection.M21,10:F5} {projection.M22,10:F5} {projection.M23,10:F5} {projection.M24,10:F5}]");
    //        Console.WriteLine($"[{projection.M31,10:F5} {projection.M32,10:F5} {projection.M33,10:F5} {projection.M34,10:F5}]");
    //        Console.WriteLine($"[{projection.M41,10:F5} {projection.M42,10:F5} {projection.M43,10:F5} {projection.M44,10:F5}]");

    //        GL.Clear(ClearBufferMask.ColorBufferBit);

    //        shader?.bind();
    //        tex.bind();

    //        shader?.setUniformMatrix4("model", model);
    //        shader?.setUniformMatrix4("view", view);
    //        shader?.setUniformMatrix4("projection", projection);

    //        foreach (CubeMesh cube in cubes)
    //        {
    //            GL.BindVertexArray(vertexArrayHandle);
    //            Vertex.setupVertexAttribLayout(vertexArrayHandle, cube.vertexBufferHandle);
    //            GL.BindVertexArray(vertexArrayHandle);
    //            cube.draw();
    //            GL.BindVertexArray(0);
    //        }

    //        SwapBuffers();
    //    }
    //}

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

        private int _elementBufferObject;

        private int _vertexBufferObject;

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

        private CubeMesh _mesh;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _mesh = new CubeMesh(Vector3.Zero, 1);

            //_vertexBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            //GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            //_elementBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
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
            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            _texture.Use(TextureUnit.Texture0);
            _shader.Use();

            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Triangles, _mesh.indices.Length, DrawElementsType.UnsignedInt, 0);

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
    }
}
