using System;
using System.Collections.Generic;
using UnityEngine;

namespace HachiFramework
{
    public class GamePoolManager : MonoBehaviour, IGameManager
    {
        private readonly Dictionary<GameObject, Stack<GameObject>> m_Pools = new();
        private readonly Dictionary<GameObject, Stack<GameObject>> m_InstancePools = new();

        public void OnAwake()
        {
            Debug.Log("GamePoolManager Initial");
            m_Pools.Clear();
            m_InstancePools.Clear();
        }

        public void OnDispose()
        {
            foreach (var goStack in m_Pools.Values)
            {
                foreach (var go in goStack)
                {
                    UnityEngine.Object.Destroy(go);
                }
            }
            m_Pools.Clear();
            m_InstancePools.Clear();
        }

        public void OnStart()
        {
        }
        public GameObject Rent(GameObject go)
        {
            if (go == null) throw new ArgumentNullException(nameof(go));
            var pool = GetOrCreatePool(go);
            GameObject obj;
            while (true)
            {
                // 尝试从对象池中拿到物体
                if (!pool.TryPop(out obj))
                {
                    // 如果对象池中没有对象就直接克隆传入的GameObject返回给用户
                    obj = UnityEngine.Object.Instantiate(go);
                    break;
                }
                else if (obj != null)
                {
                    obj.SetActive(true);
                    break;
                }
            }
            PoolCallbackHelper.InvokeOnRent(obj);
            if (!m_InstancePools.TryAdd(obj, pool))
            {
                Debug.LogWarning($"[GamePoolManager] 对象 {obj.name} 已在租借列表中");
            }
            return obj;
        }
        public GameObject Rent(GameObject original, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, parent);
                    break;
                }
                else if (obj != null)
                {
                    obj.transform.SetParent(parent);
                    obj.SetActive(true);
                    break;
                }
            }

            if (!m_InstancePools.TryAdd(obj, pool))
            {
                Debug.LogWarning($"[GamePoolManager] 对象 {obj.name} 已在租借列表中");
            }

            PoolCallbackHelper.InvokeOnRent(obj);

            return obj;
        }

        public GameObject Rent(GameObject original, Vector3 position, Quaternion rotation)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, position, rotation);
                    break;
                }
                else if (obj != null)
                {
                    obj.transform.SetPositionAndRotation(position, rotation);
                    obj.SetActive(true);
                    break;
                }
            }

            if (!m_InstancePools.TryAdd(obj, pool))
            {
                Debug.LogWarning($"[GamePoolManager] 对象 {obj.name} 已在租借列表中");
            }

            PoolCallbackHelper.InvokeOnRent(obj);

            return obj;
        }

        public GameObject Rent(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            GameObject obj;
            while (true)
            {
                if (!pool.TryPop(out obj))
                {
                    obj = UnityEngine.Object.Instantiate(original, position, rotation, parent);
                    break;
                }
                else if (obj != null)
                {
                    obj.transform.SetParent(parent);
                    obj.transform.SetPositionAndRotation(position, rotation);
                    obj.SetActive(true);
                    break;
                }
            }

            if (!m_InstancePools.TryAdd(obj, pool))
            {
                Debug.LogWarning($"[GamePoolManager] 对象 {obj.name} 已在租借列表中");
            }

            PoolCallbackHelper.InvokeOnRent(obj);

            return obj;
        }
        public TComponent Rent<TComponent>(TComponent original) where TComponent : Component
        {
            return Rent(original.gameObject).GetComponent<TComponent>();
        }
        public TComponent Rent<TComponent>(TComponent original, Vector3 position, Quaternion rotation, Transform parent) where TComponent : Component
        {
            return Rent(original.gameObject, position, rotation, parent).GetComponent<TComponent>();
        }

        public TComponent Rent<TComponent>(TComponent original, Vector3 position, Quaternion rotation) where TComponent : Component
        {
            return Rent(original.gameObject, position, rotation).GetComponent<TComponent>();
        }

        public TComponent Rent<TComponent>(TComponent original, Transform parent) where TComponent : Component
        {
            return Rent(original.gameObject, parent).GetComponent<TComponent>();
        }
        public void Return(GameObject instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (!m_InstancePools.TryGetValue(instance, out var pool))
            {
                UnityEngine.Object.Destroy(instance);
                return;
            }
            PoolCallbackHelper.InvokeOnReturn(instance);
            instance.SetActive(false);
            // 归还物体到对象池中
            pool.Push(instance);
            m_InstancePools.Remove(instance);
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <param name="original"></param>
        /// <param name="count"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Prewarm(GameObject original, int count)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);

            for (int i = 0; i < count; i++)
            {
                var obj = UnityEngine.Object.Instantiate(original);
                obj.SetActive(false);
                pool.Push(obj);

                PoolCallbackHelper.InvokeOnReturn(obj);
            }
        }
        /// <summary>
        /// 获取对象池
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private Stack<GameObject> GetOrCreatePool(GameObject go)
        {
            if (!m_Pools.TryGetValue(go, out var pool))
            {
                pool = new Stack<GameObject>();
                m_Pools.Add(go, pool);
            }
            return pool;
        }
    }
}