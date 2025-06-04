using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        // Matrix uniforms
        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GetUniformLocation(name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetMatrix3(string name, Matrix3 matrix)
        {
            int location = GetUniformLocation(name);
            GL.UniformMatrix3(location, false, ref matrix);
        }

        // Vector uniforms
        public void SetVector2(string name, Vector2 vector)
        {
            int location = GetUniformLocation(name);
            GL.Uniform2(location, vector.X, vector.Y);
        }

        public void SetVector3(string name, Vector3 vector)
        {
            int location = GetUniformLocation(name);
            GL.Uniform3(location, vector.X, vector.Y, vector.Z);
        }

        public void SetVector4(string name, Vector4 vector)
        {
            int location = GetUniformLocation(name);
            GL.Uniform4(location, vector.X, vector.Y, vector.Z, vector.W);
        }

        public void SetColor4(string name, Color4 color)
        {
            int location = GetUniformLocation(name);
            GL.Uniform4(location, color.R, color.G, color.B, color.A);
        }

        // Scalar uniforms
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

        // Array uniforms
        public void SetFloatArray(string name, float[] values)
        {
            int location = GetUniformLocation(name);
            GL.Uniform1(location, values.Length, values);
        }

        public void SetIntArray(string name, int[] values)
        {
            int location = GetUniformLocation(name);
            GL.Uniform1(location, values.Length, values);
        }

        public void SetVector3Array(string name, Vector3[] vectors)
        {
            int location = GetUniformLocation(name);
            float[] data = new float[vectors.Length * 3];
            for (int i = 0; i < vectors.Length; i++)
            {
                data[i * 3 + 0] = vectors[i].X;
                data[i * 3 + 1] = vectors[i].Y;
                data[i * 3 + 2] = vectors[i].Z;
            }
            GL.Uniform3(location, vectors.Length, data);
        }

        // Utility methods for common lighting setups
        public void SetDirectionalLight(string baseName, Vector3 direction, Color4 color, float intensity = 1.0f)
        {
            SetVector3($"{baseName}.direction", direction);
            SetColor4($"{baseName}.color", color);
            SetFloat($"{baseName}.intensity", intensity);
        }

        public void SetPointLight(string baseName, Vector3 position, Color4 color, float intensity = 1.0f,
                                 float constant = 1.0f, float linear = 0.09f, float quadratic = 0.032f)
        {
            SetVector3($"{baseName}.position", position);
            SetColor4($"{baseName}.color", color);
            SetFloat($"{baseName}.intensity", intensity);
            SetFloat($"{baseName}.constant", constant);
            SetFloat($"{baseName}.linear", linear);
            SetFloat($"{baseName}.quadratic", quadratic);
        }

        public void SetMaterial(string baseName, Color4 ambient, Color4 diffuse, Color4 specular, float shininess)
        {
            SetColor4($"{baseName}.ambient", ambient);
            SetColor4($"{baseName}.diffuse", diffuse);
            SetColor4($"{baseName}.specular", specular);
            SetFloat($"{baseName}.shininess", shininess);
        }

        public void Dispose()
        {
            GL.DeleteProgram(_handle);
        }
    }
}
