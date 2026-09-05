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
    /// <typeparam name="T">Primitive shape to draw, which picks the shader's distance function</typeparam>
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
        /// Packs this frame's transform and material into the layout the shader expects
        /// </summary>
        /// <returns>One element of the structured buffer for this primitive type</returns>
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
        /// <summary>
        /// Surface parameters for this primitive
        /// </summary>
        public MaterialBufferData material;

        /// <summary>
        /// Shape specific parameters, unused by shapes that only need a scale
        /// </summary>
        public Vector4 primitiveOptions;

        /// <summary>
        /// World space position
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Pads position out to 16 bytes
        /// </summary>
        public float positionPadding;

        /// <summary>
        /// World space rotation as a quaternion, x y z w
        /// </summary>
        public Vector4 rotation;

        /// <summary>
        /// Scale along each axis, read differently by each distance function
        /// </summary>
        public Vector3 scale;

        /// <summary>
        /// Pads scale out to 16 bytes
        /// </summary>
        public float scalePadding;

        /// <summary>
        /// Creates one buffer element. The padding fields are left at zero.
        /// </summary>
        /// <param name="material">Surface parameters</param>
        /// <param name="primitiveOptions">Shape specific parameters</param>
        /// <param name="position">World space position</param>
        /// <param name="rotation">World space rotation as a quaternion, x y z w</param>
        /// <param name="scale">Scale along each axis</param>
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
        /// <summary>
        /// Diffuse colour
        /// </summary>
        public Vector3 color;

        /// <summary>
        /// Phong specular exponent
        /// </summary>
        public float shininess;

        /// <summary>
        /// Strength of the specular highlight
        /// </summary>
        public float specularStrength;

        /// <summary>
        /// How much of the surroundings the surface reflects, 0 to 1
        /// </summary>
        public float diffraction;

        /// <summary>
        /// World space size of one checkerboard square, or 0 for a plain surface
        /// </summary>
        public float checkerSize;

        /// <summary>
        /// Pads the struct out to a multiple of 16 bytes
        /// </summary>
        public float padding;

        /// <summary>
        /// Creates the material block. The padding field is left at zero.
        /// </summary>
        /// <param name="color">Diffuse colour</param>
        /// <param name="shininess">Phong specular exponent</param>
        /// <param name="specularStrength">Strength of the specular highlight</param>
        /// <param name="diffraction">Reflectivity, 0 to 1</param>
        /// <param name="checkerSize">Size of one checkerboard square, or 0 for a plain surface</param>
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
