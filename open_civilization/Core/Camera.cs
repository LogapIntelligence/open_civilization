using OpenTK.Mathematics;

namespace open_civilization.Core
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _front;
        private Vector3 _up;
        private Vector3 _right;
        private Vector3 _worldUp;

        private float _yaw;
        private float _pitch;
        private float _speed;
        private float _sensitivity;
        private float _zoom;
        private float _aspectRatio;

        public Vector3 Position => _position;
        public Vector3 Front => _front;
        public float Zoom => _zoom;

        public Camera(Vector3 position, float aspectRatio)
        {
            _position = position;
            _worldUp = Vector3.UnitY;
            _yaw = -90.0f;
            _pitch = 0.0f;
            _speed = 2.5f;
            _sensitivity = 0.1f;
            _zoom = 45.0f;
            _aspectRatio = aspectRatio;

            UpdateCameraVectors();
        }

        public void UpdatePosition(Vector3 position)
        {
            _position = position;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(_position, _position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_zoom), _aspectRatio, 0.1f, 100.0f);
        }

        public void UpdateProjection(float aspectRatio)
        {
            _aspectRatio = aspectRatio;
        }

        public void ProcessKeyboard(CameraMovement direction, float deltaTime)
        {
            float velocity = _speed * deltaTime;

            switch (direction)
            {
                case CameraMovement.Forward:
                    _position += _front * velocity;
                    break;
                case CameraMovement.Backward:
                    _position -= _front * velocity;
                    break;
                case CameraMovement.Left:
                    _position -= _right * velocity;
                    break;
                case CameraMovement.Right:
                    _position += _right * velocity;
                    break;
            }
        }

        public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
        {
            xOffset *= _sensitivity;
            yOffset *= _sensitivity;

            _yaw += xOffset;
            _pitch += yOffset;

            if (constrainPitch)
            {
                _pitch = Math.Clamp(_pitch, -89.0f, 89.0f);
            }

            UpdateCameraVectors();
        }

        public void ProcessMouseScroll(float yOffset)
        {
            _zoom -= yOffset;
            _zoom = Math.Clamp(_zoom, 1.0f, 45.0f);
        }

        public void Update(float deltaTime)
        {
            // Update camera logic if needed
        }

        private void UpdateCameraVectors()
        {
            Vector3 front = new Vector3
            {
                X = (float)(Math.Cos(MathHelper.DegreesToRadians(_yaw)) * Math.Cos(MathHelper.DegreesToRadians(_pitch))),
                Y = (float)Math.Sin(MathHelper.DegreesToRadians(_pitch)),
                Z = (float)(Math.Sin(MathHelper.DegreesToRadians(_yaw)) * Math.Cos(MathHelper.DegreesToRadians(_pitch)))
            };

            _front = Vector3.Normalize(front);
            _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
    public enum CameraMovement
    {
        Forward,
        Backward,
        Left,
        Right
    }
}
