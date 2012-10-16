namespace Client.Renderer
{
    using Microsoft.Xna.Framework.Graphics;

    public class PlanetVisual
    {
        public float Period { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public Texture2D DiffuseTexture { get; set; }
        public Texture2D CloudsTexture { get; set; }
        public Texture2D CloudsAlphaTexture { get; set; }
    }
}
