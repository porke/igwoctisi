﻿namespace Client
{
    using System;
    using Input;
    using Microsoft.Xna.Framework;
    using Network;
    using Nuclex.UserInterface.Visuals;
    using Nuclex.UserInterface.Visuals.Flat;
    using Renderer;
    using State;
	using Client.View;

    public abstract class GameClient : Game
    {
        public GraphicsDeviceManager GraphicsManager { get; protected set; }
        public IRenderer Renderer { get; protected set; }
        public INetwork Network { get; protected set; }
        public IInput Input { get; protected set; }
        public IGuiVisualizer Visualizer { get; protected set; }
		public ViewManager ViewMgr { get; protected set; }
        public GameState State { get; protected set; }

        protected GameClient()
        {
            GraphicsManager = new GraphicsDeviceManager(this);

            Renderer = new XnaRenderer();
            Network = new WsaNetwork();
            Input = new XnaInput();
			ViewMgr = new ViewManager(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

            Renderer.Initialize(this);
            Network.Initialize(this);
            Input.Initialize(this);
            Visualizer = FlatGuiVisualizer.FromFile(Services, "Content\\Skin\\SuaveSkin.xml");
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            Input.Release();
            Network.Release();
            Renderer.Release();

            base.OnExiting(sender, args);
        }

        protected override void Update(GameTime gameTime)
        {
            var delta = gameTime.ElapsedGameTime.TotalSeconds;
            var time = gameTime.TotalGameTime.TotalSeconds;

            Input.Update(delta, time);
            Network.Update(delta, time);
			ViewMgr.Update(delta, time);

            State.OnUpdate(delta, time);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var delta = gameTime.ElapsedGameTime.TotalSeconds;
            var time = gameTime.TotalGameTime.TotalSeconds;

            State.BeforeDraw(delta, time);
			ViewMgr.Draw(delta, time);
            base.Draw(gameTime);
        }

        public void ChangeState(GameState newState)
        {
            if (State != null)
                State.OnExit();

            State = newState;
            
            if (State != null)
                State.OnEnter();
        }
    }
}
