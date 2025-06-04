using open_civilization.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace open_civilization.core
{
    public class Engine : GameWindow
    {
        public Renderer _renderer;
        public Camera _camera;
        public InputManager _input;
        private List<IGameObject> _gameObjects;
        private bool _isRunning;

        public Engine(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _gameObjects = new List<IGameObject>();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f); // Dark gray background

            _renderer = new Renderer();
            _camera = new Camera(new Vector3(0, 0, 5), Size.X / (float)Size.Y);
            _input = new InputManager(this);

            _isRunning = true;

            // Initialize game-specific content
            InitializeGame();
        }

        protected virtual void InitializeGame()
        {
            // Override in derived classes for game-specific initialization
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _renderer.BeginFrame(_camera);

            foreach (var gameObject in _gameObjects)
            {
                gameObject.Render(_renderer);
            }

            _renderer.EndFrame();

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!_isRunning) return;

            _input.Update();
            _camera.Update((float)e.Time);

            foreach (var gameObject in _gameObjects)
            {
                gameObject.Update((float)e.Time);
            }

            UpdateGame((float)e.Time);
        }

        protected virtual void UpdateGame(float deltaTime)
        {
            // Override in derived classes for game-specific updates
        }

        public void AddGameObject(IGameObject gameObject)
        {
            _gameObjects.Add(gameObject);
        }

        public void RemoveGameObject(IGameObject gameObject)
        {
            _gameObjects.Remove(gameObject);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera?.UpdateProjection(Size.X / (float)Size.Y);
        }

        protected override void OnUnload()
        {
            _renderer?.Dispose();
            base.OnUnload();
        }
    }
}
