using open_civilization.core;
using open_civilization.Core;
using open_civilization.Example.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace open_civilization.Example
{
    public class OceanWaves : Engine
    {
        private WaterPlane _waterPlane;
    
        public OceanWaves() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "OpenTK Game Engine - Rotating Cube"
        })
        {
        }

        protected override void InitializeGame()
        {
            _waterPlane = new WaterPlane
            {
                Position = Vector3.Zero,
                Scale = Vector3.One,
                Color = new Color4(0.0f, 1.0f, 0.0f, 0.5f) // Green color,
            };
            AddGameObject(_waterPlane);
        }

        protected override void UpdateGame(float deltaTime)
        {
            if (_waterPlane != null)
            {
                // Rotate around X and Y axes for a nice tumbling effect
                _waterPlane.Rotation = new Vector3(5.0f, 9.0f, 3.0f);
            }
        }
    }

    public class WaterPlane : GameObject
    {
        private Mesh _waterMesh;
        private Shader _waterShader;
        private float _time = 0;

        public float WaveSpeed { get; set; } = 1f;
        public float WaveAmplitude { get; set; } = 0.5f;
        public float WaveFrequency { get; set; } = 10.0f;
        public Vector3 DeepColor { get; set; } = new Vector3(0.0f, 0.2f, 0.4f);
        public Vector3 ShallowColor { get; set; } = new Vector3(0.0f, 0.5f, 0.7f);
        public float WaterAlpha { get; set; } = 1f;
        public float FresnelPower { get; set; } = 4.0f;
        public float SpecularStrength { get; set; } = 2.0f;
        public float Shininess { get; set; } = 128.0f;

        public WaterPlane()
        {
            // Create a high-resolution plane for water surface
            _waterMesh = MeshGenerator.CreatePlane(100, 100); // You'll need to implement this
            _waterShader = ShaderExamples.CreateWaterShader();

            // Default water color
            Color = new Color4(0.1f, 0.4f, 0.7f, 0.8f);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _time += deltaTime;
        }

        public override void Render(Renderer renderer)
        {
            Matrix4 model = GetModelMatrix();

            renderer.DrawCustomMeshRaw(_waterMesh, _waterShader, (shader) =>
            {
                // Standard matrices
                shader.SetMatrix4("model", model);
                shader.SetMatrix4("view", renderer.Camera.GetViewMatrix());
                shader.SetMatrix4("projection", renderer.Camera.GetProjectionMatrix());

                // Time and wave parameters
                shader.SetFloat("time", _time);
                shader.SetFloat("waveSpeed", WaveSpeed);
                shader.SetFloat("waveAmplitude", WaveAmplitude);
                shader.SetFloat("waveFrequency", WaveFrequency);

                // Water appearance parameters
                shader.SetVector3("waterDeepColor", DeepColor);
                shader.SetVector3("waterShallowColor", ShallowColor);
                shader.SetFloat("waterAlpha", WaterAlpha);
                shader.SetFloat("fresnelPower", FresnelPower);
                shader.SetFloat("specularStrength", SpecularStrength);
                shader.SetFloat("shininess", Shininess);

                // Lighting
                shader.SetVector4("objectColor", new Vector4(Color.R, Color.G, Color.B, Color.A));
                shader.SetVector3("lightPos", renderer.LightPosition);
                shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
                shader.SetVector3("cameraPos", renderer.Camera.Position);
            });
        }
    }
}
