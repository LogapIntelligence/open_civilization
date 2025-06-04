using open_civilization.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    // ==================== SCENE MANAGEMENT ====================
    public abstract class Scene
    {
        protected List<IGameObject> _objects;
        protected Camera _camera;

        public Scene()
        {
            _objects = new List<IGameObject>();
        }

        public virtual void Initialize()
        {
            // Override in derived classes
        }

        public virtual void Update(float deltaTime)
        {
            foreach (var obj in _objects)
            {
                obj.Update(deltaTime);
            }
        }

        public virtual void Render(Renderer renderer)
        {
            foreach (var obj in _objects)
            {
                obj.Render(renderer);
            }
        }

        public virtual void Cleanup()
        {
            _objects.Clear();
        }

        public void AddObject(IGameObject obj)
        {
            _objects.Add(obj);
        }

        public void RemoveObject(IGameObject obj)
        {
            _objects.Remove(obj);
        }

        public Camera GetCamera() => _camera;
    }
}
