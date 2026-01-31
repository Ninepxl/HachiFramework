/// <summary>
/// 游戏入口
/// Author: Hachi
/// Date: 2025-12-1
/// Description: 游戏入口类，负责初始化和管理所有的组件
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HachiFramework
{
    public class GameEntry : MonoBehaviour
    {
        private static GameEntry m_instance;
        public static GameEntry Instance => m_instance;
        private List<IGameManager> m_gameComponets = new();
        private Dictionary<Type, IGameManager> m_ComponentCache = new(); // 所有的组件
        private List<IUpdate> m_updateComponents = new();
        private List<ILateUpdate> m_lateUpdateComponents = new();
        private List<IFixedUpdate> m_fixUpdateComponents = new();
        private bool m_isInitialized = false;

        #region public
        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        public IGameManager Get(Type type)
        {
            if (m_ComponentCache.TryGetValue(type, out var component))
            {
                return component;
            }
            return null;
        }

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (Get(type) is T component)
            {
                return component;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 动态注册组件 可能没有必要，对于用户自己的Manager是在游戏运行的时候就挂在到GameEntry上上面去了
        /// </summary>
        public void RegisterComponent(IGameManager component)
        {
            if (component == null)
            {
                Debug.LogError("尝试注册空组件");
                return;
            }

            Type componentType = component.GetType();

            if (m_ComponentCache.ContainsKey(componentType))
            {
                Debug.LogWarning($"组件 {componentType.Name} 已存在，将被替换");
                UnregisterComponent(componentType);
            }

            Reg(component);
            m_gameComponets.Add(component);

            if (m_isInitialized)
            {
                component.OnAwake();
                component.OnStart();

                if (component is IUpdate updateComponent)
                {
                    m_updateComponents.Add(updateComponent);
                }
                if (component is ILateUpdate lateUpdateComponent)
                {
                    m_lateUpdateComponents.Add(lateUpdateComponent);
                }
                if (component is IFixedUpdate fixUpdateComponent)
                {
                    m_fixUpdateComponents.Add(fixUpdateComponent);
                }
            }
        }

        /// <summary>
        /// 注销组件
        /// </summary>
        public void UnregisterComponent(Type type)
        {
            if (!m_ComponentCache.TryGetValue(type, out var component))
            {
                return;
            }

            component.OnDispose();

            if (component is IUpdate updateComponent)
            {
                m_updateComponents.Remove(updateComponent);
            }
            if (component is ILateUpdate lateUpdateComponent)
            {
                m_lateUpdateComponents.Remove(lateUpdateComponent);
            }
            if (component is IFixedUpdate fixUpdateComponent)
            {
                m_fixUpdateComponents.Remove(fixUpdateComponent);
            }

            m_gameComponets.Remove(component);
            m_ComponentCache.Remove(type);
        }

        /// <summary>
        /// 注销组件
        /// </summary>
        public void UnregisterComponent<T>() where T : class
        {
            UnregisterComponent(typeof(T));
        }

        #endregion
        #region private
        private void Init()
        {
            // 自动收集当前GameObject及其子物体上所有的IGameComponent
            var allComponents = GetComponentsInChildren<IGameManager>(true);

            foreach (var item in allComponents)
            {
                Reg(item);
                m_gameComponets.Add(item);
            }

            // 执行所有组件的初始化
            foreach (var item in m_gameComponets)
            {
                item.OnAwake();
            }

            foreach (var item in m_gameComponets)
            {
                item.OnStart();
            }

            // 分类收集需要Update的组件
            foreach (var item in m_gameComponets)
            {
                if (item is IUpdate updateComponent)
                {
                    m_updateComponents.Add(updateComponent);
                }
                if (item is ILateUpdate lateUpdateComponent)
                {
                    m_lateUpdateComponents.Add(lateUpdateComponent);
                }
                if (item is IFixedUpdate fixUpdateComponent)
                {
                    m_fixUpdateComponents.Add(fixUpdateComponent);
                }
            }

            m_isInitialized = true;
        }

        private void Dispose()
        {
            foreach (var item in m_gameComponets)
            {
                item.OnDispose();
            }
            m_gameComponets.Clear();
            m_updateComponents.Clear();
            m_lateUpdateComponents.Clear();
            m_fixUpdateComponents.Clear();
            m_ComponentCache.Clear();
        }

        private void Reg(IGameManager component)
        {
            Type componentType = component.GetType();

            if (m_ComponentCache.ContainsKey(componentType))
            {
                Debug.LogError($"重复注册组件: {componentType.Name}");
            }
            else
            {
                m_ComponentCache.Add(componentType, component);
            }
        }
        #endregion
        #region Unity
        void Awake()
        {
            if (m_instance != null)
            {
                Destroy(this);
                return;
            }
            m_instance = this;
            DontDestroyOnLoad(this);
            Init();
        }

        void Update()
        {
            foreach (var item in m_updateComponents)
            {
                item.OnUpdate();
            }
        }

        void LateUpdate()
        {
            foreach (var item in m_lateUpdateComponents)
            {
                item.OnLateUpdate();
            }
        }

        void FixedUpdate()
        {
            foreach (var item in m_fixUpdateComponents)
            {
                item.OnFixedUpdate();
            }
        }

        #endregion
    }

}