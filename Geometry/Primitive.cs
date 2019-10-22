// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using System.Drawing;
using System.Numerics;

namespace EconSim.Geometry
{
    public static class Primitive
    {
        /// <summary>
        /// Returns vertices for a single equilateral unit triangle
        /// </summary>
        /// <returns></returns>
        public static RenderVertex[] Triangle()
        {
            return new[]{
                new RenderVertex(new Vector3(-0.5f, 0.5f, 0.0f), new Vector2(0, 1), Color.Red, Vector3.UnitY),
                new RenderVertex(new Vector3(0.5f, 0.5f, 0.0f), new Vector2(1, 1), Color.Red, Vector3.UnitY),
                new RenderVertex(new Vector3(0.0f, -0.5f, 0.0f), new Vector2(0.5f, 0), Color.Red, Vector3.UnitY)
            };
        }

        /// <summary>
        /// Returns vertices for a unit plane centered at (0, 0)
        /// </summary>
        /// <returns></returns>
        public static RenderVertex[] Plane()
        {
            return new[]{
                new RenderVertex(new Vector3(-0.5f, 0.5f, 0.0f), new Vector2(0, 1), Color.Red, -Vector3.UnitZ),
                new RenderVertex(new Vector3(0.5f, 0.5f, 0.0f), new Vector2(1, 1), Color.Red, -Vector3.UnitZ),
                new RenderVertex(new Vector3(0.5f, -0.5f, 0.0f), new Vector2(1, 0), Color.Red, -Vector3.UnitZ),

                new RenderVertex(new Vector3(-0.5f, 0.5f, 0.0f), new Vector2(0, 1), Color.Red, -Vector3.UnitZ),
                new RenderVertex(new Vector3(0.5f, -0.5f, 0.0f), new Vector2(1, 0), Color.Red, -Vector3.UnitZ),
                new RenderVertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0, 0), Color.Red, -Vector3.UnitZ)
            };
        }
    }
}