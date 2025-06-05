using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
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

        private float _currentMouseWheel;
        private float _previousMouseWheel;

        // Track if this is the first update
        private bool _firstUpdate = true;

        public InputManager(GameWindow window)
        {
            _window = window;
        }

        public void Update()
        {
            // Store previous states
            _previousKeyboard = _currentKeyboard;
            _previousMouse = _currentMouse;
            _previousMouseWheel = _currentMouseWheel;

            // Get current states
            _currentKeyboard = _window.KeyboardState;
            _currentMouse = _window.MouseState;
            _currentMouseWheel = _currentMouse.ScrollDelta.Y;

            // On first update, set previous states to current to avoid null issues
            if (_firstUpdate)
            {
                _previousKeyboard = _currentKeyboard;
                _previousMouse = _currentMouse;
                _previousMouseWheel = _currentMouseWheel;
                _firstUpdate = false;
            }
        }

        // Keyboard input methods
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

        public bool IsAnyKeyDown()
        {
            return _currentKeyboard.IsAnyKeyDown;
        }

        // Mouse button input methods
        public bool IsMouseButtonDown(MouseButton button)
        {
            return _currentMouse.IsButtonDown(button);
        }

        public bool IsMouseButtonPressed(MouseButton button)
        {
            return _currentMouse.IsButtonDown(button) && !_previousMouse.IsButtonDown(button);
        }

        public bool IsMouseButtonReleased(MouseButton button)
        {
            return !_currentMouse.IsButtonDown(button) && _previousMouse.IsButtonDown(button);
        }

        public bool IsAnyMouseButtonDown()
        {
            return _currentMouse.IsAnyButtonDown;
        }

        // Mouse position methods
        public Vector2 GetMousePosition()
        {
            return _currentMouse.Position;
        }

        public Vector2 GetMouseDelta()
        {
            return _currentMouse.Delta;
        }

        public Vector2 GetMousePositionDelta()
        {
            return _currentMouse.Position - _previousMouse.Position;
        }

        // Mouse wheel methods
        public float GetMouseWheel()
        {
            return _currentMouseWheel;
        }

        public float GetMouseWheelDelta()
        {
            return _currentMouse.ScrollDelta.Y;
        }

        public Vector2 GetMouseScrollDelta()
        {
            return _currentMouse.ScrollDelta;
        }

        // Utility methods
        public void SetMousePosition(Vector2 position)
        {
            _window.MousePosition = position;
        }

        public void SetMousePositionCenter()
        {
            _window.MousePosition = new Vector2(_window.Size.X / 2f, _window.Size.Y / 2f);
        }

        public bool IsCursorVisible
        {
            get => _window.CursorState == CursorState.Normal;
            set => _window.CursorState = value ? CursorState.Normal : CursorState.Hidden;
        }

        public bool IsCursorGrabbed
        {
            get => _window.CursorState == CursorState.Grabbed;
            set => _window.CursorState = value ? CursorState.Grabbed : CursorState.Normal;
        }

        // Check for specific key combinations
        public bool IsKeyComboDown(Keys modifier, Keys key)
        {
            return IsKeyDown(modifier) && IsKeyDown(key);
        }

        public bool IsKeyComboPressed(Keys modifier, Keys key)
        {
            return IsKeyDown(modifier) && IsKeyPressed(key);
        }

        // Common key combinations
        public bool IsCtrlDown() => IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);
        public bool IsShiftDown() => IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
        public bool IsAltDown() => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);

        // Get normalized mouse position (0 to 1)
        public Vector2 GetNormalizedMousePosition()
        {
            var pos = GetMousePosition();
            return new Vector2(
                pos.X / _window.Size.X,
                pos.Y / _window.Size.Y
            );
        }

        // Get mouse position in NDC space (-1 to 1)
        public Vector2 GetMousePositionNDC()
        {
            var normalized = GetNormalizedMousePosition();
            return new Vector2(
                normalized.X * 2f - 1f,
                1f - normalized.Y * 2f  // Flip Y axis for OpenGL
            );
        }
    }
}