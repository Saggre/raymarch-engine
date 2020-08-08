// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System.Collections.Generic;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace EconSim.Core
{

    /// <summary>
    /// A class that represents a physical, visible object in the scene
    /// </summary>
    public class GameObject : Object
    {
        private Mesh mesh;
        private SharedShader shader;

        public GameObject()
        {
            active = true;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
            updateables = new List<IUpdateable>();
        }

        public GameObject(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            active = true;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            updateables = new List<IUpdateable>();
        }

        public GameObject(Vector3 position, Quaternion rotation, Vector3 scale, Mesh mesh)
        {
            active = true;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.mesh = mesh;
            updateables = new List<IUpdateable>();
        }

        public GameObject(Mesh mesh)
        {
            active = true;
            this.mesh = mesh;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
            updateables = new List<IUpdateable>();
        }
        public SharedShader Shader
        {
            get => shader;
            set => shader = value;
        }

        public Mesh Mesh
        {
            get => mesh;
            set => mesh = value;
        }

    }
}