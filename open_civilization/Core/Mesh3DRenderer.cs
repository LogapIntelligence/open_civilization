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

        public void DrawMesh(string meshName, Matrix4 model, Color4 color, Camera camera, Vector3 lightPos)
        {
            if (!_meshCache.ContainsKey(meshName)) return;

            var mesh = _meshCache[meshName];

            _meshShader.Use();
            _meshShader.SetMatrix4("model", model);
            _meshShader.SetMatrix4("view", camera.GetViewMatrix());
            _meshShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            // Calculate normal matrix for proper normal transformation
            Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(model)));
            _meshShader.SetMatrix3("normalMatrix", normalMatrix);

            _meshShader.SetVector4("objectColor", new Vector4(color.R, color.G, color.B, color.A));
            _meshShader.SetVector3("lightPos", lightPos);
            _meshShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
            _meshShader.SetVector3("viewPos", camera.Position);
            _meshShader.SetBool("useTexture", false);

            mesh.Render();
        }

        public void DrawCustomMesh(Mesh mesh, Matrix4 model, Color4 color, Camera camera, Vector3 lightPos)
        {
            _meshShader.Use();
            _meshShader.SetMatrix4("model", model);
            _meshShader.SetMatrix4("view", camera.GetViewMatrix());
            _meshShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(model)));
            _meshShader.SetMatrix3("normalMatrix", normalMatrix);

            _meshShader.SetVector4("objectColor", new Vector4(color.R, color.G, color.B, color.A));
            _meshShader.SetVector3("lightPos", lightPos);
            _meshShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
            _meshShader.SetVector3("viewPos", camera.Position);
            _meshShader.SetBool("useTexture", false);

            mesh.Render();
        }

        public void AddMesh(string name, Mesh mesh)
        {
            _meshCache[name] = mesh;
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
