using System;
using System.Collections.Generic;
using HachiFramework;
using UnityEngine;

namespace ActGame
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovementControlBase : MonoBehaviour
    {
        protected CharacterController m_control;
        protected Animator m_animator;

        /// <summary>
        /// 地面检测
        /// </summary>

        [Header("地面检测")]
        [SerializeField]
        protected bool m_characterIsOnGround;
        [SerializeField]
        protected float m_groundDetectionPositionOffset;
        [SerializeField]
        protected float m_detectionRange;
        [SerializeField]
        protected LayerMask m_whatIsGroud;

        /// <summary>
        /// 重力 控制角色的Y轴位移
        /// </summary>
        protected readonly float CharacterGravity = -9.8f;
        protected float m_characterVerticalVelocity; // 角色Y轴速度
        protected float m_fallOutDltaTime;
        protected float m_fallOutTime = 0.15f;
        protected readonly float m_characterVerticalMaxVelocity = -54f; // 角色Y轴最大速度
        protected Vector3 m_characterVerticalDirection; // 角色Y轴移动方向（需要将移动的方向和重力控制的方向分别判断）
        protected Vector2 m_movementDirection; // 玩家移动的方向
        private HashSet<IDisposable> m_eventList = new();
        protected virtual void Awake()
        {
            m_control = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();
        }
        void OnEnable()
        {
            m_eventList.Add(MessageBroker<ChangeVelocityEvent>.Default.Subscribe(ChangetCHaracterVerticalVelocity));
        }

        void OnDisable()
        {
            foreach (var item in m_eventList)
            {
                item.Dispose();
            }
            m_eventList.Clear();
        }

        protected virtual void Update()
        {
            SetCharacterGravity();
            UpdateCharacterGravity();
            // Debug.Log(m_characterIsOnGround);
        }

        protected virtual void OnAnimatorMove()
        {
            m_animator.ApplyBuiltinRootMotion();
            UpdateCharacterMoveDirection(m_animator.deltaPosition);
        }

        /// <summary>
        /// 地面检测 
        /// </summary>
        /// <returns></returns>
        private bool GroundDetection()
        {
            Vector3 detectionPos = new Vector3(transform.position.x, transform.position.y - m_groundDetectionPositionOffset, transform.position.z);
            return Physics.CheckSphere(detectionPos, m_detectionRange, m_whatIsGroud, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// 角色重力计算 
        /// </summary>
        private void SetCharacterGravity()
        {
            m_characterIsOnGround = GroundDetection();
            if (m_characterIsOnGround)
            {
                // 角色在地面上面
                // 1. 重置下落时间检查
                // 2. 重置角色Y轴速度
                m_fallOutDltaTime = m_fallOutTime;
                if (m_characterVerticalVelocity < 0)
                {
                    m_characterVerticalVelocity = -2f;
                }
            }
            else
            {
                // 不在地面上面
                if (m_fallOutDltaTime > 0)
                {
                    m_fallOutDltaTime -= Time.deltaTime;
                }
                else
                {
                    // 高度足够播放下落动画
                }
                if (m_characterVerticalVelocity > m_characterVerticalMaxVelocity)
                {
                    // v = a * t
                    m_characterVerticalVelocity += CharacterGravity * Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 玩家应用重力 
        /// </summary>
        private void UpdateCharacterGravity()
        {
            m_characterVerticalDirection.Set(0, m_characterVerticalVelocity, 0);
            // s = v * t  这个速度由方向向量乘以速度构成
            m_control.Move(m_characterVerticalDirection * Time.deltaTime);
        }

        /// <summary>
        /// 坡道检测 
        /// </summary>
        /// <returns></returns>
        private Vector3 SlopDetection(Vector3 moveDirection)
        {
            // 怎么检测玩家在斜坡上面
            // 玩家在斜坡上面会怎么样
            if (Physics.Raycast(transform.position + (transform.up * 0.5f), Vector3.down, out var hit, m_control.height * 0.85f, m_whatIsGroud, QueryTriggerInteraction.Ignore))
            {
                if (Vector3.Dot(Vector3.up, hit.normal) != 0)
                {
                    return Vector3.ProjectOnPlane(moveDirection, hit.normal);
                }
            }
            return moveDirection;
        }

        private void UpdateCharacterMoveDirection(Vector3 direction)
        {
            m_movementDirection = SlopDetection(direction);
            // m_control.Move(m_movementDirection * Time.deltaTime);
        }

        void OnDrawGizmos()
        {
            Vector3 detectionPos = new Vector3(transform.position.x, transform.position.y - m_groundDetectionPositionOffset, transform.position.z);
            Gizmos.DrawWireSphere(detectionPos, m_detectionRange);
        }

        private void ChangetCHaracterVerticalVelocity(ChangeVelocityEvent velocity)
        {
            Debug.Log("角色速度改变: " + velocity);
        }

    }

}