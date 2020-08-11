// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

namespace RaymarchEngine.Core
{
    /// <summary>
    /// IUpdateable that automatically adds itself to the updateables list on creation
    /// </summary>
    public class AutoUpdateable : IUpdateable
    {
        public AutoUpdateable()
        {
            StaticUpdater.Add(this);
        }

        public virtual void Start(int startTime)
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void End(int endTime)
        {
        }
    }
}