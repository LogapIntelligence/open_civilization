using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Core
{
    public class Renderer : IDisposable
    {
        private ShaderManager _shaderManager;
        private BatchRenderer _batchRenderer;
        private Mesh3DRenderer _mesh3DRenderer;
        private Camera _currentCamera;
        private Vector3 _lightPosition = new Vector3(5, 5, 5);

        // Public properties for advanced usage
        public ShaderManager ShaderManager => _shaderManager;
        public Mesh3DRenderer Mesh3D => _mesh3DRenderer;
        public BatchRenderer Batch => _batchRenderer;
        public Camera Camera => _currentCamera;
        public Vector3 LightPosition => _lightPosition;

        public Renderer()
        {
            _shaderManager = new ShaderManager();
            _batchRenderer = new BatchRenderer();
            _mesh3DRenderer = new Mesh3DRenderer();
            LoadDefaultShaders();
        }

        private void LoadDefaultShaders()
        {
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec2 aTexCoord;
                layout (location = 2) in vec4 aColor;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                out vec2 TexCoord;
                out vec4 Color;

                void main()
                {
                    gl_Position = projection * view * model * vec4(aPos, 1.0);
                    TexCoord = aTexCoord;
                    Color = aColor;
                }";

            string fragmentShader = @"
                #version 330 core
                in vec2 TexCoord;
                in vec4 Color;

                uniform sampler2D texture0;
                uniform bool useTexture;

                out vec4 FragColor;

                void main()
                {
                    if (useTexture)
                        FragColor = texture(texture0, TexCoord) * Color;
                    else
                        FragColor = Color;
                }";

            _shaderManager.LoadShader("default", vertexShader, fragmentShader);
        }

        public void BeginFrame(Camera camera)
        {
            _currentCamera = camera;
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), camera);
        }

        // ==================== 2D RENDERING METHODS ====================
        public void DrawQuad(Matrix4 model, Vector2 size, Color4 color, int textureId = -1)
        {
            _batchRenderer.DrawQuad(model, size, color, textureId);
        }

        // ==================== 3D RENDERING METHODS ====================

        // Draw 3D object with default shader
        public void Draw3DObject(string meshType, Matrix4 model, Color4 color)
        {
            _batchRenderer.EndBatch();
            _mesh3DRenderer.DrawMesh(meshType, model, color, _currentCamera, _lightPosition);
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
        }

        // Draw 3D object with custom shader
        public void Draw3DObject(string meshType, Matrix4 model, Color4 color, Shader customShader)
        {
            _batchRenderer.EndBatch();
            _mesh3DRenderer.DrawMesh(meshType, model, color, _currentCamera, _lightPosition, customShader);
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
        }

        // Draw 3D object with custom shader and setup
        public void Draw3DObjectRaw(string meshType, Shader customShader, Action<Shader> setupUniforms)
        {
            _batchRenderer.EndBatch();
            _mesh3DRenderer.DrawMeshRaw(meshType, customShader, setupUniforms);
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
        }

        // Draw custom mesh with default shader
        public void DrawCustomMesh(Mesh mesh, Matrix4 model, Color4 color)
        {
            _batchRenderer.EndBatch();
            _mesh3DRenderer.DrawCustomMesh(mesh, model, color, _currentCamera, _lightPosition);
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
        }

        // Draw custom mesh with custom shader
        public void DrawCustomMesh(Mesh mesh, Matrix4 model, Color4 color, Shader customShader)
        {
            _batchRenderer.EndBatch();
            _mesh3DRenderer.DrawCustomMesh(mesh, model, color, _currentCamera, _lightPosition, customShader);
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
        }

        // Draw custom mesh with custom shader and setup
        public void DrawCustomMeshRaw(Mesh mesh, Shader customShader, Action<Shader> setupUniforms)
        {
            _batchRenderer.EndBatch();
            _mesh3DRenderer.DrawCustomMeshRaw(mesh, customShader, setupUniforms);
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
        }

        // ==================== SHADER MANAGEMENT ====================

        // Load a custom shader
        public void LoadShader(string name, string vertexSource, string fragmentSource)
        {
            _shaderManager.LoadShader(name, vertexSource, fragmentSource);
        }

        // Get a loaded shader
        public Shader GetShader(string name)
        {
            return _shaderManager.GetShader(name);
        }

        // ==================== MESH MANAGEMENT ====================

        public void AddCustomMesh(string name, Mesh mesh)
        {
            _mesh3DRenderer.AddMesh(name, mesh);
        }

        public void DrawCustomMesh(Mesh mesh, Matrix4 model, Color4 color, int textureId)
        {
            _batchRenderer.EndBatch();
            _mesh3DRenderer.DrawCustomMesh(mesh, model, color, _currentCamera, _lightPosition, textureId);
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
        }
        public bool HasMesh(string name)
        {
            return _mesh3DRenderer.HasMesh(name);
        }

        public Mesh GetMesh(string name)
        {
            return _mesh3DRenderer.GetMesh(name);
        }

        // ==================== LIGHTING ====================

        public void SetLightPosition(Vector3 position)
        {
            _lightPosition = position;
        }

        // ==================== BATCH CONTROL ====================

        // Force end current batch (useful when switching render states)
        public void FlushBatch()
        {
            _batchRenderer.EndBatch();
            _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
        }

        // Switch to a different 2D shader
        public void SetBatchShader(string shaderName)
        {
            _batchRenderer.EndBatch();
            var shader = _shaderManager.GetShader(shaderName);
            if (shader != null)
            {
                _batchRenderer.BeginBatch(shader, _currentCamera);
            }
            else
            {
                _batchRenderer.BeginBatch(_shaderManager.GetShader("default"), _currentCamera);
            }
        }

        // ==================== FRAME MANAGEMENT ====================

        public void EndFrame()
        {
            _batchRenderer.EndBatch();
        }

        public void Dispose()
        {
            _shaderManager?.Dispose();
            _batchRenderer?.Dispose();
            _mesh3DRenderer?.Dispose();
        }
    }
}
