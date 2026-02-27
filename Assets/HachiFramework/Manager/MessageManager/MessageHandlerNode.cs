using System;
using System.Threading;
using UnityEngine;

namespace HachiFramework
{
    /// <summary>
    /// 侵入式链表中的节点基类
    /// </summary>
    public abstract class MessageHandlerNode<T> : IDisposable
    {
        public MessageHandlerList<T> Parent = default!;
        public MessageHandlerNode<T> PreviousNode;
        public MessageHandlerNode<T> NextNode;
        public ulong Version;
        private bool disposed;
        public bool Disposed => disposed;

        public void Dispose()
        {
            if (disposed) return;
            if (Parent != null)
            {
                Parent.Remove(this);
                Volatile.Write(ref Parent!, null);
            }

            Volatile.Write(ref disposed, true);
            DisposeCore();
        }

        protected virtual void DisposeCore()
        {
        }
    }
}
