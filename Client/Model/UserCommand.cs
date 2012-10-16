namespace Client.Model
{
    public class UserCommand
    {
        private Player _player;
        private Planet _source;
        private Planet _target;
        private int _unitCount;

        public UserCommand(Player player, Planet source, Planet target)
        {
            _player = player;
            _source = source;
            _target = target;
            _unitCount = 1;
        }

        public void SubtractUnit()
        {
            if (_unitCount == 0) return;

            // TODO: temp unit subtraction implementation
            --_unitCount;
        }
    }
}
