using open_civilization.core;
using open_civilization.Core;
using open_civilization.Interface;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace open_civilization.Example
{
    public class TextRenderingExample : Engine
    {
        private StbTextRenderer _textRenderer;
        private StbTextRenderer _smallTextRenderer;  // For small UI text
        private StbTextRenderer _largeTextRenderer;  // For titles
        private float _time = 0f;

        // 3D scene objects
        private struct CubeObject
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
            public Color4 Color;
        }

        private CubeObject[] _cubes;

        // Text examples with different properties
        private struct TextExample
        {
            public string Text;
            public Vector2 Position;
            public float Scale;
            public Vector3 Color;
            public bool Animated;
            public TextAlignment Alignment;
            public int RendererIndex; // 0 = normal, 1 = small, 2 = large
        }

        private enum TextAlignment
        {
            Left,
            Center,
            Right
        }

        private TextExample[] _textExamples;

        public TextRenderingExample()
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "STB TrueType High Quality Text Rendering Example",
                Flags = ContextFlags.ForwardCompatible
            })
        {
        }

        protected override void Init()
        {
            UpdateFrequency = 165;

            // Initialize multiple text renderers for different sizes
            try
            {
                _textRenderer = new StbTextRenderer(Size.X, Size.Y, fontSize: 24);
                _smallTextRenderer = new StbTextRenderer(Size.X, Size.Y, fontSize: 16);
                _largeTextRenderer = new StbTextRenderer(Size.X, Size.Y, fontSize: 48);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize text renderer: {ex.Message}");
                throw;
            }

            // Setup camera to look at the scene
            _camera.Position = new Vector3(0, 0, 20);
            _camera.Yaw = -90f;
            _camera.Pitch = -20f;

            // Initialize text examples
            _textExamples = new TextExample[]
            {
                new TextExample
                {
                    Text = "STB TrueType Text Rendering",
                    Position = new Vector2(640, 50),
                    Scale = 1.0f,
                    Color = new Vector3(1.0f, 1.0f, 1.0f),
                    Animated = false,
                    Alignment = TextAlignment.Center,
                    RendererIndex = 2  // Use large renderer
                },
                new TextExample
                {
                    Text = "Cross-Platform High Quality Font Rendering!",
                    Position = new Vector2(640, 120),
                    Scale = 1.0f,
                    Color = new Vector3(0.3f, 0.8f, 1.0f),
                    Animated = true,
                    Alignment = TextAlignment.Center,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = "Left Aligned Text",
                    Position = new Vector2(100, 200),
                    Scale = 1.2f,
                    Color = new Vector3(1.0f, 0.5f, 0.3f),
                    Animated = false,
                    Alignment = TextAlignment.Left,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = "Center Aligned Text",
                    Position = new Vector2(640, 240),
                    Scale = 1.0f,
                    Color = new Vector3(0.5f, 1.0f, 0.5f),
                    Animated = false,
                    Alignment = TextAlignment.Center,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = "Right Aligned Text",
                    Position = new Vector2(1180, 280),
                    Scale = 1.0f,
                    Color = new Vector3(0.7f, 0.7f, 1.0f),
                    Animated = false,
                    Alignment = TextAlignment.Right,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = "Small UI Text (16pt base)",
                    Position = new Vector2(100, 340),
                    Scale = 1.0f,
                    Color = new Vector3(0.7f, 0.7f, 0.7f),
                    Animated = false,
                    Alignment = TextAlignment.Left,
                    RendererIndex = 1  // Use small renderer
                },
                new TextExample
                {
                    Text = "Animated Rainbow Text!",
                    Position = new Vector2(100, 380),
                    Scale = 1.0f,
                    Color = new Vector3(1.0f, 0.0f, 0.0f),
                    Animated = true,
                    Alignment = TextAlignment.Left,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                    Position = new Vector2(100, 440),
                    Scale = 1.0f,
                    Color = new Vector3(1.0f, 1.0f, 0.3f),
                    Animated = false,
                    Alignment = TextAlignment.Left,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = "abcdefghijklmnopqrstuvwxyz",
                    Position = new Vector2(100, 480),
                    Scale = 1.0f,
                    Color = new Vector3(1.0f, 1.0f, 0.3f),
                    Animated = false,
                    Alignment = TextAlignment.Left,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = "0123456789 !@#$%^&*()_+-=[]{}|;':\",./<>?",
                    Position = new Vector2(100, 520),
                    Scale = 1.0f,
                    Color = new Vector3(0.8f, 0.8f, 1.0f),
                    Animated = false,
                    Alignment = TextAlignment.Left,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = $"FPS: {0:F1}",
                    Position = new Vector2(10, 10),
                    Scale = 1.0f,
                    Color = new Vector3(0.0f, 1.0f, 0.0f),
                    Animated = false,
                    Alignment = TextAlignment.Left,
                    RendererIndex = 1  // Use small renderer for UI
                },
                new TextExample
                {
                    Text = "Pulsating Text!",
                    Position = new Vector2(640, 600),
                    Scale = 1.5f,
                    Color = new Vector3(1.0f, 0.2f, 0.8f),
                    Animated = true,
                    Alignment = TextAlignment.Center,
                    RendererIndex = 0
                },
                new TextExample
                {
                    Text = "Multi-line text example:\nLine 2 of the text\nLine 3 with proper spacing",
                    Position = new Vector2(900, 400),
                    Scale = 0.8f,
                    Color = new Vector3(0.9f, 0.9f, 0.9f),
                    Animated = false,
                    Alignment = TextAlignment.Left,
                    RendererIndex = 0
                }
            };

            // Set lower UI update rate (30 FPS for UI updates)
            SetInterfaceUpdateRate(1);

            // Initialize some 3D cubes for the scene
            _cubes = new CubeObject[]
            {
                new CubeObject
                {
                    Position = new Vector3(0, 0, 0),
                    Rotation = Vector3.Zero,
                    Scale = new Vector3(2, 0.5f, 2),
                    Color = new Color4(0.3f, 0.3f, 0.8f, 1.0f)
                },
                new CubeObject
                {
                    Position = new Vector3(-3, 1, -2),
                    Rotation = Vector3.Zero,
                    Scale = Vector3.One,
                    Color = new Color4(0.8f, 0.3f, 0.3f, 1.0f)
                },
                new CubeObject
                {
                    Position = new Vector3(3, 1, -2),
                    Rotation = Vector3.Zero,
                    Scale = Vector3.One,
                    Color = new Color4(0.3f, 0.8f, 0.3f, 1.0f)
                },
                new CubeObject
                {
                    Position = new Vector3(0, 2, -4),
                    Rotation = Vector3.Zero,
                    Scale = new Vector3(0.5f, 3, 0.5f),
                    Color = new Color4(0.8f, 0.8f, 0.3f, 1.0f)
                }
            };
        }

        protected override void UpdateGame(float deltaTime)
        {
            _time += deltaTime;

            // Update FPS counter
            if (_textExamples.Length > 10)
            {
                float fps = 1.0f / deltaTime;
                _textExamples[10].Text = $"FPS: {fps:F1}";
            }

            // Camera controls (WASD + Space/Shift for up/down)
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
                _camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
                _camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                _camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
                _camera.ProcessKeyboard(CameraMovement.Right, deltaTime);
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space))
                _camera.ProcessKeyboard(CameraMovement.Up, deltaTime);
            if (_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift))
                _camera.ProcessKeyboard(CameraMovement.Down, deltaTime);

            // Mouse look (hold right mouse button)
            if (_input.IsMouseButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right))
            {
                var mouseDelta = _input.GetMouseDelta();
                _camera.ProcessMouseMovement(mouseDelta.X, -mouseDelta.Y);
            }

            // Mouse wheel zoom
            var scrollDelta = _input.GetMouseWheelDelta();
            if (Math.Abs(scrollDelta) > 0.001f)
            {
                _camera.ProcessMouseScroll(scrollDelta);
            }

            // Animate cubes
            _cubes[1].Rotation = new Vector3(0, _time * 30, 0);
            _cubes[2].Rotation = new Vector3(_time * 45, 0, 0);
            _cubes[3].Rotation = new Vector3(0, 0, _time * 60);
        }

        // Override OnRenderFrame to render 3D scene before UI
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _renderer.BeginFrame(_camera);

            // Render 3D cubes
            foreach (var cube in _cubes)
            {
                Matrix4 model = Matrix4.CreateScale(cube.Scale) *
                               Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(
                                   MathHelper.DegreesToRadians(cube.Rotation.X),
                                   MathHelper.DegreesToRadians(cube.Rotation.Y),
                                   MathHelper.DegreesToRadians(cube.Rotation.Z))) *
                               Matrix4.CreateTranslation(cube.Position);

                _renderer.Draw3DObject("cube", model, cube.Color);
            }

            _renderer.EndFrame();

            // Render UI on top of everything
            GL.Disable(EnableCap.DepthTest);
            RenderInterface((float)e.Time);
            GL.Enable(EnableCap.DepthTest);

            SwapBuffers();
        }

        protected override void RenderInterface(float deltaTime)
        {
            // Select the appropriate renderer
            StbTextRenderer GetRenderer(int index)
            {
                switch (index)
                {
                    case 1: return _smallTextRenderer;
                    case 2: return _largeTextRenderer;
                    default: return _textRenderer;
                }
            }

            // Render all text examples
            for (int i = 0; i < _textExamples.Length; i++)
            {
                var example = _textExamples[i];
                Vector2 position = example.Position;
                float scale = example.Scale;
                Vector3 color = example.Color;
                var renderer = GetRenderer(example.RendererIndex);

                // Apply animations
                if (example.Animated)
                {
                    switch (i)
                    {
                        case 1: // Horizontal wave
                            position.X = example.Position.X + MathF.Sin(_time * 2.0f) * 50;
                            break;

                        case 6: // Rainbow color animation
                            float hue = (_time * 60) % 360;
                            color = HsvToRgb(hue, 1.0f, 1.0f);
                            break;

                        case 11: // Pulsating scale
                            scale = example.Scale + MathF.Sin(_time * 4.0f) * 0.3f;
                            break;
                    }
                }

                // Render based on alignment
                switch (example.Alignment)
                {
                    case TextAlignment.Left:
                        renderer.RenderText(example.Text, position.X, position.Y, scale, color);
                        break;
                    case TextAlignment.Center:
                        renderer.RenderTextCentered(example.Text, position.X, position.Y, scale, color);
                        break;
                    case TextAlignment.Right:
                        renderer.RenderTextRightAligned(example.Text, position.X, position.Y, scale, color);
                        break;
                }
            }

            // Render additional info
            string timeStr = $"Time: {_time:F2}s";
            _smallTextRenderer.RenderText(timeStr, 10, 40, 1.0f, new Vector3(0.5f, 0.5f, 0.5f));

            // Render instructions
            _smallTextRenderer.RenderTextRightAligned("Press ESC to exit", Size.X - 10, Size.Y - 30, 1.0f, new Vector3(0.7f, 0.7f, 0.7f));
            _smallTextRenderer.RenderText("WASD: Move, Space/Shift: Up/Down, Right Mouse: Look", 10, Size.Y - 30, 1.0f, new Vector3(0.7f, 0.7f, 0.7f));

            // Render text quality info
            _smallTextRenderer.RenderText("STB TrueType: Cross-platform, high-quality text rendering", 10, Size.Y - 50, 1.0f, new Vector3(0.6f, 0.6f, 0.8f));
        }

        // Helper function to convert HSV to RGB
        private Vector3 HsvToRgb(float h, float s, float v)
        {
            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            float m = v - c;

            float r, g, b;
            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return new Vector3(r + m, g + m, b + m);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            _textRenderer?.UpdateWindowSize(Size.X, Size.Y);
            _smallTextRenderer?.UpdateWindowSize(Size.X, Size.Y);
            _largeTextRenderer?.UpdateWindowSize(Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            _textRenderer?.Dispose();
            _smallTextRenderer?.Dispose();
            _largeTextRenderer?.Dispose();
            base.OnUnload();
        }
    }
}