using System.Runtime.CompilerServices;

namespace HachiFramework
{
    /// <summary>
    /// 消息处理器基类
    /// </summary>
    public abstract class MessageHandler<T> : MessageHandlerNode<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Handle(T message)
        {
            HandleCore(message);
        }

        protected virtual void HandleCore(T message)
        {
        }
    }
}
