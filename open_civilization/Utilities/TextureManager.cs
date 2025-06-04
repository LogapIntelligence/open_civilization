using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class TextureManager : IDisposable
    {
        private Dictionary<string, Texture> _textures;
        private Texture _whiteTexture;

        public TextureManager()
        {
            _textures = new Dictionary<string, Texture>();
            CreateWhiteTexture();
        }

        private void CreateWhiteTexture()
        {
            byte[] whitePixel = { 255, 255, 255, 255 };
            _whiteTexture = new Texture(1, 1, whitePixel);
            _textures["__white"] = _whiteTexture;
        }

        public Texture LoadTexture(string name, string path)
        {
            if (_textures.ContainsKey(name))
                return _textures[name];

            var texture = new Texture(path);
            _textures[name] = texture;
            return texture;
        }

        public Texture GetTexture(string name)
        {
            return _textures.TryGetValue(name, out var texture) ? texture : _whiteTexture;
        }

        public Texture GetWhiteTexture()
        {
            return _whiteTexture;
        }

        public void Dispose()
        {
            foreach (var texture in _textures.Values)
            {
                texture.Dispose();
            }
            _textures.Clear();
        }
    }
}
