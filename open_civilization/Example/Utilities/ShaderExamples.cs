using open_civilization.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Example.Utilities
{
    public static class ShaderExamples
    {

        /// <summary>
        /// Creates a simple shader that renders a mesh with a solid color and basic lighting
        /// </summary>
        public static Shader CreateColorShader(Color4 color)
        {
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec3 aNormal;
                layout (location = 2) in vec2 aTexCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                uniform mat3 normalMatrix;

                out vec3 FragPos;
                out vec3 Normal;

                void main()
                {
                    FragPos = vec3(model * vec4(aPos, 1.0));
                    Normal = normalMatrix * aNormal;
                    
                    gl_Position = projection * view * vec4(FragPos, 1.0);
                }";

            string fragmentShader = $@"
                #version 330 core
                in vec3 FragPos;
                in vec3 Normal;

                uniform vec3 lightPos;
                uniform vec3 viewPos;

                out vec4 FragColor;

                void main()
                {{
                    // Hard-coded color from shader creation
                    vec4 objectColor = vec4({color.R}f, {color.G}f, {color.B}f, {color.A}f);
                    vec3 lightColor = vec3(1.0, 1.0, 1.0);

                    // Ambient lighting
                    float ambientStrength = 0.3;
                    vec3 ambient = ambientStrength * lightColor;

                    // Diffuse lighting
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float diff = max(dot(norm, lightDir), 0.0);
                    vec3 diffuse = diff * lightColor;

                    // Specular lighting
                    float specularStrength = 0.5;
                    vec3 viewDir = normalize(viewPos - FragPos);
                    vec3 reflectDir = reflect(-lightDir, norm);
                    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
                    vec3 specular = specularStrength * spec * lightColor;

                    vec3 lighting = ambient + diffuse + specular;
                    
                    FragColor = vec4(lighting * objectColor.rgb, objectColor.a);
                }}";

            return new Shader(vertexShader, fragmentShader);
        }

        /// <summary>
        /// Creates a simple shader that renders a mesh with a solid color (no lighting)
        /// </summary>
        public static Shader CreateUnlitColorShader(Color4 color)
        {
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec3 aPos;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main()
                {
                    gl_Position = projection * view * model * vec4(aPos, 1.0);
                }";

            string fragmentShader = $@"
                #version 330 core
                out vec4 FragColor;

                void main()
                {{
                    FragColor = vec4({color.R}f, {color.G}f, {color.B}f, {color.A}f);
                }}";

            return new Shader(vertexShader, fragmentShader);
        }

        /// <summary>
        /// Creates a shader that uses a uniform color (can be changed at runtime)
        /// </summary>
        public static Shader CreateDynamicColorShader()
        {
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec3 aNormal;
                layout (location = 2) in vec2 aTexCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                uniform mat3 normalMatrix;

                out vec3 FragPos;
                out vec3 Normal;

                void main()
                {
                    FragPos = vec3(model * vec4(aPos, 1.0));
                    Normal = normalMatrix * aNormal;
                    
                    gl_Position = projection * view * vec4(FragPos, 1.0);
                }";

            string fragmentShader = @"
                #version 330 core
                in vec3 FragPos;
                in vec3 Normal;

                uniform vec4 objectColor;
                uniform vec3 lightPos;
                uniform vec3 lightColor;
                uniform vec3 viewPos;

                out vec4 FragColor;

                void main()
                {
                    // Ambient lighting
                    float ambientStrength = 0.3;
                    vec3 ambient = ambientStrength * lightColor;

                    // Diffuse lighting
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float diff = max(dot(norm, lightDir), 0.0);
                    vec3 diffuse = diff * lightColor;

                    // Specular lighting
                    float specularStrength = 0.5;
                    vec3 viewDir = normalize(viewPos - FragPos);
                    vec3 reflectDir = reflect(-lightDir, norm);
                    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
                    vec3 specular = specularStrength * spec * lightColor;

                    vec3 lighting = ambient + diffuse + specular;
                    
                    FragColor = vec4(lighting * objectColor.rgb, objectColor.a);
                }";

            return new Shader(vertexShader, fragmentShader);
        }
        public static Shader CreateToonShader()
        {
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec3 aNormal;
                layout (location = 2) in vec2 aTexCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                uniform mat3 normalMatrix;

                out vec3 FragPos;
                out vec3 Normal;

                void main()
                {
                    FragPos = vec3(model * vec4(aPos, 1.0));
                    Normal = normalMatrix * aNormal;
                    gl_Position = projection * view * vec4(FragPos, 1.0);
                }";

            string fragmentShader = @"
                #version 330 core
                in vec3 FragPos;
                in vec3 Normal;

                uniform vec4 objectColor;
                uniform vec3 lightPos;
                uniform vec3 viewPos;

                out vec4 FragColor;

                void main()
                {
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float intensity = dot(norm, lightDir);
                    
                    // Toon shading levels
                    vec3 color;
                    if (intensity > 0.95)
                        color = objectColor.rgb * 1.0;
                    else if (intensity > 0.5)
                        color = objectColor.rgb * 0.7;
                    else if (intensity > 0.05)
                        color = objectColor.rgb * 0.35;
                    else
                        color = objectColor.rgb * 0.1;
                    
                    FragColor = vec4(color, objectColor.a);
                }";

            return new Shader(vertexShader, fragmentShader);
        }

        public static Shader CreateWaterShader()
        {
            string vertexShader = @"
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aNormal;
        layout (location = 2) in vec2 aTexCoord;
        
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        uniform float time;
        
        // Wave parameters
        uniform float waveSpeed;
        uniform float waveAmplitude;
        uniform float waveFrequency;
        
        out vec3 FragPos;
        out vec3 Normal;
        out vec2 TexCoord;
        out vec3 ViewPos;
        
        // Gerstner wave function
        vec3 gerstnerWave(vec2 dir, float amplitude, float frequency, float speed, vec3 pos) {
            float steepness = 0.5;
            float k = 2.0 * 3.14159 / frequency;
            float c = sqrt(9.8 / k);
            vec2 d = normalize(dir);
            float f = k * (dot(d, pos.xz) - c * speed * time);
            float a = steepness / k;
            
            return vec3(
                d.x * (a * cos(f)),
                a * sin(f) * amplitude,
                d.y * (a * cos(f))
            );
        }
        
        void main()
        {
            vec3 pos = aPos;
            vec3 tangent = vec3(1, 0, 0);
            vec3 binormal = vec3(0, 0, 1);
            
            // Apply multiple wave layers for more realistic water
            vec3 wave1 = gerstnerWave(vec2(1.0, 0.0), waveAmplitude * 1.0, waveFrequency * 1.0, waveSpeed, pos);
            vec3 wave2 = gerstnerWave(vec2(0.0, 1.0), waveAmplitude * 0.5, waveFrequency * 1.7, waveSpeed * 1.1, pos);
            vec3 wave3 = gerstnerWave(vec2(0.7, 0.7), waveAmplitude * 0.25, waveFrequency * 3.2, waveSpeed * 0.8, pos);
            vec3 wave4 = gerstnerWave(vec2(-0.7, 0.7), waveAmplitude * 0.15, waveFrequency * 4.8, waveSpeed * 1.3, pos);
            
            pos += wave1 + wave2 + wave3 + wave4;
            
            // Calculate dynamic normal using partial derivatives
            float h = 0.01; // Small offset for numerical derivative
            vec3 posX = aPos + vec3(h, 0, 0);
            vec3 posZ = aPos + vec3(0, 0, h);
            
            // Calculate heights at offset positions
            vec3 wave1X = gerstnerWave(vec2(1.0, 0.0), waveAmplitude * 1.0, waveFrequency * 1.0, waveSpeed, posX);
            vec3 wave2X = gerstnerWave(vec2(0.0, 1.0), waveAmplitude * 0.5, waveFrequency * 1.7, waveSpeed * 1.1, posX);
            vec3 wave3X = gerstnerWave(vec2(0.7, 0.7), waveAmplitude * 0.25, waveFrequency * 3.2, waveSpeed * 0.8, posX);
            vec3 wave4X = gerstnerWave(vec2(-0.7, 0.7), waveAmplitude * 0.15, waveFrequency * 4.8, waveSpeed * 1.3, posX);
            
            vec3 wave1Z = gerstnerWave(vec2(1.0, 0.0), waveAmplitude * 1.0, waveFrequency * 1.0, waveSpeed, posZ);
            vec3 wave2Z = gerstnerWave(vec2(0.0, 1.0), waveAmplitude * 0.5, waveFrequency * 1.7, waveSpeed * 1.1, posZ);
            vec3 wave3Z = gerstnerWave(vec2(0.7, 0.7), waveAmplitude * 0.25, waveFrequency * 3.2, waveSpeed * 0.8, posZ);
            vec3 wave4Z = gerstnerWave(vec2(-0.7, 0.7), waveAmplitude * 0.15, waveFrequency * 4.8, waveSpeed * 1.3, posZ);
            
            posX += wave1X + wave2X + wave3X + wave4X;
            posZ += wave1Z + wave2Z + wave3Z + wave4Z;
            
            // Calculate normal from tangent vectors
            vec3 tangentX = normalize(posX - pos);
            vec3 tangentZ = normalize(posZ - pos);
            vec3 normal = normalize(cross(tangentZ, tangentX));
            
            FragPos = vec3(model * vec4(pos, 1.0));
            Normal = mat3(transpose(inverse(model))) * normal;
            TexCoord = aTexCoord;
            ViewPos = vec3(inverse(view) * vec4(0.0, 0.0, 0.0, 1.0));
            
            gl_Position = projection * view * vec4(FragPos, 1.0);
        }";

            string fragmentShader = @"
        #version 330 core
        in vec3 FragPos;
        in vec3 Normal;
        in vec2 TexCoord;
        in vec3 ViewPos;
        
        uniform vec4 objectColor;
        uniform vec3 lightPos;
        uniform vec3 lightColor;
        uniform vec3 cameraPos;
        uniform float time;
        
        // Water parameters
        uniform vec3 waterDeepColor;
        uniform vec3 waterShallowColor;
        uniform float waterAlpha;
        uniform float fresnelPower;
        uniform float specularStrength;
        uniform float shininess;
        
        out vec4 FragColor;
        
        void main()
        {
            vec3 norm = normalize(Normal);
            vec3 viewDir = normalize(cameraPos - FragPos);
            vec3 lightDir = normalize(lightPos - FragPos);
            
            // Fresnel effect - more reflection at glancing angles
            float fresnel = pow(1.0 - max(dot(norm, viewDir), 0.0), fresnelPower);
            
            // Diffuse lighting
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 diffuse = diff * lightColor;
            
            // Specular lighting (Blinn-Phong)
            vec3 halfwayDir = normalize(lightDir + viewDir);
            float spec = pow(max(dot(norm, halfwayDir), 0.0), shininess);
            vec3 specular = specularStrength * spec * lightColor;
            
            // Ambient lighting
            vec3 ambient = 0.3 * lightColor;
            
            // Mix deep and shallow water colors based on view angle
            vec3 waterColor = mix(waterDeepColor, waterShallowColor, fresnel);
            
            // Add some color variation based on position and time
            float colorVariation = sin(FragPos.x * 0.1 + time * 0.5) * 0.1 + 
                                 cos(FragPos.z * 0.1 - time * 0.3) * 0.1;
            waterColor += vec3(0.0, colorVariation * 0.1, colorVariation * 0.2);
            
            // Combine lighting
            vec3 lighting = ambient + diffuse;
            vec3 result = lighting * waterColor + specular;
            
            // Enhance edges with fresnel
            result = mix(result, lightColor, fresnel * 0.5);
            
            // Apply transparency with fresnel influence
            float alpha = mix(waterAlpha, 1.0, fresnel * 0.3);
            
            FragColor = vec4(result, alpha);
        }";

            return new Shader(vertexShader, fragmentShader);
        }

        public static Shader CreateWaveShader()
        {
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec3 aNormal;
                layout (location = 2) in vec2 aTexCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                uniform float time;
                uniform float waveAmplitude;
                uniform float waveFrequency;

                out vec3 FragPos;
                out vec3 Normal;

                void main()
                {
                    vec3 pos = aPos;
                    pos.y += sin(pos.x * waveFrequency + time) * waveAmplitude;
                    pos.y += cos(pos.z * waveFrequency + time) * waveAmplitude * 0.5;
                    
                    FragPos = vec3(model * vec4(pos, 1.0));
                    Normal = mat3(transpose(inverse(model))) * aNormal;
                    gl_Position = projection * view * vec4(FragPos, 1.0);
                }";

            string fragmentShader = @"
                #version 330 core
                in vec3 FragPos;
                in vec3 Normal;

                uniform vec4 objectColor;
                uniform vec3 lightPos;
                uniform vec3 lightColor;
                uniform vec3 cameraPos;

                out vec4 FragColor;

                void main()
                {
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float diff = max(dot(norm, lightDir), 0.0);
                    vec3 diffuse = diff * lightColor;
                    
                    vec3 ambient = 0.2 * lightColor;
                    vec3 result = (ambient + diffuse) * objectColor.rgb;
                    
                    FragColor = vec4(result, objectColor.a);
                }";

            return new Shader(vertexShader, fragmentShader);
        }
    }
}
