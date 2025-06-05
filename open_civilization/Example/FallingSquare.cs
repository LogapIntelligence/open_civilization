using open_civilization.Components;
using open_civilization.core;
using open_civilization.Core;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace open_civilization.Example
{
    public class FallingSquare : Engine
    {
        private bool _squaresSpawned = false;
        
        public FallingSquare() : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "OpenTK Game Engine - Falling Squares"
        })
        {
        }
        
        protected override void InitializeGame()
        {
            // Create static ground only
            var ground = new Square2D(
                new Vector2(0, -1),          // Position
                new Vector2(10, 0.5f),       // Size
                new Vector4(0.5f, 0.5f, 0.5f, 1.0f)
            );
            ground.Physics.IsStatic = true;
            ground.Physics.Restitution = 0.8f;
            AddGameObject(ground);
            
            // Set gravity
            _physics2D.Gravity = new Vector2(0, -9.81f);
        }
        
        protected override void UpdateGame(float deltaTime)
        {
            // Spawn the 5 squares when W is pressed
            if (_input.IsKeyDown(Keys.W) && !_squaresSpawned)
            {
                SpawnFallingSquares();
                _squaresSpawned = true;
            }
        }
        
        private void SpawnFallingSquares()
        {
            // Create the same 5 falling squares from the original
            for (int i = 0; i < 5; i++)
            {
                var square = new Square2D(
                    new Vector2(i * 1.5f - 3, i + 2),    // Position
                    new Vector2(0.4f, 0.4f),              // Size
                    new Vector4(1.0f, 0.5f, 0.2f, 1.0f),
                    mass: 1.0f + i * 0.5f
                );
                square.Physics.Restitution = 0.6f + i * 0.08f;
                AddGameObject(square);
            }
        }
    }
    
    public class Square2D : GameObject
    {
        private Vector2 _size;
        private Vector4 _color;
        private Physics2DComponent _physics;
        
        public Vector2 Position => _physics?.Position ?? Vector2.Zero;
        public Vector2 Size => _size;
        public Vector4 Color => _color;
        
        public Square2D(Vector2 position, Vector2 size, Vector4 color, float mass = 1.0f)
        {
            _size = size;
            _color = color;
            
            // Add physics component
            _physics = new Physics2DComponent(position, mass)
            {
                Size = size
            };
            AddComponent(_physics);
        }
        
        public override void Render(Renderer renderer)
        {
            // Create a transform matrix for the square's position
            // Convert 2D position to 3D (Z = 0)
            Matrix4 modelMatrix = Matrix4.CreateScale(new Vector3(_size.X, _size.Y, 1.0f)) *
                                  Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, 0.0f));
            
            // Draw the square using the renderer's DrawQuad method
            renderer.DrawQuad(modelMatrix, Vector2.One, new Color4(_color.X, _color.Y, _color.Z, _color.W));
        }
        
        public Physics2DComponent Physics => _physics;
    }
}