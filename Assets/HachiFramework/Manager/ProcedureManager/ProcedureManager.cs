using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using System;

namespace HachiFramework
{
    /// <summary>
    /// 流程管理器
    /// 负责管理游戏中的各个流程节点，控制流程的切换和状态查询
    /// </summary>
    public class ProcedureManager : MonoBehaviour, IGameManager, IUpdate
    {
        /// <summary>
        /// 内部状态机，用于管理流程节点的状态切换
        /// </summary>
        private StateMachine m_stateMachine;

        // 被勾选的流程
        [SerializeField, HideInInspector]
        private List<string> m_availableProcedureTypeNames = new List<string>();

        // 第一个要进入的流程
        [SerializeField, HideInInspector]
        private string m_entranceProcedureTypeName;

        #region 生命周期
        /// <summary>
        /// 管理器初始化时调用，创建状态机实例
        /// </summary>
        public void OnAwake()
        {
            Debug.Log("ProcedureManager Initial");
            m_stateMachine = new(this);
            Initialize();
        }

        /// <summary>
        /// 管理器销毁时调用，用于清理资源
        /// </summary>
        public void OnDispose()
        {
        }


        public void OnUpdate()
        {
            m_stateMachine?.Update();
        }
        /// <summary>
        /// 管理器启动时调用，启动入口流程
        /// </summary>
        public void OnStart()
        {
            if (string.IsNullOrEmpty(m_entranceProcedureTypeName))
            {
                Debug.LogError("入口流程未设置");
                return;
            }

            Type entranceType = Type.GetType(m_entranceProcedureTypeName);
            if (entranceType == null)
            {
                Debug.LogError($"无法找到入口流程类型: {m_entranceProcedureTypeName}");
                return;
            }

            StartProcedure(entranceType);
        }
        #endregion

        /// <summary>
        /// 将所有流程节点保存到ProcedureManager
        /// </summary>
        public void Initialize()
        {
            if (m_availableProcedureTypeNames == null) return;

            foreach (var typeName in m_availableProcedureTypeNames)
            {
                // 通过类名获取类型
                Type type = Type.GetType(typeName);
                if (type != null)
                {
                    // 实例化流程节点
                    IStateNode node = Activator.CreateInstance(type) as IStateNode;
                    m_stateMachine.AddNode(node);
                    node.OnCreate(m_stateMachine);
                }
            }
        }

        #region 流程控制
        /// <summary>
        /// 开启指定类型的流程
        /// </summary>
        /// <typeparam name="T">流程类型，必须继承自ProcedureBase</typeparam>
        public void StartProcedure<T>() where T : ProcedureBase
        {
            if (m_stateMachine == null)
            {
                Debug.LogError("当前流程管理器状态机为空");
                return;
            }
            m_stateMachine.Run<T>();
        }

        /// <summary>
        /// 开启指定类型的流程
        /// </summary>
        /// <param name="procedureNodeType">流程节点的类型</param>
        public void StartProcedure(Type procedureNodeType)
        {
            if (m_stateMachine == null)
            {
                Debug.LogError("当前流程管理器状态机为空");
                return;
            }
            m_stateMachine.Run(procedureNodeType);
        }
        #endregion

        #region 流程查询
        /// <summary>
        /// 获取指定类型的流程节点
        /// </summary>
        /// <typeparam name="T">流程类型，必须继承自ProcedureBase</typeparam>
        /// <returns>流程节点实例，如果不存在则返回null</returns>
        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            var nodeName = typeof(T).FullName;
            return m_stateMachine.TryGetNode(nodeName) as ProcedureBase;
        }

        /// <summary>
        /// 获取指定类型的流程节点
        /// </summary>
        /// <param name="procedureNodeType">流程节点的类型</param>
        /// <returns>流程节点实例，如果不存在则返回null</returns>
        public ProcedureBase GetProcedure(Type procedureNodeType)
        {
            var nodeName = procedureNodeType.FullName;
            return m_stateMachine.TryGetNode(nodeName) as ProcedureBase;
        }

        /// <summary>
        /// 判断指定类型的流程节点是否存在
        /// </summary>
        /// <typeparam name="T">流程类型，必须继承自ProcedureBase</typeparam>
        /// <returns>如果流程节点存在返回true，否则返回false</returns>
        public bool HasProcedure<T>() where T : ProcedureBase
        {
            var nodeName = typeof(T).FullName;
            return m_stateMachine.TryGetNode(nodeName) != null;
        }

        /// <summary>
        /// 判断指定类型的流程节点是否存在
        /// </summary>
        /// <param name="procedureNodeType">流程节点的类型</param>
        /// <returns>如果流程节点存在返回true，否则返回false</returns>
        public bool HasProcedure(Type procedureNodeType)
        {
            var nodeName = procedureNodeType.FullName;
            return m_stateMachine.TryGetNode(nodeName) != null;
        }
        #endregion
    }

}