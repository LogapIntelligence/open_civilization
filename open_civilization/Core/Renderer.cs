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
        private Camera _currentCamera;

        public Renderer()
        {
            _shaderManager = new ShaderManager();
            _batchRenderer = new BatchRenderer();

            // Load default shaders
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

        // Modified DrawQuad to accept a model matrix
        public void DrawQuad(Matrix4 model, Vector2 size, Color4 color, int textureId = -1)
        {
            _batchRenderer.DrawQuad(model, size, color, textureId);
        }

        // Keep DrawSprite if needed, or adapt it similarly if it needs individual models
        public void DrawSprite(Matrix4 model, Vector2 size, int textureId, Color4 tint = default)
        {
            if (tint == default) tint = Color4.White; // Or (tint.R == 0 && ... tint.A ==0) for explicit default check
            _batchRenderer.DrawQuad(model, size, tint, textureId, true); // Assuming a new overload for textured quads
        }
        // Overload original DrawSprite if it was meant for screen-space or non-model transformed sprites
        public void DrawSprite(Vector3 position, Vector2 size, int textureId, Color4 tint = default)
        {
            if (tint == default) tint = Color4.White;
            Matrix4 model = Matrix4.CreateTranslation(position); // Simple model if only position is needed
            _batchRenderer.DrawQuad(model, size, tint, textureId, true);
        }

        public void EndFrame()
        {
            _batchRenderer.EndBatch();
        }

        public void Dispose()
        {
            _shaderManager?.Dispose();
            _batchRenderer?.Dispose();
        }
    }
}
