using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

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

        public async UniTask<T> LoadAssetAsync<T>(string path, Transform parent = null) where T : Object
        {
            var asset = await LoadAssetAsyncInternal<T>(path, parent);
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
        private async UniTask<T> LoadAssetAsyncInternal<T>(string path, Transform parent = null) where T : Object
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
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称或路径</param>
        /// <param name="sceneMode">加载模式</param>
        /// <param name="onProgress">加载进度回调 (0-1)</param>
        /// <returns>场景句柄，用于后续卸载</returns>
        public async UniTask<SceneHandle> LoadSceneAsync(string sceneName, LoadSceneMode sceneMode = LoadSceneMode.Single, System.Action<float>
        onProgress = null)
        {
            var sceneHandle = YooAssets.LoadSceneAsync(sceneName, sceneMode);

            // 等待加载完成，同时报告进度
            while (!sceneHandle.IsDone)
            {
                onProgress?.Invoke(sceneHandle.Progress);
                await UniTask.Yield();
            }
            onProgress?.Invoke(1.0f);

            if (sceneHandle.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[AssetManager] 场景加载失败: {sceneName}, Error: {sceneHandle.LastError}");
                return null;
            }

            Debug.Log($"[AssetManager] 场景加载成功: {sceneName}");
            return sceneHandle;
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneHandle">场景句柄</param>
        public async UniTask UnloadSceneAsync(SceneHandle sceneHandle)
        {
            if (sceneHandle == null || !sceneHandle.IsValid)
            {
                Debug.LogWarning("[AssetManager] 无效的场景句柄");
                return;
            }

            var unloadOp = sceneHandle.UnloadAsync();
            await unloadOp.ToUniTask();
            Debug.Log("[AssetManager] 场景卸载成功");
        }
        #endregion
    }
}