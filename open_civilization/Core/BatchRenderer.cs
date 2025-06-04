using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace open_civilization.Core
{
    public class BatchRenderer : IDisposable
    {
        private const int MaxQuads = 10000;
        private const int MaxVertices = MaxQuads * 4;
        private const int MaxIndices = MaxQuads * 6;

        private float[] _vertices;
        private uint[] _indices;
        private int _vertexCount;
        private int _indexCount;

        private int _vao, _vbo, _ebo;
        private Shader _currentShader;
        private Camera _currentCamera;

        public BatchRenderer()
        {
            _vertices = new float[MaxVertices * 9]; // pos(3) + texCoord(2) + color(4)
            _indices = new uint[MaxIndices];

            InitializeBuffers();
            GenerateIndices();
        }

        private void InitializeBuffers()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxVertices * 9 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, MaxIndices * sizeof(uint), IntPtr.Zero, BufferUsageHint.StaticDraw);

            // Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Texture coordinates
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Color
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 9 * sizeof(float), 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
        }

        private void GenerateIndices()
        {
            uint offset = 0;
            for (int i = 0; i < MaxIndices; i += 6)
            {
                _indices[i + 0] = offset + 0;
                _indices[i + 1] = offset + 1;
                _indices[i + 2] = offset + 2;

                _indices[i + 3] = offset + 2;
                _indices[i + 4] = offset + 3;
                _indices[i + 5] = offset + 0;

                offset += 4;
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, MaxIndices * sizeof(uint), _indices);
        }

        public void BeginBatch(Shader shader, Camera camera)
        {
            _currentShader = shader;
            _currentCamera = camera;
            _vertexCount = 0;
            _indexCount = 0;
        }

        public void DrawQuad(Matrix4 model, Vector2 size, Color4 color, int textureId = -1, bool useTexture = false)
        {
            if (_indexCount >= MaxIndices || (_vertexCount + 4) * 9 > _vertices.Length) // Check vertex capacity too
            {
                EndBatch();
                BeginBatch(_currentShader, _currentCamera); // Restart batch
            }

            // Define quad vertices in local space (centered at origin)
            Vector3[] localPositions = new Vector3[]
            {
                new Vector3(-size.X * 0.5f, -size.Y * 0.5f, 0f), // Bottom-left
                new Vector3( size.X * 0.5f, -size.Y * 0.5f, 0f), // Bottom-right
                new Vector3( size.X * 0.5f,  size.Y * 0.5f, 0f), // Top-right
                new Vector3(-size.X * 0.5f,  size.Y * 0.5f, 0f)  // Top-left
            };

            Vector2[] texCoords = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };

            for (int i = 0; i < 4; i++)
            {
                // Transform local vertex position by the model matrix to get world position
                Vector3 worldPos = Vector3.TransformPosition(localPositions[i], model);

                int vertexBufferIndex = _vertexCount * 9;

                // Position
                _vertices[vertexBufferIndex + 0] = worldPos.X;
                _vertices[vertexBufferIndex + 1] = worldPos.Y;
                _vertices[vertexBufferIndex + 2] = worldPos.Z;

                // Texture coordinates
                _vertices[vertexBufferIndex + 3] = texCoords[i].X;
                _vertices[vertexBufferIndex + 4] = texCoords[i].Y;

                // Color
                _vertices[vertexBufferIndex + 5] = color.R;
                _vertices[vertexBufferIndex + 6] = color.G;
                _vertices[vertexBufferIndex + 7] = color.B;
                _vertices[vertexBufferIndex + 8] = color.A;

                _vertexCount++;
            }

            _indexCount += 6;

            // This logic might need adjustment if mixing textured/non-textured quads in one batch frequently
            // For simplicity, the 'useTexture' flag in EndBatch will apply to the whole batch.
            // If individual quads can be textured or not, the batching strategy needs to be more complex
            // or flush per texture/shader state change. The current shader handles this with a uniform.
        }

        public void EndBatch()
        {
            if (_indexCount == 0) return;

            _currentShader.Use();
            _currentShader.SetMatrix4("view", _currentCamera.GetViewMatrix());
            _currentShader.SetMatrix4("projection", _currentCamera.GetProjectionMatrix());
            // Model matrix is Identity because vertices are already in world space
            _currentShader.SetMatrix4("model", Matrix4.Identity);

            // This 'useTexture' uniform will apply to all quads in the current batch.
            // If you need to mix textured and non-textured quads freely, you'd typically:
            // 1. Sort by texture / state.
            // 2. Flush batch when texture or critical state changes.
            // 3. Or use a texture atlas and pass texture ID via vertex attributes.
            // For now, let's assume the shader's `useTexture` can be a simple toggle per batch.
            // If a DrawQuad call implied `useTexture=true` (e.g. valid textureId), 
            // you'd set this accordingly, potentially flushing if the state changes.
            // For this example, let's assume most quads are untextured or all textured similarly.
            // We'll hardcode to false for now as the example is an untextured square.
            _currentShader.SetBool("useTexture", false); // Or determine based on calls within the batch
            // _currentShader.SetInt("texture0", 0); // If using textures, set the sampler unit

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertexCount * 9 * sizeof(float), _vertices);

            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            // Reset counts for the next batch
            _vertexCount = 0;
            _indexCount = 0;
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
        }
    }
}
