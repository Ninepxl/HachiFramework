namespace HachiFramework
{
    public interface IMessagePublisher<T>
    {
        void Publish(T message);
    }
}
