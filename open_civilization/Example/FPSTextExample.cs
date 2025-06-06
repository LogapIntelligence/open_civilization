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

        // Game FPS tracking (running average)
        private float _gameFrameTimeAccumulator = 0f;
        private int _gameFrameCount = 0;
        private float _runningAverageFps = 0f;
        private string _gameFpsText = "Game FPS: 0.0 (avg)";

        // UI FPS tracking (updated at capped rate)
        private float _uiTime = 0f;
        private int _uiFrameCount = 0;
        private float _uiFrameTimeAccumulator = 0f;
        private string _uiFpsText = "UI FPS: 0.0";
        private string _uiUpdateRateText = "UI Update Rate: 2.0 Hz";

        // Added: Total average FPS tracking (for the entire application lifetime)
        private float _totalTimeAccumulator = 0f;
        private long _totalFrameCount = 0; // Use long to prevent overflow on long sessions
        private string _totalAverageFpsText = "Total Avg FPS: 0.0";

        public SimpleFpsTextExample()
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Simple FPS Text Display Example - Running Average FPS",
                Flags = ContextFlags.ForwardCompatible
            })
        {
        }

        protected override void Init()
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

            // Set UI update rate to 144 Hz
            SetInterfaceUpdateRate(20);
        }

        protected override void UpdateGame(float deltaTime)
        {
            // Accumulate frame time and count for running average
            _gameFrameTimeAccumulator += deltaTime;
            _gameFrameCount++;

            // Calculate running average FPS
            float avgFrameTime = _gameFrameTimeAccumulator / _gameFrameCount;
            _runningAverageFps = 1.0f / avgFrameTime;

            // Optional: Reset accumulators periodically to prevent overflow and adapt to changes
            // Reset every 1000 frames or 10 seconds
            if (_gameFrameCount > 1000 || _gameFrameTimeAccumulator > 10.0f)
            {
                _gameFrameTimeAccumulator = avgFrameTime; // Keep one average frame worth
                _gameFrameCount = 1;
            }

            // Added: Accumulate total frame time and count for the overall average
            _totalTimeAccumulator += deltaTime;
            _totalFrameCount++;
        }

        // This method is called at the capped rate (144 Hz in this example)
        protected override void UpdateInterface(float deltaTime)
        {
            _uiTime += deltaTime;
            _uiFrameCount++;
            _uiFrameTimeAccumulator += deltaTime;

            // Calculate UI update FPS based on actual update intervals
            float avgUpdateTime = _uiFrameTimeAccumulator / _uiFrameCount;
            float updateFps = 1.0f / avgUpdateTime;

            // Update the display text with the running average calculated in UpdateGame
            _gameFpsText = $"Game FPS: {_runningAverageFps:F1} (avg)";
            _uiFpsText = $"UI Update FPS: {updateFps:F1}";

            // Added: Calculate total average FPS and update its display text
            if (_totalTimeAccumulator > 0)
            {
                float totalAvgFps = _totalFrameCount / _totalTimeAccumulator;
                _totalAverageFpsText = $"Total Avg FPS: {totalAvgFps:F1}";
            }

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
            Vector3 totalAvgColor = new Vector3(1.0f, 0.9f, 0.6f); // Added: Light orange for total average info

            // Game FPS (running average, display updated at UI rate)
            _textRenderer.RenderText(_gameFpsText, centerX - 120, centerY - 75, 1.0f, textColor);

            // UI Update FPS (updated at capped rate)
            _textRenderer.RenderText(_uiFpsText, centerX - 120, centerY - 25, 1.0f, uiColor);

            // Added: Render the total average FPS
            _textRenderer.RenderText(_totalAverageFpsText, centerX - 120, centerY + 25, 1.0f, totalAvgColor);
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