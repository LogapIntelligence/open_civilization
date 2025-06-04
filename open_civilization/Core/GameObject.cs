using OpenTK.Mathematics;

namespace open_civilization.Core
{
    // ==================== GAME OBJECT INTERFACE ====================
    public interface IGameObject
    {
        void Update(float deltaTime);
        void Render(Renderer renderer);
    }
    public class GameObject : IGameObject
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Color4 Color { get; set; }

        public GameObject()
        {
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
            Color = Color4.White;
        }

        public virtual void Update(float deltaTime)
        {
            // Override in derived classes
        }

        public virtual void Render(Renderer renderer)
        {
            // Override in derived classes
        }
    }
}
