using open_civilization.Example.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Core
{
    public class Mesh3DRenderer : IDisposable
    {
        private Shader _meshShader;
        private Dictionary<string, Mesh> _meshCache;

        public Mesh3DRenderer()
        {
            _meshCache = new Dictionary<string, Mesh>();
            LoadMeshShader();
            LoadCommonMeshes();
        }

        private void LoadMeshShader()
        {
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec3 aNormal;
                layout (location = 2) in vec2 aTexCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                uniform mat3 normalMatrix;

                out vec3 FragPos;
                out vec3 Normal;
                out vec2 TexCoord;

                void main()
                {
                    FragPos = vec3(model * vec4(aPos, 1.0));
                    Normal = normalMatrix * aNormal;
                    TexCoord = aTexCoord;
                    
                    gl_Position = projection * view * vec4(FragPos, 1.0);
                }";

            string fragmentShader = @"
                #version 330 core
                in vec3 FragPos;
                in vec3 Normal;
                in vec2 TexCoord;

                uniform vec4 objectColor;
                uniform vec3 lightPos;
                uniform vec3 lightColor;
                uniform vec3 viewPos;
                uniform bool useTexture;
                uniform sampler2D texture0;

                out vec4 FragColor;

                void main()
                {
                    // Ambient lighting
                    float ambientStrength = 0.3;
                    vec3 ambient = ambientStrength * lightColor;

                    // Diffuse lighting
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float diff = max(dot(norm, lightDir), 0.0);
                    vec3 diffuse = diff * lightColor;

                    // Specular lighting
                    float specularStrength = 0.5;
                    vec3 viewDir = normalize(viewPos - FragPos);
                    vec3 reflectDir = reflect(-lightDir, norm);
                    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
                    vec3 specular = specularStrength * spec * lightColor;

                    vec3 lighting = ambient + diffuse + specular;
                    
                    vec4 finalColor;
                    if (useTexture)
                        finalColor = texture(texture0, TexCoord) * objectColor;
                    else
                        finalColor = objectColor;
                    
                    FragColor = vec4(lighting * finalColor.rgb, finalColor.a);
                }";

            _meshShader = new Shader(vertexShader, fragmentShader);
        }

        private void LoadCommonMeshes()
        {
            _meshCache["cube"] = MeshGenerator.CreateCube();
            _meshCache["sphere"] = MeshGenerator.CreateSphere();
            _meshCache["cylinder"] = MeshGenerator.CreateCylinder();
            _meshCache["pyramid"] = MeshGenerator.CreatePyramid();
        }

        // Draw mesh with default shader
        public void DrawMesh(string meshName, Matrix4 model, Color4 color, Camera camera, Vector3 lightPos)
        {
            if (!_meshCache.ContainsKey(meshName)) return;
            DrawMesh(meshName, model, color, camera, lightPos, null);
        }

        // Draw mesh with custom shader
        public void DrawMesh(string meshName, Matrix4 model, Color4 color, Camera camera, Vector3 lightPos, Shader customShader)
        {
            if (!_meshCache.ContainsKey(meshName)) return;

            var mesh = _meshCache[meshName];
            var shader = customShader ?? _meshShader;

            SetupShaderUniforms(shader, model, color, camera, lightPos);
            mesh.Render();
        }

        // Draw custom mesh with default shader
        public void DrawCustomMesh(Mesh mesh, Matrix4 model, Color4 color, Camera camera, Vector3 lightPos)
        {
            DrawCustomMesh(mesh, model, color, camera, lightPos, null);
        }

        // Draw custom mesh with custom shader
        public void DrawCustomMesh(Mesh mesh, Matrix4 model, Color4 color, Camera camera, Vector3 lightPos, Shader customShader)
        {
            var shader = customShader ?? _meshShader;
            SetupShaderUniforms(shader, model, color, camera, lightPos);
            mesh.Render();
        }

        // Setup standard shader uniforms
        private void SetupShaderUniforms(Shader shader, Matrix4 model, Color4 color, Camera camera, Vector3 lightPos)
        {
            shader.Use();

            // Standard matrices
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            // Calculate normal matrix for proper normal transformation
            Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(model)));
            shader.SetMatrix3("normalMatrix", normalMatrix);

            // Standard lighting uniforms
            shader.SetVector4("objectColor", new Vector4(color.R, color.G, color.B, color.A));
            shader.SetVector3("lightPos", lightPos);
            shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
            shader.SetVector3("viewPos", camera.Position);
            shader.SetBool("useTexture", false);
        }

        // Draw mesh with minimal uniform setup (for very custom shaders)
        public void DrawMeshRaw(string meshName, Shader customShader, Action<Shader> setupUniforms)
        {
            if (!_meshCache.ContainsKey(meshName) || customShader == null) return;

            var mesh = _meshCache[meshName];
            customShader.Use();
            setupUniforms?.Invoke(customShader);
            mesh.Render();
        }

        // Draw custom mesh with minimal uniform setup (for very custom shaders)
        public void DrawCustomMeshRaw(Mesh mesh, Shader customShader, Action<Shader> setupUniforms)
        {
            if (mesh == null || customShader == null) return;

            customShader.Use();
            setupUniforms?.Invoke(customShader);
            mesh.Render();
        }

        public void AddMesh(string name, Mesh mesh)
        {
            _meshCache[name] = mesh;
        }

        public bool HasMesh(string name)
        {
            return _meshCache.ContainsKey(name);
        }

        public Mesh GetMesh(string name)
        {
            return _meshCache.TryGetValue(name, out var mesh) ? mesh : null;
        }

        public void Dispose()
        {
            _meshShader?.Dispose();
            foreach (var mesh in _meshCache.Values)
            {
                mesh.Dispose();
            }
            _meshCache.Clear();
        }
    }
}
