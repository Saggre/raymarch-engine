using BepuPhysics.Collidables;

namespace RaymarchEngine.Physics
{
    public interface IPhysicsObject
    {
        /// <summary>
        /// Gets the type of Bepuphysics collider shape
        /// </summary>
        /// <returns></returns>
        IConvexShape GetColliderShape();
    }
}