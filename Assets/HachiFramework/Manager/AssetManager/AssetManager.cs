using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;

namespace HachiFramework
{
    public class AssetManager : MonoBehaviour, IGameManager
    {
        public Dictionary<int, AssetHandle> m_GOHandles = new Dictionary<int, AssetHandle>();
        public Dictionary<int, Queue<AssetHandle>> m_ObjHandles = new Dictionary<int, Queue<AssetHandle>>();

        public void OnAwake()
        {
            Debug.Log("AssetManager Initial");
        }

        public void OnDispose()
        {
        }

        public void OnStart()
        {
        }

        #region Common
        public string GetResVersion()
        {
            return YooAssets.GetPackage(ResPackageDefine.DefaultPackage).GetPackageVersion();
        }

        public T LoadAsset<T>(string path, Transform parent = null) where T : Object
        {
            return LoadAssetInternal<T>(path, parent);
        }

        public async UniTask<T> LoadAssetAsync<T>(string path, System.Action<T> callBack, Transform parent = null) where T : Object
        {
            var asset = await LoadAssetAsyncInternal<T>(path, parent, callBack);
            return asset;
        }

        public Sprite LoadSpriteSync(string spriteName)
        {
            return LoadAsset<Sprite>(spriteName);
        }

        public Material GetMaterialByPath(string path)
        {
            return LoadAsset<Material>(path);
        }

        public void ReleaseObj(Object obj)
        {
            RemoveHandleInternal(obj);
        }

        public void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }
        #endregion

        #region Internal
        public T LoadAssetInternal<T>(string path, Transform parent) where T : Object
        {
            T obj = null;
            if (!path.StartsWith(ResPackageDefine.ResPrefix))
            {
                path = ResPackageDefine.ResPrefix + path;
            }
            var handle = YooAssets.LoadAssetSync<T>(path);
            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[AssetManager] 加载资源失败: {path}, Error: {handle.LastError}");
                return obj;
            }
            if (typeof(T) == typeof(GameObject))
            {
                if (parent != null)
                {
                    obj = handle.InstantiateSync(parent) as T;
                }
                else
                {
                    obj = handle.InstantiateSync() as T;
                }
            }
            else
            {
                obj = handle.AssetObject as T;
            }
            AddHandleInternal(obj, handle);
            return obj;
        }

        // 对于异步加载有多种方法这里统一使用UniTask，如果想支持其他方法可以自行扩展
        private async UniTask<T> LoadAssetAsyncInternal<T>(string path, Transform parent, System.Action<T> callBack = null) where T : Object
        {
            T obj = null;
            if (!path.StartsWith(ResPackageDefine.ResPrefix))
            {
                path = ResPackageDefine.ResPrefix + path;
            }
            var handle = YooAssets.LoadAssetAsync<T>(path);
            await handle.ToUniTask();
            if (handle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[AssetManager] 加载资源失败: {path}, Error: {handle.LastError}");
                return obj;
            }
            if (typeof(T) == typeof(GameObject))
            {
                if (parent != null)
                {
                    obj = handle.InstantiateSync(parent) as T;
                }
                else
                {
                    obj = handle.InstantiateSync() as T;
                }
            }
            else
            {
                obj = handle.AssetObject as T;
            }
            AddHandleInternal(obj, handle);
            callBack?.Invoke(obj);
            return obj;
        }
        #endregion

        #region Handle
        private void AddHandleInternal(Object obj, AssetHandle handle)
        {
            if (obj == null)
            {
                Debug.LogError("add handle value null");
                return;
            }
            int intanceID = obj.GetInstanceID();
            if (obj.GetType() == typeof(GameObject))
            {
                if (!m_GOHandles.TryAdd(intanceID, handle))
                {
                    Debug.Log("add GO handle repeat key:" + obj.name);
                }
            }
            else
            {
                if (!m_ObjHandles.TryGetValue(intanceID, out var handleQueue))
                {
                    handleQueue = new Queue<AssetHandle>();
                    m_ObjHandles.TryAdd(intanceID, handleQueue);
                }
                handleQueue.Enqueue(handle);
            }
        }

        private void RemoveHandleInternal(Object obj)
        {
            if (obj == null)
            {
                Debug.LogError("remove obj is null");
                return;
            }
            int instanceId = obj.GetInstanceID();
            if (obj is GameObject)
            {
                if (m_GOHandles.TryGetValue(instanceId, out var handle))
                {
                    handle.Release();
                    m_GOHandles.Remove(instanceId);
                    GameObject.Destroy(obj);
                }
            }
            else
            {
                if (!m_ObjHandles.TryGetValue(instanceId, out var handlesQueue))
                {
                    Debug.Log("cur Obj not find handle queue" + obj.name);
                    return;
                }
                var handle = handlesQueue.Dequeue();
                handle.Release();
                if (handlesQueue.Count <= 0)
                {
                    m_ObjHandles.Remove(instanceId);
                }
            }
        }
        #endregion

        #region Scene
        #endregion
    }
}