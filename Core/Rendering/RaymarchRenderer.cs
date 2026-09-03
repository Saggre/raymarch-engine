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

        private Vector4 GetOptions()
        {
            return new Vector4(parent.Movement.Scale.MinComponent(), 0f, 0f, 0f);
        }

        /// <summary>
        /// Get data needed to render this shape
        /// </summary>
        /// <returns></returns>
        public PrimitiveBufferData GetBufferData()
        {
            return new PrimitiveBufferData(
                new MaterialBufferData(Color, Shininess, SpecularStrength, Diffraction),
                GetOptions(),
                parent.Movement.Position,
                parent.Movement.Rotation.QuaternionToEuler(),
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
    /// Euler angles are passed instead of quaternion rotation, because the object can't be/shouldn't be rotated in the shader. Euler angle data will suffice for rendering the shape at different rotations.
    /// Vectors are padded to 16 byte boundaries so the C# and HLSL layouts agree. Change both.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PrimitiveBufferData
    {
        public MaterialBufferData material;
        public Vector4 primitiveOptions;
        public Vector3 position;
        public float positionPadding;
        public Vector3 eulerAngles;
        public float eulerAnglesPadding;
        public Vector3 scale;
        public float scalePadding;

        public PrimitiveBufferData(
            MaterialBufferData material,
            Vector4 primitiveOptions,
            Vector3 position,
            Vector3 eulerAngles,
            Vector3 scale
        ) : this()
        {
            this.material = material;
            this.primitiveOptions = primitiveOptions;
            this.position = position;
            this.eulerAngles = eulerAngles;
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
        public Vector2 padding;

        public MaterialBufferData(Vector3 color, float shininess, float specularStrength, float diffraction)
            : this()
        {
            this.color = color;
            this.shininess = shininess;
            this.specularStrength = specularStrength;
            this.diffraction = diffraction;
        }
    }
}
