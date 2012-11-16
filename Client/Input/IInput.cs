namespace Client.Input
{
    public interface IInput
    {
        void Initialize(GameClient client);
        void Release();
        void Update(double delta, double time);
    }
}
