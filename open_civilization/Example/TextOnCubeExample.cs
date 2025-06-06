using open_civilization.core;
using open_civilization.Core;
using open_civilization.Interface;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace open_civilization.Example
{
    public class TextOnCubeExample : Engine
    {
        private StbTextRenderer _textRenderer;
        private NumberedCube _numberedCube;
        public TextOnCubeExample(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void Init()
        {

            // Initialize text renderer
            _textRenderer = new StbTextRenderer(Size.X, Size.Y, fontSize: 256);

            // Create numbered cube
            _numberedCube = new NumberedCube(_textRenderer);
            _numberedCube.Position = new Vector3(0, 0, 0);
            AddGameObject(_numberedCube);

            // Set camera position
            _camera.Position = new Vector3(3, 3, 5);
            _camera.Yaw = -135f;
            _camera.Pitch = -25f;

            CursorState = CursorState.Hidden;
        }

        protected override void UpdateGame(float deltaTime)
        {

            _numberedCube.Rotation = new Vector3(
            _numberedCube.Rotation.X + deltaTime * 22f,  // 45 degrees per second
            _numberedCube.Rotation.Y + deltaTime * 15f,  // 30 degrees per second
            _numberedCube.Rotation.Z);


            // Camera controls
            if (_input.IsKeyDown(Keys.W))
                _camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
            if (_input.IsKeyDown(Keys.S))
                _camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
            if (_input.IsKeyDown(Keys.A))
                _camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
            if (_input.IsKeyDown(Keys.D))
                _camera.ProcessKeyboard(CameraMovement.Right, deltaTime);
            if (_input.IsKeyDown(Keys.Space))
                _camera.ProcessKeyboard(CameraMovement.Up, deltaTime);
            if (_input.IsKeyDown(Keys.LeftShift))
                _camera.ProcessKeyboard(CameraMovement.Down, deltaTime);

            // Mouse look
            if (IsFocused)
            {
                var mouseDelta = _input.GetMouseDelta();
                _camera.ProcessMouseMovement(mouseDelta.X, -mouseDelta.Y);
            }
           

            // Toggle cursor
            if (_input.IsKeyPressed(Keys.Escape))
            {
                CursorState = CursorState == CursorState.Grabbed ?
                             CursorState.Normal : CursorState.Grabbed;
            }
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

    public class NumberedCube : GameObject
    {
        private Mesh _cubeMesh;
        private int _texture;
        private Dictionary<string, System.Drawing.RectangleF> _uvMap;

        public NumberedCube(StbTextRenderer textRenderer)
        {
            // 1. Use the new TextAtlasRenderer
            var atlasRenderer = new TextAtlasRenderer(textRenderer);
            var faceTexts = new[] { "1", "2", "3", "4", "5", "6" };

            // 2. Create the atlas texture and get the UV map for each face
            (_texture, _uvMap) = atlasRenderer.CreateAtlas(
                faceTexts,
                Color4.White,
                new Color4(0.2f, 0.3f, 0.8f, 1.0f)
            );

            // 3. Create the cube mesh with UVs mapped to the atlas
            _cubeMesh = CreateCubeWithAtlasUVs(_uvMap);
        }

        private Mesh CreateCubeWithAtlasUVs(Dictionary<string, System.Drawing.RectangleF> uvMap)
        {
            float half = 0.5f;

            // Base vertices for a cube. We will modify the UVs.
            float[] vertices = {
            // Position           Normal              Original UV
            // Front face (maps to "1")
            -half, -half,  half,  0,  0,  1,  0, 1,
             half, -half,  half,  0,  0,  1,  1, 1,
             half,  half,  half,  0,  0,  1,  1, 0,
            -half,  half,  half,  0,  0,  1,  0, 0,

            // Back face (maps to "2")
            -half, -half, -half,  0,  0, -1,  1, 1,
            -half,  half, -half,  0,  0, -1,  1, 0,
             half,  half, -half,  0,  0, -1,  0, 0,
             half, -half, -half,  0,  0, -1,  0, 1,

            // Top face (maps to "3")
            -half,  half, -half,  0,  1,  0,  0, 1,
            -half,  half,  half,  0,  1,  0,  0, 0,
             half,  half,  half,  0,  1,  0,  1, 0,
             half,  half, -half,  0,  1,  0,  1, 1,

            // Bottom face (maps to "4")
            -half, -half, -half,  0, -1,  0,  1, 1,
             half, -half, -half,  0, -1,  0,  0, 1,
             half, -half,  half,  0, -1,  0,  0, 0,
            -half, -half,  half,  0, -1,  0,  1, 0,

            // Right face (maps to "5")
             half, -half, -half,  1,  0,  0,  1, 1,
             half,  half, -half,  1,  0,  0,  1, 0,
             half,  half,  half,  1,  0,  0,  0, 0,
             half, -half,  half,  1,  0,  0,  0, 1,

            // Left face (maps to "6")
            -half, -half, -half, -1,  0,  0,  0, 1,
            -half, -half,  half, -1,  0,  0,  1, 1,
            -half,  half,  half, -1,  0,  0,  1, 0,
            -half,  half, -half, -1,  0,  0,  0, 0
        };

            uint[] indices = {
            0,  1,  2,   0,  2,  3,   // Front
            4,  5,  6,   4,  6,  7,   // Back
            8,  9, 10,   8, 10, 11,  // Top
           12, 13, 14,  12, 14, 15,  // Bottom
           16, 17, 18,  16, 18, 19,  // Right
           20, 21, 22,  20, 22, 23   // Left
        };

            // Map face index to text
            string[] faceNumToText = { "1", "2", "3", "4", "5", "6" };

            // Remap UV coordinates for each face
            for (int face = 0; face < 6; face++)
            {
                var uvRect = uvMap[faceNumToText[face]];
                for (int vert = 0; vert < 4; vert++)
                {
                    int vertIndex = (face * 4 + vert) * 8; // 8 floats per vertex
                    float originalU = vertices[vertIndex + 6];
                    float originalV = vertices[vertIndex + 7];

                    // Map original (0,1) UVs to the atlas rectangle
                    vertices[vertIndex + 6] = uvRect.X + originalU * uvRect.Width;
                    vertices[vertIndex + 7] = uvRect.Y + originalV * uvRect.Height;
                }
            }

            return new Mesh(vertices, indices);
        }

        public override void Update(float deltaTime)
        {

        }

        public override void Render(Renderer renderer)
        {
            Matrix4 model = GetModelMatrix();

            renderer.DrawCustomMesh(_cubeMesh, model, Color4.White, _texture);

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        }

    }
}
