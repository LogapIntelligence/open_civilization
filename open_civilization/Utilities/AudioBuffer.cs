using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class AudioBuffer : IDisposable
    {
        public int Handle { get; private set; }

        public AudioBuffer(string path)
        {
            Handle = AL.GenBuffer();
            LoadFromFile(path);
        }

        private void LoadFromFile(string path)
        {
            // This is a simplified example - you'd typically use a library like NVorbis for OGG
            // or NAudio for WAV files for production use
            if (path.EndsWith(".wav"))
            {
                LoadWav(path);
            }
            else
            {
                throw new NotSupportedException($"Audio format not supported: {Path.GetExtension(path)}");
            }
        }

        private void LoadWav(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fs);

            // Simple WAV file parsing (for basic PCM WAV files)
            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
                throw new NotSupportedException("Not a valid WAV file");

            reader.ReadInt32(); // File size
            string format = new string(reader.ReadChars(4));
            if (format != "WAVE")
                throw new NotSupportedException("Not a valid WAV file");

            // Find fmt chunk
            while (fs.Position < fs.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                if (chunkId == "fmt ")
                {
                    short audioFormat = reader.ReadInt16();
                    short channels = reader.ReadInt16();
                    int sampleRate = reader.ReadInt32();
                    reader.ReadInt32(); // Byte rate
                    reader.ReadInt16(); // Block align
                    short bitsPerSample = reader.ReadInt16();

                    // Skip any extra format bytes
                    fs.Position += chunkSize - 16;

                    // Find data chunk
                    while (fs.Position < fs.Length)
                    {
                        string dataChunkId = new string(reader.ReadChars(4));
                        int dataChunkSize = reader.ReadInt32();

                        if (dataChunkId == "data")
                        {
                            byte[] data = reader.ReadBytes(dataChunkSize);

                            ALFormat alFormat = GetFormat(channels, bitsPerSample);
                            AL.BufferData(Handle, alFormat, data, sampleRate);
                            return;
                        }
                        else
                        {
                            fs.Position += dataChunkSize;
                        }
                    }
                }
                else
                {
                    fs.Position += chunkSize;
                }
            }
        }

        private ALFormat GetFormat(short channels, short bitsPerSample)
        {
            return (channels, bitsPerSample) switch
            {
                (1, 8) => ALFormat.Mono8,
                (1, 16) => ALFormat.Mono16,
                (2, 8) => ALFormat.Stereo8,
                (2, 16) => ALFormat.Stereo16,
                _ => throw new NotSupportedException($"Unsupported audio format: {channels} channels, {bitsPerSample} bits")
            };
        }

        public void Dispose()
        {
            AL.DeleteBuffer(Handle);
        }
    }
}
