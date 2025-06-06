using open_civilization.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Example.Utilities
{
    public static class TexturedCubeGenerator
    {
        public static Mesh CreateTexturedCube()
        {
            var vertices = new List<float>();
            var indices = new List<uint>();
            uint vertexCount = 0;

            // Define the 6 faces with proper UV coordinates
            // Front face (Z+)
            AddFace(vertices, indices, ref vertexCount,
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0, 0, 1));

            // Back face (Z-)
            AddFace(vertices, indices, ref vertexCount,
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0, 0, -1));

            // Right face (X+)
            AddFace(vertices, indices, ref vertexCount,
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(1, 0, 0));

            // Left face (X-)
            AddFace(vertices, indices, ref vertexCount,
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-1, 0, 0));

            // Top face (Y+)
            AddFace(vertices, indices, ref vertexCount,
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0, 1, 0));

            // Bottom face (Y-)
            AddFace(vertices, indices, ref vertexCount,
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0, -1, 0));

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        private static void AddFace(List<float> vertices, List<uint> indices, ref uint vertexCount,
            Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
        {
            // Add vertices with position, UV, and normal
            // v0 - bottom left
            vertices.AddRange(new[] { v0.X, v0.Y, v0.Z, 0f, 1f, normal.X, normal.Y, normal.Z });
            // v1 - bottom right
            vertices.AddRange(new[] { v1.X, v1.Y, v1.Z, 1f, 1f, normal.X, normal.Y, normal.Z });
            // v2 - top right
            vertices.AddRange(new[] { v2.X, v2.Y, v2.Z, 1f, 0f, normal.X, normal.Y, normal.Z });
            // v3 - top left
            vertices.AddRange(new[] { v3.X, v3.Y, v3.Z, 0f, 0f, normal.X, normal.Y, normal.Z });

            // Add indices for two triangles
            indices.AddRange(new[] { vertexCount, vertexCount + 1, vertexCount + 2,
                                    vertexCount, vertexCount + 2, vertexCount + 3 });

            vertexCount += 4;
        }
    }
}
