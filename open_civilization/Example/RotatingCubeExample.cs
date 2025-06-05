using open_civilization.core;
using open_civilization.Core;
using open_civilization.Example.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace open_civilization.Example
{
    public class RotatingCubeExample : Engine
    {
        private ExampleCube _centerCube;

        public RotatingCubeExample() : base(GameWindowSettings.Default, new NativeWindowSettings()
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
            };
            AddGameObject(_centerCube);
        }

        protected override void UpdateGame(float deltaTime)
        {
            _centerCube.Rotation = new Vector3(
                _centerCube.Rotation.X + deltaTime * 22f,  // 45 degrees per second
                _centerCube.Rotation.Y + deltaTime * 15f,  // 30 degrees per second
                _centerCube.Rotation.Z
            );
        }
    }

    public class ExampleCube : GameObject
    {
        private Mesh _cubeMesh;
        private Shader _cubeShader;
        public Vector3 Size { get; set; } = Vector3.One;

        public ExampleCube()
        {
            _cubeMesh = MeshGenerator.CreateCube();
            _cubeShader = ShaderExamples.CreateColorShader(new Color4(1.0f, 0.0f, 0.0f, 1.0f));
        }

        public override void Render(Renderer renderer)
        {
            Matrix4 model = GetModelMatrix();

            renderer.DrawCustomMesh(_cubeMesh, model, new Color4(0.0f, 1.0f, 0.0f, 1.0f), _cubeShader);
        }
    }
}
