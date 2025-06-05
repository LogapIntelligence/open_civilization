using open_civilization.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Example.Utilities
{
    public static class MeshGenerator
    {
        public static Mesh CreatePlane(int subdivisions = 10, float size = 10.0f)
        {
            int verticesPerSide = subdivisions + 1;
            int vertexCount = verticesPerSide * verticesPerSide;
            int triangleCount = subdivisions * subdivisions * 2;

            float[] vertices = new float[vertexCount * 8]; // pos(3) + normal(3) + texcoord(2)
            uint[] indices = new uint[triangleCount * 3];

            // Generate vertices
            int vertexIndex = 0;
            for (int z = 0; z < verticesPerSide; z++)
            {
                for (int x = 0; x < verticesPerSide; x++)
                {
                    float xPos = (x / (float)subdivisions - 0.5f) * size;
                    float zPos = (z / (float)subdivisions - 0.5f) * size;

                    // Position
                    vertices[vertexIndex * 8 + 0] = xPos;
                    vertices[vertexIndex * 8 + 1] = 0;
                    vertices[vertexIndex * 8 + 2] = zPos;

                    // Normal (pointing up)
                    vertices[vertexIndex * 8 + 3] = 0;
                    vertices[vertexIndex * 8 + 4] = 1;
                    vertices[vertexIndex * 8 + 5] = 0;

                    // Texture coordinates
                    vertices[vertexIndex * 8 + 6] = x / (float)subdivisions;
                    vertices[vertexIndex * 8 + 7] = z / (float)subdivisions;

                    vertexIndex++;
                }
            }

            // Generate indices
            int indexCount = 0;
            for (int z = 0; z < subdivisions; z++)
            {
                for (int x = 0; x < subdivisions; x++)
                {
                    int topLeft = z * verticesPerSide + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + verticesPerSide;
                    int bottomRight = bottomLeft + 1;

                    // First triangle
                    indices[indexCount++] = (uint)topLeft;
                    indices[indexCount++] = (uint)bottomLeft;
                    indices[indexCount++] = (uint)topRight;

                    // Second triangle
                    indices[indexCount++] = (uint)topRight;
                    indices[indexCount++] = (uint)bottomLeft;
                    indices[indexCount++] = (uint)bottomRight;
                }
            }

            return new Mesh(vertices, indices);
        }

        public static Mesh CreateCube(float size = 1.0f)
        {
            float half = size * 0.5f;

            // Vertices: position(3) + normal(3) + texcoord(2)
            float[] vertices = {
                // Front face
                -half, -half,  half,  0,  0,  1,  0, 0, // 0
                 half, -half,  half,  0,  0,  1,  1, 0, // 1
                 half,  half,  half,  0,  0,  1,  1, 1, // 2
                -half,  half,  half,  0,  0,  1,  0, 1, // 3

                // Back face
                -half, -half, -half,  0,  0, -1,  1, 0, // 4
                -half,  half, -half,  0,  0, -1,  1, 1, // 5
                 half,  half, -half,  0,  0, -1,  0, 1, // 6
                 half, -half, -half,  0,  0, -1,  0, 0, // 7

                // Left face
                -half,  half,  half, -1,  0,  0,  1, 0, // 8
                -half,  half, -half, -1,  0,  0,  1, 1, // 9
                -half, -half, -half, -1,  0,  0,  0, 1, // 10
                -half, -half,  half, -1,  0,  0,  0, 0, // 11

                // Right face
                 half,  half,  half,  1,  0,  0,  1, 0, // 12
                 half, -half,  half,  1,  0,  0,  0, 0, // 13
                 half, -half, -half,  1,  0,  0,  0, 1, // 14
                 half,  half, -half,  1,  0,  0,  1, 1, // 15

                // Top face
                -half,  half, -half,  0,  1,  0,  0, 1, // 16
                -half,  half,  half,  0,  1,  0,  0, 0, // 17
                 half,  half,  half,  0,  1,  0,  1, 0, // 18
                 half,  half, -half,  0,  1,  0,  1, 1, // 19

                // Bottom face
                -half, -half, -half,  0, -1,  0,  1, 1, // 20
                 half, -half, -half,  0, -1,  0,  0, 1, // 21
                 half, -half,  half,  0, -1,  0,  0, 0, // 22
                -half, -half,  half,  0, -1,  0,  1, 0, // 23
            };

            uint[] indices = {
                0,  1,  2,   0,  2,  3,   // Front
                4,  5,  6,   4,  6,  7,   // Back
                8,  9, 10,   8, 10, 11,   // Left
               12, 13, 14,  12, 14, 15,   // Right
               16, 17, 18,  16, 18, 19,   // Top
               20, 21, 22,  20, 22, 23    // Bottom
            };

            return new Mesh(vertices, indices);
        }

        public static Mesh CreateSphere(float radius = 1.0f, int segments = 20)
        {
            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();

            // Generate vertices
            for (int lat = 0; lat <= segments; lat++)
            {
                float theta = lat * MathF.PI / segments;
                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int lon = 0; lon <= segments; lon++)
                {
                    float phi = lon * 2 * MathF.PI / segments;
                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    float x = cosPhi * sinTheta;
                    float y = cosTheta;
                    float z = sinPhi * sinTheta;

                    // Position
                    vertices.Add(x * radius);
                    vertices.Add(y * radius);
                    vertices.Add(z * radius);

                    // Normal (same as position for unit sphere)
                    vertices.Add(x);
                    vertices.Add(y);
                    vertices.Add(z);

                    // Texture coordinates
                    vertices.Add((float)lon / segments);
                    vertices.Add((float)lat / segments);
                }
            }

            // Generate indices
            for (int lat = 0; lat < segments; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    uint first = (uint)(lat * (segments + 1) + lon);
                    uint second = (uint)(first + segments + 1);

                    indices.Add(first);
                    indices.Add(second);
                    indices.Add(first + 1);

                    indices.Add(second);
                    indices.Add(second + 1);
                    indices.Add(first + 1);
                }
            }

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        public static Mesh CreateCylinder(float radius = 1.0f, float height = 2.0f, int segments = 20)
        {
            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();

            float halfHeight = height * 0.5f;

            // Top center vertex
            vertices.AddRange(new float[] { 0, halfHeight, 0, 0, 1, 0, 0.5f, 0.5f });

            // Bottom center vertex
            vertices.AddRange(new float[] { 0, -halfHeight, 0, 0, -1, 0, 0.5f, 0.5f });

            // Side vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2.0f * MathF.PI * i / segments;
                float x = MathF.Cos(angle) * radius;
                float z = MathF.Sin(angle) * radius;

                // Top ring
                vertices.AddRange(new float[] { x, halfHeight, z, 0, 1, 0, (x / radius + 1) * 0.5f, (z / radius + 1) * 0.5f });

                // Bottom ring
                vertices.AddRange(new float[] { x, -halfHeight, z, 0, -1, 0, (x / radius + 1) * 0.5f, (z / radius + 1) * 0.5f });

                // Side vertices (top)
                vertices.AddRange(new float[] { x, halfHeight, z, x / radius, 0, z / radius, (float)i / segments, 1 });

                // Side vertices (bottom)
                vertices.AddRange(new float[] { x, -halfHeight, z, x / radius, 0, z / radius, (float)i / segments, 0 });
            }

            // Generate indices for caps and sides
            for (int i = 0; i < segments; i++)
            {
                // Top cap
                indices.Add(0);
                indices.Add((uint)(2 + i * 4));
                indices.Add((uint)(2 + ((i + 1) % segments) * 4));

                // Bottom cap
                indices.Add(1);
                indices.Add((uint)(3 + ((i + 1) % segments) * 4));
                indices.Add((uint)(3 + i * 4));

                // Side faces
                uint topLeft = (uint)(4 + i * 4);
                uint bottomLeft = (uint)(5 + i * 4);
                uint topRight = (uint)(4 + ((i + 1) % segments) * 4);
                uint bottomRight = (uint)(5 + ((i + 1) % segments) * 4);

                indices.Add(topLeft);
                indices.Add(bottomLeft);
                indices.Add(topRight);

                indices.Add(bottomLeft);
                indices.Add(bottomRight);
                indices.Add(topRight);
            }

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        public static Mesh CreatePyramid(float size = 1.0f)
        {
            float half = size * 0.5f;
            float height = size;

            float[] vertices = {
                // Base vertices
                -half, 0,  half,  0, -1,  0,  0, 0, // 0
                 half, 0,  half,  0, -1,  0,  1, 0, // 1
                 half, 0, -half,  0, -1,  0,  1, 1, // 2
                -half, 0, -half,  0, -1,  0,  0, 1, // 3

                // Top vertex (repeated for each face with different normals)
                0, height, 0,  0.5f, 0.5f, 0.5f,  0.5f, 1, // 4 - front face
                0, height, 0, -0.5f, 0.5f, 0.5f,  0.5f, 1, // 5 - right face  
                0, height, 0, -0.5f, 0.5f, -0.5f, 0.5f, 1, // 6 - back face
                0, height, 0,  0.5f, 0.5f, -0.5f, 0.5f, 1, // 7 - left face
            };

            uint[] indices = {
                // Base
                0, 1, 2,  0, 2, 3,
                // Front face
                0, 4, 1,
                // Right face
                1, 5, 2,
                // Back face
                2, 6, 3,
                // Left face
                3, 7, 0
            };

            return new Mesh(vertices, indices);
        }
    }
}
