namespace Client
{
    using View;
    using State;
    using Client.Model;

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
            Content.RootDirectory = "Content";
            GraphicsManager.PreferredBackBufferWidth = 800;
            GraphicsManager.PreferredBackBufferHeight = 600;
            IsMouseVisible = true;
        }
    }
}
    