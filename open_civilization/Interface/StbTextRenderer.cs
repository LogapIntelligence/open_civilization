using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbTrueTypeSharp;
using System.Reflection;


namespace open_civilization.Interface
{
    /// <summary>
    /// High-quality text renderer using STB TrueType for cross-platform font rendering
    /// </summary>
    public class StbTextRenderer : IDisposable
    {
        private int _shaderProgram;
        private int _vao, _vbo, _ebo;
        private int _fontTexture;
        private Dictionary<int, StbTrueType.stbtt_packedchar> _packedChars;
        private StbTrueType.stbtt_pack_context _packContext;
        private float _fontSize;
        private int _atlasWidth = 2048;  // Larger for better quality
        private int _atlasHeight = 2048;
        private int _windowWidth, _windowHeight;
        private float _lineHeight;
        private bool _disposed = false;

        // Character range - extended for more special characters
        private const int FIRST_CHAR = 32;
        private const int CHAR_COUNT = 224; // Extended to include more Unicode

        // Shader source code
        private const string VertexShaderSource = @"
#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTex;

out vec2 TexCoords;

uniform mat4 projection;

void main()
{
    gl_Position = projection * vec4(aPos, 0.0, 1.0);
    TexCoords = aTex;
}";

        private const string FragmentShaderSource = @"
#version 330 core
in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D text;
uniform vec3 textColor;

void main()
{    
    // Sample the red channel (where STB stores the alpha)
    float alpha = texture(text, TexCoords).r;
    
    // Apply gamma correction for better quality
    alpha = pow(alpha, 1.0/2.2);
    
    // Output with premultiplied alpha for better blending
    FragColor = vec4(textColor * alpha, alpha);
}";

        /// <summary>
        /// Create a text renderer with automatic font detection
        /// </summary>
        public StbTextRenderer(int windowWidth, int windowHeight, float fontSize = 24)
            : this(windowWidth, windowHeight, (string)null, fontSize)
        {
        }

        /// <summary>
        /// Create a text renderer with a specific font file
        /// </summary>
        public StbTextRenderer(int windowWidth, int windowHeight, string fontPath, float fontSize = 24)
        {
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
            _fontSize = fontSize;
            _packedChars = new Dictionary<int, StbTrueType.stbtt_packedchar>();

            InitializeShaders();
            InitializeBuffers();

            // Load font
            byte[] fontData = null;

            if (string.IsNullOrEmpty(fontPath))
            {
                // Try to load embedded font first
                fontData = LoadEmbeddedFont();

                // If no embedded font, try system fonts
                if (fontData == null)
                {
                    fontPath = FindSystemFont();
                    if (File.Exists(fontPath))
                    {
                        fontData = File.ReadAllBytes(fontPath);
                    }
                }
            }
            else if (File.Exists(fontPath))
            {
                fontData = File.ReadAllBytes(fontPath);
            }

            if (fontData == null || fontData.Length == 0)
            {
                throw new FileNotFoundException("No suitable font found. Please provide a font file.");
            }

            LoadFontFromMemory(fontData, fontSize);
        }

        /// <summary>
        /// Create a text renderer with font data from memory
        /// </summary>
        public StbTextRenderer(int windowWidth, int windowHeight, byte[] fontData, float fontSize = 24)
        {
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
            _fontSize = fontSize;
            _packedChars = new Dictionary<int, StbTrueType.stbtt_packedchar>();

            InitializeShaders();
            InitializeBuffers();
            LoadFontFromMemory(fontData, fontSize);
        }

        private byte[] LoadEmbeddedFont()
        {
            // Look for embedded font resources
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            // Common font extensions
            string[] fontExtensions = { ".ttf", ".otf" };

            foreach (var resourceName in resourceNames)
            {
                foreach (var ext in fontExtensions)
                {
                    if (resourceName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    stream.CopyTo(ms);
                                    return ms.ToArray();
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private string FindSystemFont()
        {
            // Platform-specific font paths
            string[] windowsFonts =
            {
                @"C:\Windows\Fonts\arial.ttf",
                @"C:\Windows\Fonts\Arial.ttf",
                @"C:\Windows\Fonts\segoeui.ttf",
                @"C:\Windows\Fonts\calibri.ttf",
                @"C:\Windows\Fonts\tahoma.ttf"
            };

            string[] linuxFonts =
            {
                "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
                "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                "/usr/share/fonts/truetype/ubuntu/Ubuntu-R.ttf",
                "/usr/share/fonts/truetype/noto/NotoSans-Regular.ttf",
                "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc"
            };

            string[] macFonts =
            {
                "/System/Library/Fonts/Helvetica.ttc",
                "/Library/Fonts/Arial.ttf",
                "/System/Library/Fonts/Avenir.ttc",
                "/System/Library/Fonts/Courier.ttc"
            };

            // Determine platform and check appropriate paths
            PlatformID platform = Environment.OSVersion.Platform;
            string[] fontsToCheck = null;

            switch (platform)
            {
                case PlatformID.Win32NT:
                    fontsToCheck = windowsFonts;
                    break;
                case PlatformID.Unix:
                    // Could be Linux or macOS
                    if (Directory.Exists("/System/Library/Fonts"))
                        fontsToCheck = macFonts;
                    else
                        fontsToCheck = linuxFonts;
                    break;
                case PlatformID.MacOSX:
                    fontsToCheck = macFonts;
                    break;
            }

            if (fontsToCheck != null)
            {
                foreach (var path in fontsToCheck)
                {
                    if (File.Exists(path))
                        return path;
                }
            }

            // Last resort: search common font directories
            string[] commonDirs =
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                "/usr/share/fonts",
                "/usr/local/share/fonts",
                "~/.fonts"
            };

            foreach (var dir in commonDirs)
            {
                if (Directory.Exists(dir))
                {
                    var ttfFiles = Directory.GetFiles(dir, "*.ttf", SearchOption.AllDirectories);
                    if (ttfFiles.Length > 0)
                        return ttfFiles[0];
                }
            }

            return null;
        }

        private void InitializeShaders()
        {
            // Compile vertex shader
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, VertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader, "Vertex");

            // Compile fragment shader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, FragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader, "Fragment");

            // Create program
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);
            CheckProgramLinking(_shaderProgram);

            // Cleanup
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private void CheckShaderCompilation(int shader, string shaderType)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"{shaderType} shader compilation failed: {infoLog}");
            }
        }

