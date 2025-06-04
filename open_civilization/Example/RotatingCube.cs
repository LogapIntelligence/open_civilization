using open_civilization.core;
using open_civilization.Core;
using open_civilization.Example.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace open_civilization.Example
{
    public class RotatingCube : Engine
    {
        private ExampleCube _centerCube;

        public RotatingCube() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "OpenTK Game Engine - Rotating Cube"
        })
        {
        }

        protected override void InitializeGame()
        {
            _centerCube = new ExampleCube
            {
                Position = Vector3.Zero,
                Scale = Vector3.One,
                Color = new Color4(0.0f, 1.0f, 0.0f, 1.0f) // Green color
            };
            AddGameObject(_centerCube);
        }

        protected override void UpdateGame(float deltaTime)
        {
            if (_centerCube != null)
            {
                // Rotate around X and Y axes for a nice tumbling effect
                _centerCube.Rotation += new Vector3(deltaTime * 30.0f, deltaTime * 45.0f, deltaTime * 20.0f);
            }
        }
    }

    public class ExampleCube : GameObject
    {
        private Mesh _cubeMesh;
        public Vector3 Size { get; set; } = Vector3.One;

        public ExampleCube()
        {
            _cubeMesh = MeshGenerator.CreateCube();
        }

        public override void Render(Renderer renderer)
        {
            Matrix4 model = GetModelMatrix();
            renderer.DrawCustomMesh(_cubeMesh, model, Color);
        }
    }
}
