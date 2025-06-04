using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class AssetManager : IDisposable
    {
        private TextureManager _textureManager;
        private AudioManager _audioManager;
        private Dictionary<string, object> _assets;

        public TextureManager Textures => _textureManager;
        public AudioManager Audio => _audioManager;

        public AssetManager()
        {
            _textureManager = new TextureManager();
            _audioManager = new AudioManager();
            _assets = new Dictionary<string, object>();
        }

        public void LoadAssets(string manifestPath)
        {
            // Load assets from a manifest file
            if (!File.Exists(manifestPath))
                return;

            var lines = File.ReadAllLines(manifestPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('|');
                if (parts.Length < 3)
                    continue;

                string type = parts[0].Trim();
                string name = parts[1].Trim();
                string path = parts[2].Trim();

                try
                {
                    switch (type.ToLower())
                    {
                        case "texture":
                            _textureManager.LoadTexture(name, path);
                            break;
                        case "audio":
                            _audioManager.LoadSound(name, path);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load asset {name}: {ex.Message}");
                }
            }
        }

        public T GetAsset<T>(string name) where T : class
        {
            return _assets.TryGetValue(name, out var asset) ? asset as T : null;
        }

        public void SetAsset<T>(string name, T asset)
        {
            _assets[name] = asset;
        }

        public void Dispose()
        {
            _textureManager?.Dispose();
            _audioManager?.Dispose();
            _assets.Clear();
        }
    }
}
