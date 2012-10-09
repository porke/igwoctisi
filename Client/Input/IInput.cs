namespace Client.Input
{
    public interface IInput
    {
        void Initialize(Client client);
        void Release();
        void Update(double delta, double time);
    }
}
