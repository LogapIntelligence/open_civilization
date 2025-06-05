using OpenTK.Mathematics;

namespace open_civilization.Components
{
    public class Physics2DSystem
    {
        private List<Physics2DComponent> _physicsComponents;
        private Vector2 _gravity = new Vector2(0, -9.81f);
        private float _fixedTimeStep = 1f / 60f;
        private float _accumulator = 0f;

        public Vector2 Gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        public Physics2DSystem()
        {
            _physicsComponents = new List<Physics2DComponent>();
        }

        public void AddComponent(Physics2DComponent component)
        {
            if (!_physicsComponents.Contains(component))
            {
                _physicsComponents.Add(component);
            }
        }

        public void RemoveComponent(Physics2DComponent component)
        {
            _physicsComponents.Remove(component);
        }

        public void Update(float deltaTime)
        {
            _accumulator += deltaTime;

            while (_accumulator >= _fixedTimeStep)
            {
                FixedUpdate(_fixedTimeStep);
                _accumulator -= _fixedTimeStep;
            }
        }

        private void FixedUpdate(float fixedDeltaTime)
        {
            // Apply forces and update physics
            foreach (var physics in _physicsComponents)
            {
                if (!physics.Enabled || physics.IsStatic) continue;

                // Reset acceleration
                physics.Acceleration = Vector2.Zero;

                // Apply gravity
                if (physics.UseGravity)
                {
                    physics.Force += _gravity * physics.Mass;
                }

                // Calculate acceleration from forces (F = ma, so a = F/m)
                if (physics.Mass > 0)
                {
                    physics.Acceleration = physics.Force / physics.Mass;
                }

                // Update velocity
                physics.Velocity += physics.Acceleration * fixedDeltaTime;

                // Apply damping
                physics.Velocity *= (1f - physics.LinearDamping);

                // Update position
                Vector2 newPosition = physics.Position + physics.Velocity * fixedDeltaTime;

                // Apply position constraints
                if (!physics.FreezePositionX)
                    physics.Position = new Vector2(newPosition.X, physics.Position.Y);
                if (!physics.FreezePositionY)
                    physics.Position = new Vector2(physics.Position.X, newPosition.Y);

                // Clear forces for next frame
                physics.Force = Vector2.Zero;
            }

            // Handle collisions
            HandleCollisions();
        }

        private void HandleCollisions()
        {
            for (int i = 0; i < _physicsComponents.Count; i++)
            {
                for (int j = i + 1; j < _physicsComponents.Count; j++)
                {
                    var a = _physicsComponents[i];
                    var b = _physicsComponents[j];

                    if (!a.Enabled || !b.Enabled) continue;
                    if (a.IsTrigger || b.IsTrigger) continue; // Skip trigger collisions for now
                    if (a.IsStatic && b.IsStatic) continue;

                    if (CheckAABBCollision(a, b))
                    {
                        ResolveCollision(a, b);
                    }
                }
            }
        }

        private bool CheckAABBCollision(Physics2DComponent a, Physics2DComponent b)
        {
            var boundsA = a.GetBounds();
            var boundsB = b.GetBounds();

            return boundsA.Min.X < boundsB.Max.X &&
                   boundsA.Max.X > boundsB.Min.X &&
                   boundsA.Min.Y < boundsB.Max.Y &&
                   boundsA.Max.Y > boundsB.Min.Y;
        }

        private void ResolveCollision(Physics2DComponent a, Physics2DComponent b)
        {
            var boundsA = a.GetBounds();
            var boundsB = b.GetBounds();

            // Calculate overlap
            float overlapX = Math.Min(boundsA.Max.X - boundsB.Min.X, boundsB.Max.X - boundsA.Min.X);
            float overlapY = Math.Min(boundsA.Max.Y - boundsB.Min.Y, boundsB.Max.Y - boundsA.Min.Y);

            Vector2 normal;
            float penetration;

            if (overlapX < overlapY)
            {
                penetration = overlapX;
                normal = a.Position.X < b.Position.X ? new Vector2(-1, 0) : new Vector2(1, 0);
            }
            else
            {
                penetration = overlapY;
                normal = a.Position.Y < b.Position.Y ? new Vector2(0, -1) : new Vector2(0, 1);
            }

            if (!a.IsStatic && !b.IsStatic)
            {
                float totalMass = a.Mass + b.Mass;
                float pushA = b.Mass / totalMass;
                float pushB = a.Mass / totalMass;
                a.Position += normal * penetration * pushA;
                b.Position -= normal * penetration * pushB;
            }
            else if (!a.IsStatic)
            {
                a.Position += normal * penetration;
            }
            else if (!b.IsStatic)
            {
                b.Position -= normal * penetration;
            }

            // Calculate relative velocity
            Vector2 relativeVelocity = a.Velocity - b.Velocity;
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

            // Don't resolve if velocities are separating
            if (velocityAlongNormal > 0) return;

            // Calculate restitution
            float e = Math.Min(a.Restitution, b.Restitution);

            // Calculate impulse scalar
            float j = -(1 + e) * velocityAlongNormal;

            // Handle static objects properly - they have infinite mass (1/mass = 0)
            float invMassA = a.IsStatic ? 0 : 1 / a.Mass;
            float invMassB = b.IsStatic ? 0 : 1 / b.Mass;
            j /= invMassA + invMassB;

            // Apply impulse
            Vector2 impulse = j * normal;
            if (!a.IsStatic)
                a.Velocity += impulse * invMassA;
            if (!b.IsStatic)
                b.Velocity -= impulse * invMassB;

            // Apply friction
            Vector2 tangent = relativeVelocity - velocityAlongNormal * normal;
            if (tangent.LengthSquared > 0.0001f)
            {
                tangent = tangent.Normalized();
                float frictionImpulse = -Vector2.Dot(relativeVelocity, tangent);
                float mu = (a.Friction + b.Friction) * 0.5f;
                frictionImpulse = Math.Max(-j * mu, Math.Min(frictionImpulse, j * mu));

                Vector2 frictionVector = frictionImpulse * tangent;
                if (!a.IsStatic)
                    a.Velocity += frictionVector * invMassA * a.Mass;
                if (!b.IsStatic)
                    b.Velocity -= frictionVector * invMassB * b.Mass;
            }
        }
    }
}