using System;

namespace HachiFramework
{
    public static class MessageSubscriberExtensions
    {
        public static IDisposable Subscribe<T>(this IMessageSubscriber<T> subscriber, Action<T> handler)
        {
            return subscriber.Subscribe(new AnonymousMessageHandler<T>(handler));
        }
    }
}
