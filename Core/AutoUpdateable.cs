// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

namespace EconSim.Core
{
    /// <summary>
    /// IUpdateable that automatically adds itself to the updateables list
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