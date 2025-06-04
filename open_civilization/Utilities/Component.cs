using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public abstract class Component
    {
        public Entity Owner { get; internal set; }
        public virtual void Update(float deltaTime) { }
    }
}
