namespace Client.View
{
    using Input;

    public interface IView
    {
        bool IsLoaded { get; }
        bool IsTransparent { get; }
        IInputReceiver InputReceiver { get; }

        void OnShow(ViewManager viewMgr, double time);
        void OnHide(double time);
        void Update(double delta, double time);
        void Draw(double delta, double time);
    }
}
