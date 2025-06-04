using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class AudioSource : IDisposable
    {
        public int Handle { get; private set; }

        public Vector3 Position
        {
            get
            {
                AL.GetSource(Handle, ALSource3f.Position, out var pos);
                return new Vector3(pos.X, pos.Y, pos.Z);
            }
            set => AL.Source(Handle, ALSource3f.Position, value.X, value.Y, value.Z);
        }

        public float Volume
        {
            get
            {
                AL.GetSource(Handle, ALSourcef.Gain, out var gain);
                return gain;
            }
            set => AL.Source(Handle, ALSourcef.Gain, value);
        }

        public float Pitch
        {
            get
            {
                AL.GetSource(Handle, ALSourcef.Pitch, out var pitch);
                return pitch;
            }
            set => AL.Source(Handle, ALSourcef.Pitch, value);
        }

        public bool IsLooping
        {
            get
            {
                AL.GetSource(Handle, ALSourceb.Looping, out var looping);
                return looping;
            }
            set => AL.Source(Handle, ALSourceb.Looping, value);
        }

        public AudioSource()
        {
            Handle = AL.GenSource();
        }

        public void Play()
        {
            AL.SourcePlay(Handle);
        }

        public void Pause()
        {
            AL.SourcePause(Handle);
        }

        public void Stop()
        {
            AL.SourceStop(Handle);
        }

        public void SetBuffer(int buffer)
        {
            AL.Source(Handle, ALSourcei.Buffer, buffer);
        }

        public void Dispose()
        {
            AL.DeleteSource(Handle);
        }
    }
}
