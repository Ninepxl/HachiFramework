using System;
using System.Runtime.CompilerServices;

namespace HachiFramework
{
    /// <summary>
    /// 消息代理，管理某一类型消息的订阅和发布
    /// 使用侵入式双向链表存储订阅者
    /// 通过泛型静态单例 Default 提供零开销的全局访问
    /// </summary>
    public class MessageBroker<T> : IMessageSubscriber<T>, IMessagePublisher<T>, IDisposable
    {
        /// <summary>
        /// 全局默认实例，每个消息类型 T 自动拥有独立的 Broker
        /// 利用 C# 泛型静态字段天然按类型隔离，无需 Dictionary 查找
        /// </summary>
        public static readonly MessageBroker<T> Default = new();

        object gate;
        readonly MessageHandlerList<T> syncHandlers;
        bool isDisposed;

        public bool IsDisposed => isDisposed;

        public MessageBroker()
        {
            gate = new();
            syncHandlers = new(gate);
        }

        public void Publish(T message)
        {
            var node = syncHandlers.Root;
            var version = syncHandlers.GetVersion();

            while (node != null)
            {
                if (node.Version > version) break;
                ((MessageHandler<T>)node).Handle(message);
                node = node.NextNode;
            }
        }

        public IDisposable Subscribe(MessageHandler<T> handler)
        {
            return SubscribeCore(handler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Subscribe(Action<T> handler)
        {
            return SubscribeCore(new AnonymousMessageHandler<T>(handler));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        MessageHandler<T> SubscribeCore(MessageHandler<T> handler)
        {
            syncHandlers.Add(handler);
            return handler;
        }

        public void Dispose()
        {
            lock (gate)
            {
                isDisposed = true;
            }
            syncHandlers.Dispose();
        }
    }
}
