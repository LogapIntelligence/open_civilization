using open_civilization.Core;
using OpenTK.Mathematics;

namespace open_civilization.Components
{
    public interface IComponent
    {
        bool Enabled { get; set; }
        IGameObject GameObject { get; set; }
        void Update(float deltaTime);
    }
    public class Physics2DComponent : IComponent
    {
        public bool Enabled { get; set; } = true;
        public IGameObject GameObject { get; set; }

        // Physics properties
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float Mass { get; set; } = 1.0f;
        public float Restitution { get; set; } = 0.5f; // Bounciness (0-1)
        public float Friction { get; set; } = 0.1f;
        public float LinearDamping { get; set; } = 0.01f; // Air resistance

        // Forces
        public Vector2 Force { get; set; }
        public bool UseGravity { get; set; } = true;

        // Constraints
        public bool FreezePositionX { get; set; }
        public bool FreezePositionY { get; set; }
        public bool FreezeRotation { get; set; } = true; // For 2D, we'll keep it simple

        // Collision
        public bool IsStatic { get; set; } = false;
        public bool IsTrigger { get; set; } = false;

        // Bounds (for simple AABB collision)
        public Vector2 Size { get; set; } = Vector2.One;

        public Physics2DComponent(Vector2 position, float mass = 1.0f)
        {
            Position = position;
            Mass = mass;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            Force = Vector2.Zero;
        }

        public void AddForce(Vector2 force)
        {
            Force += force;
        }

        public void AddImpulse(Vector2 impulse)
        {
            if (!IsStatic && Mass > 0)
            {
                Velocity += impulse / Mass;
            }
        }

        public void Update(float deltaTime)
        {

        }

        public Box2 GetBounds()
        {
            return new Box2(Position - Size / 2, Position + Size / 2);
        }
    }
}
