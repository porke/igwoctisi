namespace Client
{
    using State;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System;
    using Client.Common;
    using Nuclex.UserInterface;
	using Microsoft.Xna.Framework.Graphics;

    public class IGWOCTISI : GameClient
    {
        #region Microsoft.Xna.Framework.Game members

        protected override void Initialize()
        {
            base.Initialize();

            ChangeState(new MenuState(this));
        }

        #endregion

        public IGWOCTISI()
        {
            Window.Title = DefaultMainWindowTitle;
            Content.RootDirectory = "Content";
            GraphicsManager.PreferredBackBufferWidth = 1280;
            GraphicsManager.PreferredBackBufferHeight = 720;
			GraphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            IsMouseVisible = true;
        }

        public const string DefaultMainWindowTitle = "IGWOCTISI";
    }
}
    