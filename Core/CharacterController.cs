using RaymarchEngine.Physics;

namespace RaymarchEngine.Core
{
    public class CharacterController : IComponent
    {
        private PrimitivePhysics physics;

        public void OnAddedToGameObject(GameObject gameObject)
        {
            physics = gameObject.GetComponent<PrimitivePhysics>();
        }

        public void Start(int startTime)
        {
        }

        public void Update(float deltaTime)
        {
        }

        public void End(int endTime)
        {
        }
    }
}