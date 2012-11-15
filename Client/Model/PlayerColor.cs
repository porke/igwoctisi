namespace Client.Model
{
    using System;
	using System.Runtime.Serialization;
    using Microsoft.Xna.Framework;

    [DataContract]
    [Serializable]
    public class PlayerColor
    {
        [DataMember]
        public int ColorId { get; set; }

        [DataMember]
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                int r = (byte)((int)_value >> 16);
                int g = (byte)((int)_value >> 8);
                int b = (byte)((int)_value >> 0);
                int transparency = (byte)((int)_value >> 24);
                int alpha = 0xFF - transparency;
                XnaColor = Color.FromNonPremultiplied(r, g, b, alpha);
            }
        }
        private int _value;

        public Color XnaColor { get; private set; }

        public PlayerColor(int colorId, int value)
        {
            Value = value;
            ColorId = colorId;
        }
    }
}