        private void CheckProgramLinking(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Shader program linking failed: {infoLog}");
            }
        }

        private void InitializeBuffers()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            // Setup VBO for dynamic data
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 6 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Setup EBO
            uint[] indices = { 0, 1, 2, 0, 2, 3 };
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Texture coordinate attribute
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        private unsafe void LoadFontFromMemory(byte[] fontData, float fontSize)
        {
            // Create texture for font atlas
            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);

            // Allocate texture memory
            byte[] atlasData = new byte[_atlasWidth * _atlasHeight];

            fixed (byte* fontPtr = fontData)
            fixed (byte* atlasPtr = atlasData)
            {
                // Initialize font
                var fontInfo = new StbTrueType.stbtt_fontinfo();
                if (StbTrueType.stbtt_InitFont(fontInfo, fontPtr, 0) == 0)
                {
                    throw new Exception("Failed to initialize font");
                }

                // Get font metrics for line height
                int ascent, descent, lineGap;
                StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);
                float scale = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, fontSize);
                _lineHeight = (ascent - descent + lineGap) * scale;

                // Pack font into atlas
                _packContext = new StbTrueType.stbtt_pack_context();
                StbTrueType.stbtt_PackBegin(_packContext, atlasPtr, _atlasWidth, _atlasHeight, 0, 1, null);

                // Set oversampling for better quality
                StbTrueType.stbtt_PackSetOversampling(_packContext, 2, 2);

                // Allocate packed char data
                var packedChars = new StbTrueType.stbtt_packedchar[CHAR_COUNT];
                fixed (StbTrueType.stbtt_packedchar* charPtr = packedChars)
                {
                    // Pack the font
                    StbTrueType.stbtt_PackFontRange(_packContext, fontPtr, 0, fontSize,
                        FIRST_CHAR, CHAR_COUNT, charPtr);
                }

                StbTrueType.stbtt_PackEnd(_packContext);

                // Store packed chars in dictionary
                for (int i = 0; i < CHAR_COUNT; i++)
                {
                    _packedChars[FIRST_CHAR + i] = packedChars[i];
                }
            }

            // Upload texture to GPU
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8,
                         _atlasWidth, _atlasHeight, 0, PixelFormat.Red,
                         PixelType.UnsignedByte, atlasData);

            // Set texture parameters for best quality
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                           (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                           (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                           (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                           (int)TextureWrapMode.ClampToEdge);

            // Generate mipmaps for better quality at different scales
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                           (int)TextureMinFilter.LinearMipmapLinear);

            // Enable anisotropic filtering if available
            float maxAniso = GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt);
            if (maxAniso > 0)
            {
                GL.TexParameter(TextureTarget.Texture2D,
                    (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt,
                    Math.Min(16.0f, maxAniso));
            }
        }

        /// <summary>
        /// Update the window size for proper projection
        /// </summary>
        public void UpdateWindowSize(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        /// <summary>
        /// Get the width of a text string
        /// </summary>
        public float MeasureString(string text, float scale = 1.0f)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            float width = 0;
            float x = 0, y = 0;
            unsafe
            {
                var quad = new StbTrueType.stbtt_aligned_quad();
                foreach (char c in text)
                {
                    if (_packedChars.TryGetValue(c, out var pc))
                    {
                        // Create a temporary copy and get its address
                        var tempPc = pc;
                        StbTrueType.stbtt_GetPackedQuad(&tempPc, _atlasWidth, _atlasHeight,
                            0, &x, &y, &quad, 1);
                    }
                    else if (c == ' ')
                    {
                        x += _fontSize * 0.3f;
                    }
                }
                width = x * scale;
            }
            return width;
        }

        /// <summary>
        /// Get the line height for the current font
        /// </summary>
        public float GetLineHeight(float scale = 1.0f)
        {
            return _lineHeight * scale;
        }

        /// <summary>
        /// Render text at the specified position
        /// </summary>
        public unsafe void RenderText(string text, float x, float y, float scale = 1.0f, Vector3? color = null)
        {
            if (string.IsNullOrEmpty(text)) return;

            Vector3 textColor = color ?? Vector3.One;

            // Save OpenGL state
            int currentProgram = GL.GetInteger(GetPName.CurrentProgram);
            bool depthTest = GL.IsEnabled(EnableCap.DepthTest);
            bool cullFace = GL.IsEnabled(EnableCap.CullFace);
            bool blend = GL.IsEnabled(EnableCap.Blend);
            int blendSrc = GL.GetInteger(GetPName.BlendSrc);
            int blendDst = GL.GetInteger(GetPName.BlendDst);
            int activeTexture = GL.GetInteger(GetPName.ActiveTexture);
            int boundTexture = GL.GetInteger(GetPName.TextureBinding2D);

            // Setup rendering state
            GL.UseProgram(_shaderProgram);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Setup projection matrix (top-left origin)
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, _windowWidth, _windowHeight, 0, -1, 1);
            int projectionLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            GL.UniformMatrix4(projectionLoc, false, ref projection);

            // Set text color
            int colorLoc = GL.GetUniformLocation(_shaderProgram, "textColor");
            GL.Uniform3(colorLoc, textColor);

            // Bind texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            int textureLoc = GL.GetUniformLocation(_shaderProgram, "text");
            GL.Uniform1(textureLoc, 0);

            GL.BindVertexArray(_vao);

            float currentX = x;
            float currentY = y;

            // Create a quad for aligned data
            var quad = new StbTrueType.stbtt_aligned_quad();

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    // Handle newline
                    currentX = x;
                    currentY += _lineHeight * scale;
                    continue;
                }

                if (_packedChars.TryGetValue(c, out var pc))
                {
                    float tempX = currentX;
                    float tempY = currentY;
                    var tempPc = pc;
                    StbTrueType.stbtt_GetPackedQuad(&tempPc, _atlasWidth, _atlasHeight,
                        0, &tempX, &tempY, &quad, 1);

                    // Apply scale and adjust position
                    float xDiff = (tempX - currentX) * scale;
                    float x0 = currentX + (quad.x0 - currentX) * scale;
                    float y0 = currentY + (quad.y0 - currentY) * scale;
                    float x1 = currentX + (quad.x1 - currentX) * scale;
                    float y1 = currentY + (quad.y1 - currentY) * scale;

                    // Create vertices for the character quad
                    float[] vertices = {
                        // Position       // TexCoords
                        x0, y0,          quad.s0, quad.t0,  // Top-left
                        x0, y1,          quad.s0, quad.t1,  // Bottom-left
                        x1, y1,          quad.s1, quad.t1,  // Bottom-right
                        x1, y0,          quad.s1, quad.t0   // Top-right
                    };

                    // Update VBO
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero,
                                    vertices.Length * sizeof(float), vertices);
                    // Render quad
                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                    // Update position for next character
                    currentX += xDiff;
                }
                else if (c == ' ')
                {
                    // Handle space character
                    currentX += _fontSize * 0.3f * scale;
                }
                else if (c == '\t')
                {
                    // Handle tab (4 spaces)
                    currentX += _fontSize * 1.2f * scale;
                }
            }

            // Restore OpenGL state
            GL.BindVertexArray(0);
            GL.ActiveTexture((TextureUnit)activeTexture);
            GL.BindTexture(TextureTarget.Texture2D, boundTexture);

            GL.BlendFunc((BlendingFactor)blendSrc, (BlendingFactor)blendDst);
            if (!blend) GL.Disable(EnableCap.Blend);
            if (depthTest) GL.Enable(EnableCap.DepthTest);
            if (cullFace) GL.Enable(EnableCap.CullFace);
            GL.UseProgram(currentProgram);
        }

        /// <summary>
        /// Render centered text
        /// </summary>
        public void RenderTextCentered(string text, float x, float y, float scale = 1.0f, Vector3? color = null)
        {
            float width = MeasureString(text, scale);
            RenderText(text, x - width * 0.5f, y, scale, color);
        }

        /// <summary>
        /// Render right-aligned text
        /// </summary>
        public void RenderTextRightAligned(string text, float x, float y, float scale = 1.0f, Vector3? color = null)
        {
            float width = MeasureString(text, scale);
            RenderText(text, x - width, y, scale, color);
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteVertexArray(_vao);
                GL.DeleteBuffer(_vbo);
                GL.DeleteBuffer(_ebo);
                GL.DeleteTexture(_fontTexture);
                GL.DeleteProgram(_shaderProgram);

                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        ~StbTextRenderer()
        {
            Dispose();
        }
    }

    /// <summary>
    /// Extension class for OpenGL extension enums if not available
    /// </summary>
    internal static class ExtTextureFilterAnisotropic
    {
        public const int TextureMaxAnisotropyExt = 0x84FE;
        public const int MaxTextureMaxAnisotropyExt = 0x84FF;
    }
}