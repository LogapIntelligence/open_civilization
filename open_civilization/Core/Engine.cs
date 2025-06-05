using open_civilization.Components;
using open_civilization.Core;
using open_civilization.Interface;
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
        public Physics2DSystem _physics2D = new Physics2DSystem();
        private List<IGameObject> _gameObjects;
        private bool _isRunning;

        // UI Update system
        private double _uiUpdateInterval = 1.0 / 20.0; // 20 FPS for UI updates
        private double _lastUIUpdate = 0.0;
        private double _totalTime = 0.0;

        // UI Projection Matrix - made public for UI rendering
        public Matrix4 UIProjectionMatrix { get; private set; }

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
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            _renderer = new Renderer();
            _camera = new Camera(new Vector3(0, 0, 5), Size.X / (float)Size.Y);
            _input = new InputManager(this);

            // Initialize UI projection matrix for 2D rendering
            UpdateUIProjectionMatrix();

            _isRunning = true;
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

            // Render UI on top of everything
            // Disable depth testing for UI
            GL.Disable(EnableCap.DepthTest);
            RenderInterface();
            GL.Enable(EnableCap.DepthTest);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (!_isRunning) return;

            _totalTime += e.Time;

            _input.Update();
            _camera.Update((float)e.Time);
            _physics2D.Update((float)e.Time);

            foreach (var gameObject in _gameObjects)
            {
                gameObject.Update((float)e.Time);
            }

            UpdateGame((float)e.Time);

            // Update UI at lower frequency
            if (_totalTime - _lastUIUpdate >= _uiUpdateInterval)
            {
                UpdateInterface((float)(_totalTime - _lastUIUpdate));
                _lastUIUpdate = _totalTime;
            }
        }

        protected virtual void UpdateGame(float deltaTime)
        {
            // Override in derived classes for game-specific updates
        }

        // UI update method - called at lower frequency (20 FPS by default)
        protected virtual void UpdateInterface(float deltaTime)
        {
            // Override in derived classes for UI-specific updates
            // This is where you update UI data that doesn't need 60fps updates
        }

        // UI render method - called every frame but uses data updated at lower frequency
        protected virtual void RenderInterface()
        {
            // Override in derived classes for UI rendering
            // This is where you call _textRenderer.RenderText()
        }

        // Set UI update frequency
        public void SetInterfaceUpdateRate(double updatesPerSecond)
        {
            _uiUpdateInterval = 1.0 / updatesPerSecond;
        }

        public void AddGameObject(IGameObject gameObject)
        {
            _gameObjects.Add(gameObject);

            if (gameObject is GameObject go)
            {
                var physics = go.GetComponent<Physics2DComponent>();
                if (physics != null)
                {
                    _physics2D.AddComponent(physics);
                }
            }
        }

        public void RemoveGameObject(IGameObject gameObject)
        {
            _gameObjects.Remove(gameObject);

            if (gameObject is GameObject go)
            {
                var physics = go.GetComponent<Physics2DComponent>();
                if (physics != null)
                {
                    _physics2D.RemoveComponent(physics);
                }
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera?.UpdateProjection(Size.X / (float)Size.Y);
            UpdateUIProjectionMatrix();
        }

        private void UpdateUIProjectionMatrix()
        {
            // Create orthographic projection for UI (0,0 is top-left)
            UIProjectionMatrix = Matrix4.CreateOrthographicOffCenter(
                0, Size.X,     // left, right
                Size.Y, 0,     // bottom, top (inverted for top-left origin)
                -1.0f, 1.0f    // near, far
            );
        }

        protected override void OnUnload()
        {
            _renderer?.Dispose();
            base.OnUnload();
        }
    }
}