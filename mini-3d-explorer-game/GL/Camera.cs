using OpenTK.Graphics.ES20;
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
    public class Camera
    {
        // Those vectors are directions pointing outwards from the camera to define how it rotated.
        private Vector3 front = -Vector3.UnitZ;

        private Vector3 up = Vector3.UnitY;

        private Vector3 right = Vector3.UnitX;

        // Rotation around the X axis (radians)
        private float pitch;

        // Rotation around the Y axis (radians)
        private float yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.

        private float fov = 90;

        // The position of the camera
        public Vector3 Position { get; set; }

        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public float AspectRatio { private get; set; }

        private bool firstMouseMove = true;

        private Vector2 lastMousePos;

        public float sensitivity = 0.2f;

        public float moveSpeed = 1.5f;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + front, up);
        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.fov), AspectRatio, 0.01f, 100f);
        }

        // This function is going to update the direction vertices using some of the math learned in the web tutorials.
        private void UpdateVectors()
        {
            float yawRadians = MathHelper.DegreesToRadians(yaw);
            float pitchRadians = MathHelper.DegreesToRadians(pitch);

            // First, the front matrix is calculated using some basic trigonometry.
            front.X = MathF.Cos(pitchRadians) * MathF.Cos(yawRadians);
            front.Y = MathF.Sin(pitchRadians);
            front.Z = MathF.Cos(pitchRadians) * MathF.Sin(yawRadians);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            front = Vector3.Normalize(front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        public void Update(float deltaTime, KeyboardState keyboardState, MouseState mouseState)
        {
            // WASD
            if (keyboardState.IsKeyDown(Keys.W))
            {
                this.Position += this.front * this.moveSpeed * deltaTime; // Forward
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                this.Position -= this.front * this.moveSpeed * deltaTime; // Backwards
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                this.Position -= this.right * this.moveSpeed * deltaTime; // Left
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                this.Position += this.right * this.moveSpeed * deltaTime; // Right
            }
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                this.Position += this.up * this.moveSpeed * deltaTime; // Up
            }
            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                this.Position -= this.up * this.moveSpeed * deltaTime; // Down
            }

            // Mouse look at
            if (firstMouseMove)
            {
                lastMousePos = mouseState.Position;
                firstMouseMove = false;
            }
            else
            {
                var deltaX = mouseState.X - lastMousePos.X;
                var deltaY = mouseState.Y - lastMousePos.Y;
                lastMousePos = mouseState.Position;

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                this.yaw += deltaX * this.sensitivity;
                if (this.yaw > 89) this.yaw = 89;
                if (this.yaw < -89) this.yaw = -89;
                this.pitch -= deltaY * this.sensitivity; // Reversed since y-coordinates range from bottom to top

                UpdateVectors();
            }

            // Fov
            fov += (-1.0f * mouseState.ScrollDelta.Y) * 2; // scroll up to zoom in
            if (fov > 90) fov = 90;
            if (fov < 30) fov = 30;
        }
    }
}
