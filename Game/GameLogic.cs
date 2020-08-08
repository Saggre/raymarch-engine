﻿// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System;
using System.Collections.Generic;
using System.Numerics;
using EconSim.Collections;
using EconSim.Core;
using EconSim.Core.Input;
using EconSim.Geometry;
using EconSim.Terrain;
using EconSim.EMath;
using SharpDX.Direct3D11;
using System.Diagnostics;

namespace EconSim.Game
{
    /// <summary>
    /// Main class for the game logic separated from the engine itself
    ///
    /// Creates enough tile objects to satisfy the render distance, and updates them with new terrain maps when moving
    /// </summary>
    public class GameLogic : AutoUpdateable
    {
        struct ShaderBuffer
        {
            public Vector3 cameraPosition;
            public float aspectRatio;
            public Vector4 cameraDirection;
        }

        private ShaderBuffer raymarchShaderBuffer; // Values to send to the raymarch shader

        private Camera camera;
        private Vector2 lookVector;
        private PlayerMovement playerMovement;

        private GameObject plane;

        public override void Start(int startTime)
        {
            // Init movement manager
            playerMovement = new PlayerMovement();

            // Set camera initial pos
            camera = new Camera();
            camera.Position = new Vector3(0, 0, 0);
            Engine.CurrentScene.ActiveCamera = camera; // TODO more elegantly
            lookVector = new Vector2(0, 140);

            SharedShader shader = SharedShader.CompileFromFiles(@"Shaders\Raymarch");

            plane = new GameObject(Mesh.CreateQuad())
            {
                Shader = shader
            };

            Engine.CurrentScene.AddGameObject(plane);
        }

        private void CameraLook(float deltaTime)
        {
            float sensitivity = 0.03f;

            // Move camera
            camera.Move(playerMovement.MovementInput.Multiply(Engine.CurrentScene.ActiveCamera.Rotation), deltaTime);

            // Rotate camera
            // 180, 171
            lookVector.X += InputDevice.Mouse.DeltaPosition.X * sensitivity;
            lookVector.Y -= InputDevice.Mouse.DeltaPosition.Y * sensitivity;

            if (lookVector.Y < 100)
            {
                lookVector.Y = 100;
            }
            else if (lookVector.Y > 260 - float.Epsilon)
            {
                lookVector.Y = 260;
            }

            //camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, lookVector.X * EMath.Util.Deg2Rad) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, lookVector.Y * EMath.Util.Deg2Rad);

        }


        public override void Update(float deltaTime)
        {
            CameraLook(deltaTime);

            plane.Rotation = Quaternion.CreateFromAxisAngle(camera.Right, (float)Math.PI * -0.5f);
            plane.Position = camera.Position + camera.Forward * 2
              - new Vector3(0.5f, 0.5f, 0);

            raymarchShaderBuffer.cameraPosition = camera.Position;
            raymarchShaderBuffer.aspectRatio = Engine.AspectRatio();
            raymarchShaderBuffer.cameraDirection = new Vector4(camera.Forward, 0.0f);

            Console.WriteLine(Engine.AspectRatio());

            plane.Shader.SetConstantBuffer(plane, 1,
                    Shader.CreateSingleElementBuffer(ref raymarchShaderBuffer)
                );
        }

        public override void End(int endTime)
        {

        }
    }
}