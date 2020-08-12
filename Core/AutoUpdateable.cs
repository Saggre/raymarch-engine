// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

namespace RaymarchEngine.Core
{
    /// <summary>
    /// IUpdateable that automatically adds itself to the updateables list on creation
    /// </summary>
    public abstract class AutoUpdateable : IUpdateable
    {
        /// <summary>
        /// Automatically adds this updateable to a list to be updated
        /// </summary>
        protected AutoUpdateable()
        {
            StaticUpdater.Add(this);
        }

        /// <inheritdoc />
        public void OnAddedToGameObject(GameObject parent)
        {
        }

        /// <inheritdoc />
        public abstract void Start(int startTime);

        /// <inheritdoc />
        public abstract void Update(float deltaTime);

        /// <inheritdoc />
        public abstract void End(int endTime);
    }
}