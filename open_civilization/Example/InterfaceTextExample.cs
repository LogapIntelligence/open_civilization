// open_civilization.Example/ExampleGame.cs
using open_civilization.core; // Assuming Engine is in open_civilization.core
using open_civilization.Core; // Assuming GameObject, Renderer etc. are in open_civilization.Core
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Color4 = OpenTK.Mathematics.Color4; // Explicitly use OpenTK.Mathematics.Color4

namespace open_civilization.Example
{
    public class InterfaceTextExample : Engine
    {
        private ExampleSquare _centerSquare;

        // UI data - updated at lower frequency
        private string _fpsText = "FPS: --";
        private string _positionText = "Position: (0, 0, 0)";
        private string _rotationText = "Rotation: 0°";
        private int _frameCount = 0;
        private double _fpsTimer = 0.0;

        public InterfaceTextExample() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "OpenTK Game Engine - Rotating Square with UI",
        })
        {
        }

        protected override void InitializeGame()
        {
            // Set UI update rate to 10 FPS (optional - default is 20 FPS)
            SetInterfaceUpdateRate(10.0);

            _centerSquare = new ExampleSquare
            {
                Position = Vector3.Zero,
                Scale = Vector3.One,
                Color = new Color4(1.0f, 0.0f, 0.0f, 1.0f)
            };
            AddGameObject(_centerSquare);
        }

        protected override void UpdateGame(float deltaTime)
        {
            if (_centerSquare != null)
            {
                // Rotate around Z-axis, 45 degrees per second
                _centerSquare.Rotation += new Vector3(0, 0, deltaTime * 45.0f);

                // Keep rotation in 0-360 range for display
                if (_centerSquare.Rotation.Z >= 360.0f)
                    _centerSquare.Rotation = new Vector3(_centerSquare.Rotation.X, _centerSquare.Rotation.Y, _centerSquare.Rotation.Z - 360.0f);
            }

            // Count frames for FPS calculation
            _frameCount++;
            _fpsTimer += deltaTime;
        }

        // This updates at lower frequency (10 FPS in this example)
        protected override void UpdateInterface(float deltaTime)
        {
            // Calculate FPS
            if (_fpsTimer >= 1.0)
            {
                double fps = _frameCount / _fpsTimer;
                _fpsText = $"FPS: {fps:F1}";
                _frameCount = 0;
                _fpsTimer = 0.0;
            }

            // Update position and rotation text
            if (_centerSquare != null)
            {
                _positionText = $"Position: ({_centerSquare.Position.X:F1}, {_centerSquare.Position.Y:F1}, {_centerSquare.Position.Z:F1})";
                _rotationText = $"Rotation: {_centerSquare.Rotation.Z:F1}°";
            }
        }

        // This renders every frame but uses data updated at lower frequency
        protected override void RenderInterface()
        {
           
        }
    }

    public class StaticSquare : GameObject
    {
        public override void Render(Renderer renderer)
        {
            // Calculate the model matrix based on the GameObject's properties
            Matrix4 scaleMatrix = Matrix4.CreateScale(Scale);
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
                                     Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
                                     Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));
            Matrix4 translationMatrix = Matrix4.CreateTranslation(Position);
            Matrix4 model = scaleMatrix * rotationMatrix * translationMatrix;

            // Call the renderer's DrawQuad method with the model matrix
            renderer.DrawQuad(model, new Vector2(1, 1), Color);
        }
    }
}