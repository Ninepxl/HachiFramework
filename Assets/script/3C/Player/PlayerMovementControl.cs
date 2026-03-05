using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActGame
{
    public class PlayerMovementControl : CharacterMovementControlBase
    {
        private float m_rotationVelocity;
        [SerializeField] private float m_rotationSmoothTime;
        private float m_rotationAngle;
        private Camera m_mainCamera;
        protected override void Awake()
        {
            base.Awake();
            m_mainCamera = Camera.main;
        }
        protected override void Update()
        {
            base.Update();
        }

        void LateUpdate()
        {
            UpdateAnimation();
            CharacterRotationControl();
        }

        private void CharacterRotationControl()
        {
            if (!m_characterIsOnGround) return;
            if (m_animator.GetBool("HasInput"))
            {
                m_rotationAngle = Mathf.Atan2(GameEntry.inputManager.Movement.x, GameEntry.inputManager.Movement.y) * Mathf.Rad2Deg + m_mainCamera.transform.eulerAngles.y;
            }
            if (m_animator.GetBool("HasInput") && m_animator.AnimationAtTag("Motion"))
            {
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, m_rotationAngle, ref m_rotationVelocity, m_rotationSmoothTime);
            }
        }

        /// <summary>
        /// 更新动画状态 
        /// </summary>
        private void UpdateAnimation()
        {
            if (!m_characterIsOnGround) return;
            m_animator.SetBool("HasInput", GameEntry.inputManager.Movement != Vector2.zero);
            if (m_animator.GetBool("HasInput"))
            {
                if (GameEntry.inputManager.Run)
                {
                    m_animator.SetBool("Run", true);
                }
                m_animator.SetFloat("Movement", m_animator.GetBool("Run") ? 2f : GameEntry.inputManager.Movement.sqrMagnitude, 0.25f, Time.deltaTime);
            }
            else
            {
                m_animator.SetFloat("Movement", 0f, 0.25f, Time.deltaTime);
                if (m_animator.GetFloat("Movement") < 0.2f)
                {
                    m_animator.SetBool("Run", false);
                }
            }
        }
        // 
    }
}
