using UniFramework.Machine;
using UnityEngine;

namespace HachiFramework
{
    public abstract class ProcedureBase : IStateNode
    {
        protected StateMachine Machine { get; private set; }

        public virtual void OnCreate(StateMachine machine)
        {
            Machine = machine;
            // Debug.Log($"[ProcedureManager]流程节点创建成功: {GetType().FullName}");
        }

        public virtual void OnEnter()
        {
            Debug.Log($"[ProcedureManager]进入{GetType().FullName}节点");
        }

        public virtual void OnExit()
        {
            Debug.Log($"[ProcedureManager]退出{GetType().FullName}节点");
        }

        public virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 切换到指定流程
        /// </summary>
        protected void ChangeState<T>() where T : ProcedureBase
        {
            Machine.ChangeState<T>();
        }

        /// <summary>
        /// 切换到指定流程
        /// </summary>
        protected void ChangeState(System.Type procedureType)
        {
            Machine.ChangeState(procedureType);
        }
    }
}