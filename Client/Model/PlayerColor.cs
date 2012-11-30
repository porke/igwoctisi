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
        public uint Value
        {
            get { return _value; }
            set
            {
                _value = value;
                int r = (byte)((int)_value >> 16);
                int g = (byte)((int)_value >> 8);
                int b = (byte)((int)_value >> 0);
                int opacity = (byte)((int)_value >> 24);
                XnaColor = Color.FromNonPremultiplied(r, g, b, opacity);
            }
        }
        private uint _value;

        public Color XnaColor { get; private set; }

        public PlayerColor(int colorId, uint value)
        {
            Value = value;
            ColorId = colorId;
        }
    }
}
