using System;

namespace HachiFramework
{
    public interface IMessageSubscriber<T>
    {
        IDisposable Subscribe(MessageHandler<T> handler);
    }
}
