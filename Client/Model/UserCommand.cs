namespace Client.Model
{
    public class UserCommand
    {
        public Player Player { get; private set; }
        public Planet Source { get; private set; }
        public Planet Target { get; private set; }
        public int UnitCount { get; private set; }

        public UserCommand(Player player, Planet source, Planet target)
        {
            Player = player;
            Source = source;
            Target = target;
            UnitCount = 1;
        }

        public void SubtractUnit()
        {
            if (UnitCount == 0) return;

            // TODO: temp unit subtraction implementation
            --UnitCount;
        }
    }
}
