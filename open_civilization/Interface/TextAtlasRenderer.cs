using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace open_civilization.Interface
{
    /// <summary>
    /// Creates a single texture atlas from multiple strings.
    /// </summary>
    public class TextAtlasRenderer
    {
        private StbTextRenderer _textRenderer;

        public TextAtlasRenderer(StbTextRenderer textRenderer)
        {
            _textRenderer = textRenderer;
        }

        /// <summary>
        /// Creates a texture atlas containing the given texts.
        /// </summary>
        /// <param name="texts">The texts to render onto the atlas.</param>
        /// <param name="textColor">The color of the text.</param>
        /// <param name="bgColor">The background color of the texture.</param>
        /// <param name="atlasWidth">The width of the atlas texture.</param>
        /// <param name="atlasHeight">The height of the atlas texture.</param>
        /// <param name="gridCols">How many columns the atlas grid should have.</param>
        /// <param name="gridRows">How many rows the atlas grid should have.</param>
        /// <returns>A tuple containing the texture ID and a dictionary mapping each text to its UV coordinates.</returns>
        public (int textureId, Dictionary<string, RectangleF> uvMap) CreateAtlas(
            IEnumerable<string> texts,
            Color4 textColor,
            Color4 bgColor,
            int atlasWidth = 1024,
            int atlasHeight = 1024,
            int gridCols = 3,
            int gridRows = 2)
        {
            var uvMap = new Dictionary<string, RectangleF>();

            // Save current OpenGL state
            int currentFbo = GL.GetInteger(GetPName.FramebufferBinding);
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            // Create a new framebuffer and texture for the atlas
            int fbo = GL.GenFramebuffer();
            int texture = GL.GenTexture();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            // Setup the atlas texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, atlasWidth, atlasHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);

            // Setup rendering to the new framebuffer
            GL.Viewport(0, 0, atlasWidth, atlasHeight);
            GL.ClearColor(bgColor);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            _textRenderer.UpdateWindowSize(atlasWidth, atlasHeight);

            int cellWidth = atlasWidth / gridCols;
            int cellHeight = atlasHeight / gridRows;
            int currentCell = 0;

            foreach (var text in texts)
            {
                int col = currentCell % gridCols;
                int row = currentCell / gridCols;

                float cellX = col * cellWidth;
                float cellY = row * cellHeight;

                // Calculate centered position for the text within its cell
                float textWidth = _textRenderer.MeasureString(text);
                float x = cellX + (cellWidth - textWidth) / 2;
                float y = cellY + (cellHeight - _textRenderer.GetLineHeight()) / 2 + 175;

                // Render the text
                _textRenderer.RenderText(text, x, y, 1.0f, new Vector3(textColor.R, textColor.G, textColor.B));

                // Store the UV coordinates for this text
                var uvRect = new RectangleF(
                    (float)col / gridCols,
                    (float)row / gridRows,
                    1.0f / gridCols,
                    1.0f / gridRows
                );
                uvMap[text] = uvRect;

                currentCell++;
            }

            // Restore original OpenGL state
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, currentFbo);
            GL.Viewport(viewport[0], viewport[1], viewport[2], viewport[3]);
            _textRenderer.UpdateWindowSize(viewport[2], viewport[3]);

            // Cleanup
            GL.DeleteFramebuffer(fbo);

            return (texture, uvMap);
        }
    }
}