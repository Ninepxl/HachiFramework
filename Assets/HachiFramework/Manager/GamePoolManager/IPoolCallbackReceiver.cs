namespace HachiFramework
{
    public interface IPoolCallbackReceiver
    {
        void OnRent();
        void OnReturn();
    }
}