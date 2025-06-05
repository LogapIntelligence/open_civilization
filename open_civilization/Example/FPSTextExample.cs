using open_civilization.core;
using open_civilization.Core;
using open_civilization.Interface;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace open_civilization.Example
{
    public class SimpleFpsTextExample : Engine
    {
        private StbTextRenderer _textRenderer;
        private float _time = 0f;
        private string _fpsText = "FPS: 0.0";

        // Simple 3D scene object for background
        private struct CubeObject
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
            public Color4 Color;
        }

        private CubeObject _cube;

        public SimpleFpsTextExample()
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1280, 720),
                Title = "Simple FPS Text Display Example",
                Flags = ContextFlags.ForwardCompatible
            })
        {
        }

        protected override void InitializeGame()
        {
            // Initialize text renderer
            try
            {
                // Use default system font with 32pt size for clear visibility
                _textRenderer = new StbTextRenderer(Size.X, Size.Y, fontSize: 32);

                // Option to specify a custom font file (uncomment and modify path)
                // string fontPath = "assets/fonts/Roboto-Regular.ttf";
                // _textRenderer = new StbTextRenderer(Size.X, Size.Y, fontPath, fontSize: 32);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize text renderer: {ex.Message}");
                throw;
            }

            // Setup camera to look at the scene
            _camera.Position = new Vector3(0, 2, 5);
            _camera.Yaw = -90f;
            _camera.Pitch = -10f;

            // Initialize a simple rotating cube for visual interest
            _cube = new CubeObject
            {
                Position = new Vector3(0, 0, 0),
                Rotation = Vector3.Zero,
                Scale = new Vector3(2, 2, 2),
                Color = new Color4(0.5f, 0.7f, 0.9f, 1.0f)
            };
        }

        protected override void UpdateGame(float deltaTime)
        {
            _time += deltaTime;

            // Update FPS text
            float fps = 1.0f / deltaTime;
            _fpsText = $"FPS: {fps:F1}";

            // Camera controls (WASD + mouse)
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
                _camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
                _camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                _camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
                _camera.ProcessKeyboard(CameraMovement.Right, deltaTime);

            // Mouse look (hold right mouse button)
            if (_input.IsMouseButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right))
            {
                var mouseDelta = _input.GetMouseDelta();
                _camera.ProcessMouseMovement(mouseDelta.X, -mouseDelta.Y);
            }

            // Animate the cube
            _cube.Rotation = new Vector3(_time * 20, _time * 30, _time * 10);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _renderer.BeginFrame(_camera);

            // Render the rotating cube
            Matrix4 model = Matrix4.CreateScale(_cube.Scale) *
                           Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(
                               MathHelper.DegreesToRadians(_cube.Rotation.X),
                               MathHelper.DegreesToRadians(_cube.Rotation.Y),
                               MathHelper.DegreesToRadians(_cube.Rotation.Z))) *
                           Matrix4.CreateTranslation(_cube.Position);

            _renderer.Draw3DObject("cube", model, _cube.Color);

            _renderer.EndFrame();

            // Render UI on top
            GL.Disable(EnableCap.DepthTest);
            RenderInterface();
            GL.Enable(EnableCap.DepthTest);

            SwapBuffers();
        }

        protected override void RenderInterface()
        {
            // Calculate center position
            float centerX = Size.X / 2.0f;
            float centerY = Size.Y / 2.0f;

            // Render FPS text in the center with a nice color
            Vector3 textColor = new Vector3(1.0f, 1.0f, 1.0f); // White text
            _textRenderer.RenderTextCentered(_fpsText, centerX, centerY, 1.0f, textColor);

            // Optional: Add a subtle glow effect by rendering slightly offset darker text behind
            Vector3 shadowColor = new Vector3(0.0f, 0.0f, 0.0f); // Black shadow
            _textRenderer.RenderTextCentered(_fpsText, centerX + 2, centerY + 2, 1.0f, shadowColor);
            _textRenderer.RenderTextCentered(_fpsText, centerX, centerY, 1.0f, textColor);

            // Render simple instructions at the bottom
            string instructions = "WASD: Move Camera | Right Mouse: Look Around | ESC: Exit";
            _textRenderer.RenderTextCentered(instructions, centerX, Size.Y - 50, 0.6f, new Vector3(0.7f, 0.7f, 0.7f));
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
}