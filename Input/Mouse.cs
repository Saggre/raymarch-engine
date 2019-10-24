// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;
using EconSim.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IUpdateable = EconSim.Core.IUpdateable;

namespace EconSim.Input
{
  using GameMouse = Microsoft.Xna.Framework.Input.Mouse;

  public class Mouse : IUpdateable
  {
    /// <summary>
    /// FPS or menu mouse
    /// </summary>
    public enum MouseMode
    {
      Infinite = 0,
      Constrained = 1
    }

    private Vector2 position;
    private Vector2 deltaPosition;
    public MouseMode mouseMode;
    private readonly int screenX;
    private readonly int screenY;
    private MouseState initialMouseState;

    public Mouse()
    {
      StaticUpdater.Add(this);
      mouseMode = MouseMode.Infinite;
      screenX = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
      screenY = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

      SetCursorPosition(screenX / 2, screenY / 2);
      initialMouseState = GameMouse.GetState();
    }

    public Vector2 DeltaPosition => deltaPosition;

    /// <summary>
    /// Current cursor position
    /// </summary>
    public Vector2 Position => position;

    /// <summary>
    /// Set cursor to a certain pixel on screen
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SetCursorPosition(int x, int y)
    {
      GameMouse.SetPosition(x, y);
    }

    /// <summary>
    /// Set cursor to a point on screen between [0,1]
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SetCursorPositionRelative(float x, float y)
    {
      GameMouse.SetPosition((int)(screenX * x), (int)(screenY * y));
    }

    public void Start()
    {

    }

    public void Update()
    {
      var mouseState = GameMouse.GetState();
      if (mouseState != initialMouseState)
      {
        position.X = mouseState.X;
        position.Y = mouseState.Y;

        deltaPosition.X = initialMouseState.X - mouseState.X;
        deltaPosition.Y = initialMouseState.Y - mouseState.Y;

        SetCursorPosition(initialMouseState.X, initialMouseState.Y);
      }
      else
      {
        deltaPosition.X = 0;
        deltaPosition.Y = 0;
      }
    }

    public void End()
    {

    }
  }
}