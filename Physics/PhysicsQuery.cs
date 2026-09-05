using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;

namespace RaymarchEngine.Physics
{
    /// <summary>
    /// Ray queries against the physics world, for things that ask what is in front of them rather
    /// than get pushed around by the solver.
    /// </summary>
    public static class PhysicsQuery
    {
        /// <summary>
        /// Keeps the nearest hit.
        ///
        /// Narrowing maximumT on every hit is what makes this a nearest query rather than a list
        /// of everything the ray passes through: the traversal obeys the new limit, so branches
        /// further away are skipped instead of tested.
        /// </summary>
        private struct ClosestHit : IRayHitHandler
        {
            public bool Hit;
            public float Distance;
            public Vector3 Normal;

            public bool AllowTest(CollidableReference collidable)
            {
                return true;
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return true;
            }

            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal,
                CollidableReference collidable, int childIndex)
            {
                if (Hit && t >= Distance)
                {
                    return;
                }

                Hit = true;
                Distance = t;
                Normal = normal;

                maximumT = t;
            }
        }

        /// <summary>
        /// Casts a ray and reports the nearest surface it meets.
        ///
        /// The broad phase bounds are refreshed by the timestep, which runs after the components
        /// have updated, so a query sees animated bodies where they were on the previous frame.
        /// Static geometry is exact.
        /// </summary>
        /// <param name="origin">Where the ray starts, in world units</param>
        /// <param name="direction">Which way it points, normalised</param>
        /// <param name="maximumDistance">How far to look, in world units</param>
        /// <param name="distance">How far along the ray the surface was, when one was found</param>
        /// <param name="normal">The surface normal there, when one was found</param>
        /// <returns>True when the ray met something</returns>
        public static bool Raycast(Vector3 origin, Vector3 direction, float maximumDistance,
            out float distance, out Vector3 normal)
        {
            distance = maximumDistance;
            normal = Vector3.UnitY;

            Simulation simulation = PhysicsHandler.Simulation;
            if (simulation == null)
            {
                return false;
            }

            ClosestHit hit = new ClosestHit();
            simulation.RayCast(origin, direction, maximumDistance, ref hit, 0);

            if (!hit.Hit)
            {
                return false;
            }

            distance = hit.Distance;
            normal = hit.Normal;

            return true;
        }
    }
}
