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

        // Game FPS tracking (updated every frame)
        private float _gameTime = 0f;
        private int _gameFrameCount = 0;
        private float _gameFrameTimeAccumulator = 0f;
        private float gameFps = 0f;
        private string _gameFpsText = "Game FPS: 0.0";

        // UI FPS tracking (updated at capped rate)
        private float _uiTime = 0f;
        private int _uiFrameCount = 0;
        private float _uiFrameTimeAccumulator = 0f;
        private string _uiFpsText = "UI FPS: 0.0";
        private string _uiUpdateRateText = "UI Update Rate: 2.0 Hz";

        public SimpleFpsTextExample()
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1280, 720),
                Title = "Simple FPS Text Display Example - Separated UI Updates",
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

            // Set UI update rate to 2 Hz (updates twice per second)
            SetInterfaceUpdateRate(2000);
        }

        protected override void UpdateGame(float deltaTime)
        {
            // Just calculate directly - no need for accumulators
            gameFps = 1.0f / deltaTime;
        }

        // This method is called at the capped rate (2 Hz in this example)
        protected override void UpdateInterface(float deltaTime)
        {
            _uiTime += deltaTime;
            _uiFrameCount++;
            _uiFrameTimeAccumulator += deltaTime;

            // Calculate UI update FPS based on actual update intervals
            float avgUpdateTime = _uiFrameTimeAccumulator / _uiFrameCount;
            float updateFps = 1.0f / avgUpdateTime;
            _uiFpsText = $"UI Update FPS: {updateFps:F1}";
            _gameFpsText = $"Game Update FPS: {gameFps:F1}";
            _uiFrameCount = 0;
            _uiFrameTimeAccumulator = 0f;
        }

        // This method is called every frame but uses data updated at the capped rate
        protected override void RenderInterface(float deltaTime)
        {
            // Calculate positions for centered text
            float centerX = Size.X / 2.0f;
            float centerY = Size.Y / 2.0f;

            // Render text using the data calculated in UpdateInterface
            Vector3 textColor = new Vector3(1.0f, 1.0f, 1.0f); // White text
            Vector3 uiColor = new Vector3(0.8f, 1.0f, 0.8f);   // Light green for UI info

            // Game FPS (updated every frame)
            _textRenderer.RenderText(_gameFpsText, centerX - 100, centerY - 100, 1.0f, textColor);

            // UI Update FPS (updated at capped rate)
            _textRenderer.RenderText(_uiFpsText, centerX - 100, centerY - 50, 1.0f, uiColor);

            // Show the update rate setting
            _textRenderer.RenderText(_uiUpdateRateText, centerX - 100, centerY, 1.0f, uiColor);
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