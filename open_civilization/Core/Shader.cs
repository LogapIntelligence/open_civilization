using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Core
{
    public class Shader : IDisposable
    {
        private int _handle;
        private Dictionary<string, int> _uniformLocations;

        public Shader(string vertexSource, string fragmentSource)
        {
            _uniformLocations = new Dictionary<string, int>();

            int vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

            _handle = GL.CreateProgram();
            GL.AttachShader(_handle, vertexShader);
            GL.AttachShader(_handle, fragmentShader);
            GL.LinkProgram(_handle);

            GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(_handle);
                throw new Exception($"Error linking shader program: {infoLog}");
            }

            GL.DetachShader(_handle, vertexShader);
            GL.DetachShader(_handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling {type} shader: {infoLog}");
            }

            return shader;
        }

        public void Use()
        {
            GL.UseProgram(_handle);
        }

        public int GetUniformLocation(string name)
        {
            if (_uniformLocations.TryGetValue(name, out int location))
                return location;

            location = GL.GetUniformLocation(_handle, name);
            _uniformLocations[name] = location;
            return location;
        }

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GetUniformLocation(name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetBool(string name, bool value)
        {
            int location = GetUniformLocation(name);
            GL.Uniform1(location, value ? 1 : 0);
        }

        public void SetInt(string name, int value)
        {
            int location = GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetFloat(string name, float value)
        {
            int location = GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void Dispose()
        {
            GL.DeleteProgram(_handle);
        }
    }
}
