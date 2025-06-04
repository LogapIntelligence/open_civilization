using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Utilities
{
    public class Texture : IDisposable
    {
        public int Handle { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture(string path)
        {
            LoadFromFile(path);
        }

        public Texture(int width, int height, byte[] data)
        {
            LoadFromData(width, height, data);
        }

        private void LoadFromFile(string path)
        {
            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            using var image = new Bitmap(path);
            Width = image.Width;
            Height = image.Height;

            var data = image.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                Width, Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            SetDefaultParameters();
        }

        private void LoadFromData(int width, int height, byte[] data)
        {
            Handle = GL.GenTexture();
            Width = width;
            Height = height;

            GL.BindTexture(TextureTarget.Texture2D, Handle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                width, height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                PixelType.UnsignedByte, data);

            SetDefaultParameters();
        }

        private void SetDefaultParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public void Dispose()
        {
            GL.DeleteTexture(Handle);
        }
    }
}
