using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace open_civilization.Core
{
    public class InputManager
    {
        private GameWindow _window;
        private KeyboardState _currentKeyboard;
        private KeyboardState _previousKeyboard;
        private MouseState _currentMouse;
        private MouseState _previousMouse;

        public InputManager(GameWindow window)
        {
            _window = window;
        }

        public void Update()
        {
            _previousKeyboard = _currentKeyboard;
            _previousMouse = _currentMouse;

            _currentKeyboard = _window.KeyboardState;
            _currentMouse = _window.MouseState;
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboard.IsKeyDown(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
        }

        public bool IsKeyReleased(Keys key)
        {
            return !_currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyDown(key);
        }

        public bool IsMouseButtonDown(MouseButton button)
        {
            return _currentMouse.IsButtonDown(button);
        }

        public bool IsMouseButtonPressed(MouseButton button)
        {
            return _currentMouse.IsButtonDown(button) && !_previousMouse.IsButtonDown(button);
        }

        public Vector2 GetMousePosition()
        {
            return _currentMouse.Position;
        }

        public Vector2 GetMouseDelta()
        {
            return _currentMouse.Position - _previousMouse.Position;
        }
    }
}
