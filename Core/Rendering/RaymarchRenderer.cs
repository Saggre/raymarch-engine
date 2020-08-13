using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using RaymarchEngine.Core.Primitives;
using RaymarchEngine.EMath;
using Plane = RaymarchEngine.Core.Primitives.Plane;

namespace RaymarchEngine.Core.Rendering
{
    /// <summary>
    /// Keeps track of raymarched objects in game
    /// </summary>
    public static class RaymarchRenderer
    {
        /// <summary>
        /// How many primitives are allowed in the game
        /// </summary>
        private static Dictionary<Type, int> primitiveCounts;

        /// <summary>
        /// Init
        /// </summary>
        public static void Init()
        {
            primitiveCounts = new Dictionary<Type, int>
            {
                {typeof(Sphere), 0},
                {typeof(Box), 0},
                {typeof(Plane), 0},
                //{typeof(Ellipsoid), 32},
                //{typeof(Torus), 32},
                //{typeof(CappedTorus), 32}
            };
        }

        /// <summary>
        /// Get the number of primitives by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int PrimitiveCount<T>() where T : IPrimitive
        {
            return primitiveCounts[typeof(T)];
        }

        /// <summary>
        /// Increment number of recorded primitives
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void AddPrimitiveCount<T>() where T : IPrimitive
        {
            primitiveCounts[typeof(T)]++;
        }
    }

    /// <summary>
    /// Attached to a gameobject to enable raymarch rendering
    /// </summary>
    public class RaymarchRenderer<T> : IComponent where T : IPrimitive
    {
        private IPrimitive primitive;
        private Type primitiveType;
        private GameObject parent;

        /// <summary>
        /// Create new raymarch renderer component
        /// </summary>
        public RaymarchRenderer()
        {
        }

        /// <inheritdoc />
        public void OnAddedToGameObject(GameObject parent)
        {
            if (Engine.RenderDevice != null)
            {
                throw new InvalidOperationException("RaymarchRenderer can only be added in Start()");
            }

            RaymarchRenderer.AddPrimitiveCount<T>();
            this.parent = parent;
            primitiveType = typeof(T);
        }

        private Vector4 GetOptions()
        {
            return new Vector4(parent.Scale.MinComponent(), 0f, 0f, 0f);
            return Vector4.Zero;
        }

        /// <summary>
        /// Get data needed to render this shape
        /// </summary>
        /// <returns></returns>
        public PrimitiveBufferData GetBufferData()
        {
            return new PrimitiveBufferData(
                GetOptions(),
                parent.Position,
                parent.Rotation.QuaternionToEuler(),
                parent.Scale
            );
        }

        /// <inheritdoc />
        public void Start(int startTime)
        {
        }

        /// <inheritdoc />
        public void Update(float deltaTime)
        {
        }

        /// <inheritdoc />
        public void End(int endTime)
        {
        }
    }

    /// <summary>
    /// Data that is passed to the raymarch shader.
    /// Euler angles is passed instead of quaternion rotation, because the object can't be/shouldn't be rotated in the shader. Euler angle data will suffice for rendering the shape at different rotations.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PrimitiveBufferData
    {
        public Vector4 primitiveOptions;
        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;
        public Vector4 color;
        public Vector2 materialOptions;

        public PrimitiveBufferData(
            Vector4 primitiveOptions,
            Vector3 position,
            Vector3 eulerAngles,
            Vector3 scale
        )
        {
            this.primitiveOptions = primitiveOptions;
            this.position = position;
            this.eulerAngles = eulerAngles;
            this.scale = scale;
            color = Vector4.One;
            materialOptions = Vector2.Zero;
        }
    }
}