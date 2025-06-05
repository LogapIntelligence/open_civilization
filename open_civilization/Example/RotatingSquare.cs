// open_civilization.Example/ExampleGame.cs
using open_civilization.core; // Assuming Engine is in open_civilization.core
using open_civilization.Core; // Assuming GameObject, Renderer etc. are in open_civilization.Core
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Color4 = OpenTK.Mathematics.Color4; // Explicitly use OpenTK.Mathematics.Color4

namespace open_civilization.Example
{
    public class RotatingSquare : Engine
    {
        private ExampleSquare _centerSquare;

        public RotatingSquare() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "OpenTK Game Engine - Rotating Square",
        })
        {

        }

        protected override void InitializeGame()
        {
            _centerSquare = new ExampleSquare
            {
                Position = Vector3.Zero, // Center of the world
                Scale = Vector3.One,
                Color = new Color4(1.0f, 0.0f, 0.0f, 1.0f) // Red color
            };
            AddGameObject(_centerSquare);
        }

        protected override void UpdateGame(float deltaTime)
        {
            if (_centerSquare != null)
            {
                // Rotate around Z-axis, 45 degrees per second
                _centerSquare.Rotation += new Vector3(0, 0, deltaTime * 45.0f);
            }
        }
    }

    public class ExampleSquare : GameObject
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