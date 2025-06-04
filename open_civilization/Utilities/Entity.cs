using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class Entity
    {
        private List<Component> _components;
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;

        public Entity()
        {
            _components = new List<Component>();
        }

        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T();
            component.Owner = this;
            _components.Add(component);
            return component;
        }

        public T GetComponent<T>() where T : Component
        {
            return _components.OfType<T>().FirstOrDefault();
        }

        public void RemoveComponent<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component != null)
                _components.Remove(component);
        }

        public void Update(float deltaTime)
        {
            foreach (var component in _components)
            {
                component.Update(deltaTime);
            }
        }
    }
}
