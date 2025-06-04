using open_civilization.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class SceneManager
    {
        private Dictionary<string, Scene> _scenes;
        private Scene _currentScene;
        private Scene _nextScene;

        public Scene CurrentScene => _currentScene;

        public SceneManager()
        {
            _scenes = new Dictionary<string, Scene>();
        }

        public void RegisterScene(string name, Scene scene)
        {
            _scenes[name] = scene;
        }

        public void SwitchToScene(string name)
        {
            if (_scenes.TryGetValue(name, out var scene))
            {
                _nextScene = scene;
            }
        }

        public void Update(float deltaTime)
        {
            if (_nextScene != null)
            {
                _currentScene?.Cleanup();
                _currentScene = _nextScene;
                _currentScene.Initialize();
                _nextScene = null;
            }

            _currentScene?.Update(deltaTime);
        }

        public void Render(Renderer renderer)
        {
            _currentScene?.Render(renderer);
        }
    }
}
