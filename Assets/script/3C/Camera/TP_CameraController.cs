using UnityEngine;
using HachiFramework;

namespace ActGame
{
    public class TP_CameraController : MonoBehaviour
    {
        [Tooltip("相机围绕观察的目标（通常是玩家角色）。")]
        [SerializeField]
        private Transform m_lookTarget;

        [Tooltip("鼠标/摇杆输入转换为相机旋转时的灵敏度。")]
        [SerializeField]
        private float m_controlSpeed = 5f;

        [Tooltip("X=最小俯仰角，Y=最大俯仰角，用于限制相机上下看范围。")]
        [SerializeField]
        private Vector2 m_cameraVerticalMaxAngle;

        [Tooltip("相机与目标之间的距离偏移（沿相机后方方向）。")]
        [SerializeField]
        private float m_positionOffset;

        [Tooltip("旋转平滑时间，越大转向越缓，越小越跟手。")]
        [SerializeField]
        private float m_smoothSpeed;

        [Tooltip("位置插值速度，控制相机跟随目标时的平滑程度。")]
        [SerializeField]
        private float m_positionSmoothTime;

        private Vector2 m_input;
        private Vector3 m_cameraRotation;
        private Vector3 m_smoothDampVelocity = Vector3.zero;

        void Awake()
        {
            var mainPlayer = GameObject.FindWithTag("MainPlayer");
            if (mainPlayer != null)
            {
                m_lookTarget = mainPlayer.transform;
            }
            else
            {
                Debug.LogError("TP_CameraController: 未找到 MainPlayer 标签对象，请检查场景标签配置。", this);
            }
        }
        void Start()
        {
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }
        void Update()
        {
            CameraInput();
        }

        void LateUpdate()
        {
            UpdateCameraRotation();
            CameraPosition();
        }

        private void CameraInput()
        {
            // 摄像机Y轴旋转，视角左右移动 
            m_input.y += GameEntry.inputManager.CameraLook.x * m_controlSpeed;
            // 摄像机X轴旋转，视角上下移动
            m_input.x -= GameEntry.inputManager.CameraLook.y * m_controlSpeed;
            m_input.x = Mathf.Clamp(m_input.x, m_cameraVerticalMaxAngle.x, m_cameraVerticalMaxAngle.y);
        }
        private void UpdateCameraRotation()
        {
            m_cameraRotation = Vector3.SmoothDamp(m_cameraRotation, new Vector3(m_input.x, m_input.y, 0f), ref m_smoothDampVelocity, m_smoothSpeed);
            transform.eulerAngles = m_cameraRotation;
        }

        private void CameraPosition()
        {
            var newPosition = m_lookTarget.position + (-transform.forward * m_positionOffset);
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * m_positionSmoothTime);
        }
    }
}
