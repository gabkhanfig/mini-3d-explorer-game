using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace explorer
{
    public struct Vertex {
        public Vector3 position;
        public Vector3 normal;
        public Vector3 color;
        public Vector2 texCoord;
        public Vertex(Vector3 pos, Vector3 norm, Vector3 col, Vector2 tex)
        {
            position = pos;
            normal = norm;
            color = col;
            FunnyNormalize(ref col);
            texCoord = tex;
        }

        static void FunnyNormalize(ref Vector3 v)
        {
            while (v.X < 0)
            {
                v.X += 1;
            }
            while (v.X > 1)
            {
                v.X -= 1;
            }
            while (v.Y < 0)
            {
                v.Y += 1;
            }
            while (v.Y > 1)
            {
                v.Y -= 1;
            }
            while (v.Z < 0)
            {
                v.Z += 1;
            }
            while (v.Z > 1)
            {
                v.Z -= 1;
            }
        }

        public static void setupVertexAttribLayout(int vertexArrayHandle, int vertexBufferHandle)
        {
            GL.BindVertexArray(vertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);

            const int totalStride = (3 + 3 + 3 + 2) * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, totalStride, 0); // vertex shader layout location 0 position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, totalStride, 12); // vertex shader layout location 1 normal
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, totalStride, 24); // vertex shader layout location 2 colour
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, totalStride, 36); // vertex shader layout location 2 texture
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }

    public class Square
    {
        public Vertex v00;
        public Vertex v01;
        public Vertex v10;
        public Vertex v11;

        public Square(Vector3 baseVector, float length, Vector3 normal)
        {
            Vector3 inverseNorm = flipNormal(normal);
            Vector3 p00 = baseVector;
            Vector3 p11 = baseVector + (inverseNorm * new Vector3(length));
            Vector3 p01 = Vector3.Zero;
            Vector3 p10 = Vector3.Zero;
            if (normal.X == 1 || normal.X == -1)
            {
                p01 = baseVector + new Vector3(0, length, 0);
                p10 = baseVector + new Vector3(0, 0, length);
            }
            else if (normal.Y == 1 || normal.Y == -1)
            {
                p01 = baseVector + new Vector3(length, 0, 0);
                p10 = baseVector + new Vector3(0, 0, length);
            }
            else if (normal.Z == 1 || normal.Z == -1)
            {
                p01 = baseVector + new Vector3(length, 0, 0);
                p10 = baseVector + new Vector3(0, length, 0);
            }

            v00 = new Vertex(p00, normal, new Vector3(1, 1, 1), new Vector2(0, 0));
            v01 = new Vertex(p01, normal, new Vector3(1, 1, 1), new Vector2(0, 1));
            v10 = new Vertex(p10, normal, new Vector3(1, 1, 1), new Vector2(1, 0));
            v11 = new Vertex(p11, normal, new Vector3(1, 1, 1), new Vector2(1, 1));
        }

        static Vector3 flipNormal(Vector3 v)
        {
            Vector3 inverse = new Vector3();
            inverse.X = (v.X == 0) ? 1 : 0;
            inverse.Y = (v.Y == 0) ? 1 : 0;
            inverse.Z = (v.Z == 0) ? 1 : 0;
            return inverse;
        }
    }


    public class CubeMesh
    {
        public int vertexBufferHandle;
        public int vertexCount;

        public CubeMesh(Vector3 position, float length)
        {
            Square bottom = new Square(position, length, Vector3.UnitY);
            Square north = new Square(position, length, Vector3.UnitX);
            Square east = new Square(position, length, Vector3.UnitZ);
            Square south = new Square(new Vector3(position.X + length, position.Y, position.Z), length, -Vector3.UnitX);
            Square west = new Square(new Vector3(position.X, position.Y, position.Z + length), length, -Vector3.UnitZ);
            Square top = new Square(new Vector3(position.X, position.Y + length, position.Z), length, -Vector3.UnitY);

            Vertex[] vertices = new Vertex[] // first three vertices are the position, next 3 are colour
            {
                bottom.v00, bottom.v01, bottom.v11,
                bottom.v00, bottom.v11, bottom.v10,

                north.v00, north.v10, north.v11,
                north.v00, north.v11, north.v01,

                east.v01, east.v00, east.v10,
                east.v01, east.v10, east.v11,

                south.v10, south.v00, south.v01,
                south.v10, south.v01, south.v11,

                west.v00, west.v01, west.v11,
                west.v00, west.v11, west.v10,

                top.v00, top.v10, top.v11,
                top.v00, top.v11, top.v01
            };

            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float) * (3 + 3 + 3 + 2), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Unbind to prevent accidental modifications
            vertexCount = vertices.Length;
        }

        public void draw()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
