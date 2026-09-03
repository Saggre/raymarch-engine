using System;
using System.Numerics;
using System.Runtime.InteropServices;
using RaymarchEngine.Core.Primitives;
using RaymarchEngine.EMath;

namespace RaymarchEngine.Core.Rendering
{
    /// <summary>
    /// Attached to a gameobject to enable raymarch rendering
    /// </summary>
    public class RaymarchRenderer<T> : IComponent where T : IPrimitive
    {
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

            this.parent = parent;
        }

        /// <summary>
        /// Diffuse colour of the surface
        /// </summary>
        public Vector3 Color { get; set; } = Vector3.One;

        /// <summary>
        /// Phong specular exponent. Higher values give a tighter highlight.
        /// </summary>
        public float Shininess { get; set; } = 50f;

        /// <summary>
        /// Strength of the specular highlight
        /// </summary>
        public float SpecularStrength { get; set; } = 1f;

        /// <summary>
        /// How much of the surroundings the surface reflects, 0 to 1
        /// </summary>
        public float Diffraction { get; set; }

        /// <summary>
        /// Shape specific parameters for signed distance functions that need more than a scale
        /// </summary>
        public Vector4 Options { get; set; }

        /// <summary>
        /// World space size of one checkerboard square laid over the surface, or 0 for a plain
        /// surface. The pattern is in world space, so it does not travel with the object.
        /// </summary>
        public float CheckerSize { get; set; }

        /// <summary>
        /// Get data needed to render this shape
        /// </summary>
        /// <returns></returns>
        public PrimitiveBufferData GetBufferData()
        {
            Quaternion rotation = parent.Movement.Rotation;

            return new PrimitiveBufferData(
                new MaterialBufferData(Color, Shininess, SpecularStrength, Diffraction, CheckerSize),
                Options,
                parent.Movement.Position,
                new Vector4(rotation.X, rotation.Y, rotation.Z, rotation.W),
                parent.Movement.Scale
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
    /// Data that is passed to the raymarch shader, mirrored by cPrimitiveData in Common.hlsl.
    /// Rotation travels as a quaternion: no euler convention to agree on, and it fits the same
    /// 16 bytes the padded euler triple used.
    /// Vectors are padded to 16 byte boundaries so the C# and HLSL layouts agree. Change both.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PrimitiveBufferData
    {
        public MaterialBufferData material;
        public Vector4 primitiveOptions;
        public Vector3 position;
        public float positionPadding;
        public Vector4 rotation;
        public Vector3 scale;
        public float scalePadding;

        public PrimitiveBufferData(
            MaterialBufferData material,
            Vector4 primitiveOptions,
            Vector3 position,
            Vector4 rotation,
            Vector3 scale
        ) : this()
        {
            this.material = material;
            this.primitiveOptions = primitiveOptions;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    /// <summary>
    /// Surface parameters for a single primitive, mirrored by cMaterialData in Common.hlsl
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialBufferData
    {
        public Vector3 color;
        public float shininess;
        public float specularStrength;
        public float diffraction;
        public float checkerSize;
        public float padding;

        public MaterialBufferData(Vector3 color, float shininess, float specularStrength, float diffraction,
            float checkerSize)
            : this()
        {
            this.color = color;
            this.shininess = shininess;
            this.specularStrength = specularStrength;
            this.diffraction = diffraction;
            this.checkerSize = checkerSize;
        }
    }
}
