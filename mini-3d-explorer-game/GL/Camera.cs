using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace explorer
{
    // https://opentk.net/learn/chapter1/9-camera.html?tabs=input-opentk4%2Cdelta-time-input-opentk4%2Ccursor-mode-opentk4%2Cmouse-move-opentk4%2Cscroll-opentk4
    //public class Camera
    //{
    //    public Vector3 Position;
    //    private Vector3 Front = -Vector3.UnitZ;
    //    private Vector3 Up = Vector3.UnitY;
    //    private Vector3 Right = Vector3.UnitX;
    //    private float Yaw = -90;
    //    private float Pitch = 0;
    //    private float sensitivity = 0.6f;
    //    private float fov = 45;
    //    public float AspectRatio;
    //    private bool _firstMove = true;
    //    private Vector2 _lastPos;

    //    public Camera(Vector3 position, float aspectRatio)
    //    {
    //        Position = position;
    //        AspectRatio = aspectRatio;
    //    }

    //    public Matrix4 getView()
    //    {
    //        return Matrix4.LookAt(Position, Position + Front, Up);
    //    }

    //    public Matrix4 getProjection()
    //    {
    //        // return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), AspectRatio, 0.01f, 100);
    //        return Matrix4.Identity;

    //        /*
    //         *  [ 2.48504 0.00000 0.00000 0.00000] 
    //         *  [ 0.00000 2.41421 0.00000 0.00000] 
    //         *  [ 0.00000 0.00000 -1.00200 -1.00000] 
    //         *  [ 0.00000 0.00000 -0.20020 0.00000]
    //         */

    //        Matrix4 proj = new Matrix4(
    //            2.48f, 0, 0, 0,
    //            0, 2.41f, 0, 0,
    //            0, 0, -1, -0.2f,
    //            0, 0, -0.2f, 0.8f
    //            );
    //        return proj;
    //    }

    //    public void Update(float deltaTime, KeyboardState input, MouseState mouse)
    //    {
    //        const float cameraSpeed = 1.5f;

    //        if (input.IsKeyDown(Keys.W))
    //        {
    //            Position += Front * cameraSpeed * deltaTime; // Forward
    //        }

    //        if (input.IsKeyDown(Keys.S))
    //        {
    //            Position -= Front * cameraSpeed * deltaTime; // Backwards
    //        }
    //        if (input.IsKeyDown(Keys.A))
    //        {
    //            Position -= Right * cameraSpeed * deltaTime; // Left
    //        }
    //        if (input.IsKeyDown(Keys.D))
    //        {
    //            Position += Right * cameraSpeed * deltaTime; // Right
    //        }
    //        if (input.IsKeyDown(Keys.Space))
    //        {
    //            Position += Up * cameraSpeed * deltaTime; // Up
    //        }
    //        if (input.IsKeyDown(Keys.LeftShift))
    //        {
    //            Position -= Up * cameraSpeed * deltaTime; // Down
    //        }

    //        if (_firstMove) // This bool variable is initially set to true.
    //        {
    //            _lastPos = new Vector2(mouse.X, mouse.Y);
    //            _firstMove = false;
    //        }
    //        else
    //        {
    //            // Calculate the offset of the mouse position
    //            var deltaX = mouse.X - _lastPos.X;
    //            var deltaY = mouse.Y - _lastPos.Y;
    //            _lastPos = new Vector2(mouse.X, mouse.Y);

    //            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
    //            Yaw += deltaX * sensitivity;
    //            Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
    //            UpdateVectors();
    //        }
    //    }

    //    private void UpdateVectors()
    //    {
    //        // First, the front matrix is calculated using some basic trigonometry.
    //        Front.X = MathF.Cos(Pitch) * MathF.Cos(Yaw);
    //        Front.Y = MathF.Sin(Pitch);
    //        Front.Z = MathF.Cos(Pitch) * MathF.Sin(Yaw);

    //        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
    //        Front = Vector3.Normalize(Front);

    //        // Calculate both the right and the up vector using cross product.
    //        // Note that we are calculating the right from the global up; this behaviour might
    //        // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
    //        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
    //        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    //    }


    //}

    public class Camera
    {
        // Those vectors are directions pointing outwards from the camera to define how it rotated.
        private Vector3 _front = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        // Rotation around the X axis (radians)
        private float _pitch;

        // Rotation around the Y axis (radians)
        private float _yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.

        // The field of view of the camera (radians)
        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        // The position of the camera
        public Vector3 Position { get; set; }

        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public float AspectRatio { private get; set; }

        public Vector3 Front => _front;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation.
                // If you want to read more about this you can try researching a topic called gimbal lock
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // The field of view (FOV) is the vertical angle of the camera view.
        // This has been discussed more in depth in a previous tutorial,
        // but in this tutorial, you have also learned how we can use this to simulate a zoom feature.
        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        // This function is going to update the direction vertices using some of the math learned in the web tutorials.
        private void UpdateVectors()
        {
            // First, the front matrix is calculated using some basic trigonometry.
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}
