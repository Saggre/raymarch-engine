// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

namespace EconSim.Core
{
    public interface IUpdateable
    {
        void Start();
        void Update();
        void End();
    }
}