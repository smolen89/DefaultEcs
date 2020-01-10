﻿using System;
using System.Collections.Generic;
using DefaultBoids.Component;
using DefaultEcs;
using DefaultEcs.System;
using DefaultEcs.Threading;
using Microsoft.Xna.Framework;

namespace DefaultBoids.System
{
    [With(typeof(DrawInfo), typeof(Acceleration), typeof(Velocity), typeof(Grid))]
    public sealed class BoidsSystem : AEntitySystem<float>
    {
        private readonly float _maxDistance;
        private readonly float _maxDistanceSquared;

        public BoidsSystem(World world, IParallelRunner runner)
            : base(world, runner)
        {
            _maxDistance = DefaultGame.NeighborRange;
            _maxDistanceSquared = MathF.Pow(_maxDistance, 2);
        }

        protected override void Update(float state, ReadOnlySpan<Entity> entities)
        {
            foreach (ref readonly Entity entity in entities)
            {
                Vector2 position = entity.Get<DrawInfo>().Position;
                Vector2 separation = Vector2.Zero;
                Vector2 alignment = Vector2.Zero;
                Vector2 cohesion = Vector2.Zero;
                int neighborCount = 0;

                foreach (List<Entity> neighbors in entity.Get<Grid>().GetEnumerator(position))
                {
                    foreach (Entity neighbor in neighbors)
                    {
                        if (entity == neighbor)
                        {
                            continue;
                        }

                        Vector2 otherPosition = neighbor.Get<DrawInfo>().Position;

                        Vector2 offset = position - otherPosition;

                        if (offset.LengthSquared() < _maxDistanceSquared)
                        {
                            separation += Vector2.Normalize(offset);

                            alignment += neighbor.Get<Velocity>().Value;

                            cohesion += otherPosition;

                            ++neighborCount;
                        }
                    }
                }

                if (neighborCount > 0)
                {
                    alignment = (alignment / neighborCount) - entity.Get<Velocity>().Value;

                    cohesion = position - (cohesion / neighborCount);
                }

                entity.Get<Acceleration>().Value =
                    (separation * DefaultGame.BehaviorSeparationWeight)
                    + (alignment * DefaultGame.BehaviorAlignmentWeight)
                    + (cohesion * DefaultGame.BehaviorCohesionWeight);
            }
        }
    }
}