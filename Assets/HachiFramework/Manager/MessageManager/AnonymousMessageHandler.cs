using System;

namespace HachiFramework
{
    /// <summary>
    /// 匿名消息处理器，包装 Action 委托
    /// </summary>
    public sealed class AnonymousMessageHandler<T> : MessageHandler<T>
    {
        Action<T> handler;

        public AnonymousMessageHandler(Action<T> handler)
        {
            this.handler = handler;
        }

        protected override void HandleCore(T message)
        {
            handler(message);
        }
    }
}
