using open_civilization.core;
using open_civilization.Core;
using open_civilization.Example.Utilities;
using open_civilization.Interface;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using System;

namespace open_civilization.Example
{
    public class OceanWavesFPSExample : Engine
    {
        private WaterPlane2 _waterPlane;
        private StbTextRenderer _textRenderer;

        // FPS tracking variables
        private float _frameTimeAccumulator = 0f;
        private int _frameCount = 0;
        private float _currentFps = 0f;
        private string _fpsText = "FPS: 0.0";

        public OceanWavesFPSExample() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "Ocean Waves with FPS Counter",
            Flags = ContextFlags.ForwardCompatible
        })
        {
        }

        protected override void Init()
        {
            // Initialize text renderer for FPS display
            try
            {
                _textRenderer = new StbTextRenderer(Size.X, Size.Y, fontSize: 24);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize text renderer: {ex.Message}");
                throw;
            }

            // Initialize water plane
            _waterPlane = new WaterPlane2
            {
                Position = Vector3.Zero,
                Scale = Vector3.One * 10f, // Make it bigger
                Color = new Color4(0.0f, 0.5f, 1.0f, 0.8f) // Nice blue color
            };
            AddGameObject(_waterPlane);

            // Setup camera to look at the water
            _camera.Position = new Vector3(0, 5, 15);
            _camera.Yaw = -90f;
            _camera.Pitch = -20f;

            SetInterfaceUpdateRate(1);
        }

        protected override void UpdateGame(float deltaTime)
        {
            // Update FPS tracking
            _frameTimeAccumulator += deltaTime;
            _frameCount++;

            // Calculate FPS every 0.5 seconds for smooth updates
            if (_frameTimeAccumulator >= 0.5f)
            {
                _currentFps = _frameCount / _frameTimeAccumulator;
                _fpsText = $"FPS: {_currentFps:F1}";

                // Reset counters
                _frameTimeAccumulator = 0f;
                _frameCount = 0;
            }

            // Update water animation
            if (_waterPlane != null)
            {
                // Gentle rotation for visual effect
                _waterPlane.Rotation = new Vector3(0f, _waterPlane.Rotation.Y + 10f * deltaTime, 0f);
            }
        }

        protected override void UpdateInterface(float deltaTime)
        {
            Console.WriteLine(_fpsText);
        }
        protected override void RenderInterface(float deltaTime)
        {
            // Render FPS text in the top-left corner
            Vector3 fpsColor = new Vector3(1.0f, 1.0f, 1.0f); // White text
            _textRenderer.RenderText(_fpsText, 0, -20, 1.0f, fpsColor);

        
            // Additional info
            Vector3 infoColor = new Vector3(0.8f, 0.8f, 0.8f); // Light gray
            _textRenderer.RenderText("Ocean Waves Demo", 10, 40, 0.8f, infoColor);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            _textRenderer?.UpdateWindowSize(Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            _textRenderer?.Dispose();
            base.OnUnload();
        }
    }

    public class WaterPlane2 : GameObject
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

        public WaterPlane2()
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