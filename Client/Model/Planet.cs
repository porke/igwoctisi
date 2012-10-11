namespace Client.Model
{
    using Microsoft.Xna.Framework;

    class Planet
    {
        public string Name { get; set; }
        public int BaseFleetCountPerTurn { get; set; }
        public int Id { get; set; }
        public Vector3 Position { get; set; }
    }
}
