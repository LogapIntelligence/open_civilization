using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class AudioManager : IDisposable
    {
        private ALDevice _device;
        private ALContext _context;
        private Dictionary<string, AudioBuffer> _buffers;
        private List<AudioSource> _sources;

        public AudioManager()
        {
            _device = ALC.OpenDevice(null);
            _context = ALC.CreateContext(_device, (int[])null);
            ALC.MakeContextCurrent(_context);

            _buffers = new Dictionary<string, AudioBuffer>();
            _sources = new List<AudioSource>();
        }

        public AudioBuffer LoadSound(string name, string path)
        {
            if (_buffers.ContainsKey(name))
                return _buffers[name];

            var buffer = new AudioBuffer(path);
            _buffers[name] = buffer;
            return buffer;
        }

        public AudioSource CreateSource()
        {
            var source = new AudioSource();
            _sources.Add(source);
            return source;
        }

        public void PlaySound(string name, float volume = 1.0f, float pitch = 1.0f, bool loop = false)
        {
            if (!_buffers.TryGetValue(name, out var buffer))
                return;

            var source = CreateSource();
            source.SetBuffer(buffer.Handle);
            source.Volume = volume;
            source.Pitch = pitch;
            source.IsLooping = loop;
            source.Play();
        }

        public void SetListenerPosition(Vector3 position)
        {
            AL.Listener(ALListener3f.Position, position.X, position.Y, position.Z);
        }

        public void SetListenerOrientation(Vector3 forward, Vector3 up)
        {
            var orientation = new float[] { forward.X, forward.Y, forward.Z, up.X, up.Y, up.Z };
            AL.Listener(ALListenerfv.Orientation, orientation);
        }

        public void Dispose()
        {
            foreach (var source in _sources)
                source.Dispose();

            foreach (var buffer in _buffers.Values)
                buffer.Dispose();

            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(_context);
            ALC.CloseDevice(_device);
        }
    }
}
