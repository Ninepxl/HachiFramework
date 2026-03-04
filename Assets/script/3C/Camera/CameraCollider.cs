using UnityEngine;
namespace ActGame
{
    public class CameraCollider : MonoBehaviour
    {
        [Header("检测范围")]
        [SerializeField] private Vector2 m_maxDistanceOffset; // x为最小值，y为最大值

        [Header("碰撞检测层")]
        [SerializeField] private LayerMask m_whatIsDetetcionLayer;

        [SerializeField, Header("射线长度"), Space(10f)]
        private float m_detectionDistance;

        [SerializeField, Header("摄像机移动平滑度"), Space(10f)]

        private float m_smoothColliderTime;

        // 摄像机的相对起点用来判断摄像机是拉进还是拉远
        private Vector3 m_originPosition;
        // TP_Camera z轴位置
        private float m_originOffsetDistance;
        private Transform m_mainCamera;
        void Awake()
        {
            m_mainCamera = Camera.main.transform;
        }

        void Start()
        {
            m_originPosition = transform.localPosition.normalized;
            m_originOffsetDistance = m_maxDistanceOffset.y;
        }

        void LateUpdate()
        {
            UpdateDetetionCollider();
        }

        /// <summary>
        /// 检测摄像机到角色之间是否被遮挡
        /// </summary>
        public void UpdateDetetionCollider()
        {
            var targetPos = transform.TransformPoint(m_originPosition * m_detectionDistance);
            if (Physics.Linecast(transform.position, targetPos, out var hit, m_whatIsDetetcionLayer, QueryTriggerInteraction.Ignore))
            {
                // 摄像机碰撞到了
                m_originOffsetDistance = Mathf.Clamp(hit.distance * .8f, m_maxDistanceOffset.x, m_maxDistanceOffset.y);
            }
            else
            {
                // 摄像机没有碰撞到
                m_originOffsetDistance = m_maxDistanceOffset.y;
            }
            // 修正MainCream的位置
            m_mainCamera.localPosition = Vector3.Lerp(m_mainCamera.localPosition, m_originPosition * (m_originOffsetDistance - 0.1f), GameUnilts.UnTetheredLerp(m_smoothColliderTime));
        }

        void OnDrawGizmos()
        {
            var targetPos = transform.TransformPoint(m_originPosition * m_detectionDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPos);
        }
    }
}
