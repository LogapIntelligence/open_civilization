using OpenTK.Mathematics;
using System;

namespace open_civilization.Core
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _front;
        private Vector3 _up;
        private Vector3 _right;
        private Vector3 _worldUp;

        public float Yaw { get; set; }
        public float Pitch { get; set; }

        private float _speed;
        private float _sensitivity;
        private float _zoom;
        private float _aspectRatio;

        // Properties with both getters and setters where needed
        public Vector3 Position
        {
            get => _position;
            set => _position = value;
        }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        public float Zoom
        {
            get => _zoom;
            set => _zoom = Math.Clamp(value, 1.0f, 45.0f);
        }

        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        public float Sensitivity
        {
            get => _sensitivity;
            set => _sensitivity = value;
        }

        public Camera(Vector3 position, float aspectRatio)
        {
            _position = position;
            _worldUp = Vector3.UnitY;
            Yaw = -90.0f;
            Pitch = 0.0f;
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
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(_zoom),
                _aspectRatio,
                0.1f,
                100.0f
            );
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
                case CameraMovement.Up:
                    _position += _worldUp * velocity;
                    break;
                case CameraMovement.Down:
                    _position -= _worldUp * velocity;
                    break;
            }
        }

        public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
        {
            xOffset *= _sensitivity;
            yOffset *= _sensitivity;

            Yaw += xOffset;
            Pitch += yOffset;

            if (constrainPitch)
            {
                Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);
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
            // This method can be extended for automatic camera movements, animations, etc.
        }

        // Look at a specific target position
        public void LookAt(Vector3 target)
        {
            Vector3 direction = Vector3.Normalize(target - _position);

            // Calculate yaw (rotation around Y axis)
            Yaw = MathHelper.RadiansToDegrees((float)Math.Atan2(direction.Z, direction.X));

            // Calculate pitch (rotation around X axis)
            Pitch = MathHelper.RadiansToDegrees((float)Math.Asin(direction.Y));

            UpdateCameraVectors();
        }

        // Set camera to orbit around a target
        public void OrbitAround(Vector3 target, float radius, float angle, float height)
        {
            float x = target.X + radius * (float)Math.Cos(MathHelper.DegreesToRadians(angle));
            float z = target.Z + radius * (float)Math.Sin(MathHelper.DegreesToRadians(angle));
            float y = target.Y + height;

            _position = new Vector3(x, y, z);
            LookAt(target);
        }

        private void UpdateCameraVectors()
        {
            // Calculate the new front vector
            Vector3 front = new Vector3
            {
                X = (float)(Math.Cos(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch))),
                Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch)),
                Z = (float)(Math.Sin(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)))
            };

            _front = Vector3.Normalize(front);
            _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }

        // Get the camera's view-projection matrix (commonly used combination)
        public Matrix4 GetViewProjectionMatrix()
        {
            return GetViewMatrix() * GetProjectionMatrix();
        }

        // Reset camera to default position and orientation
        public void Reset(Vector3 position, float yaw = -90.0f, float pitch = 0.0f)
        {
            _position = position;
            Yaw = yaw;
            Pitch = pitch;
            _zoom = 45.0f;
            UpdateCameraVectors();
        }
    }

    public enum CameraMovement
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }
}