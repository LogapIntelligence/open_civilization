using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Core
{
    public class ShaderManager : IDisposable
    {
        private Dictionary<string, Shader> _shaders;

        public ShaderManager()
        {
            _shaders = new Dictionary<string, Shader>();
        }

        public void LoadShader(string name, string vertexSource, string fragmentSource)
        {
            if (_shaders.ContainsKey(name))
            {
                _shaders[name].Dispose();
            }

            _shaders[name] = new Shader(vertexSource, fragmentSource);
        }

        public Shader GetShader(string name)
        {
            return _shaders.TryGetValue(name, out Shader shader) ? shader : null;
        }

        public void Dispose()
        {
            foreach (var shader in _shaders.Values)
            {
                shader.Dispose();
            }
            _shaders.Clear();
        }
    }
}
