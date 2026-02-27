using UnityEngine;

namespace HachiFramework
{
    /// <summary>
    /// 消息管理器
    /// 不负责 Broker 的创建和查找（由泛型静态单例 MessageBroker&lt;T&gt;.Default 处理）
    /// 仅作为 V1 框架 Manager 体系的一部分，保持架构一致性
    /// 
    /// 使用方式：
    ///   MessageBroker&lt;MyMessage&gt;.Default.Publish(new MyMessage());
    ///   MessageBroker&lt;MyMessage&gt;.Default.Subscribe(msg =&gt; { ... });
    /// </summary>
    public class MessageManager : MonoBehaviour, IGameManager
    {
        public void OnAwake()
        {
            Debug.Log("MessageManager Initialized");
        }

        public void OnStart()
        {
        }

        public void OnDispose()
        {
        }
    }
}
