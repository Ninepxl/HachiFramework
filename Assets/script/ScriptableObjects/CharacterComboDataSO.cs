using UnityEngine;

namespace ActGame
{
    [CreateAssetMenu(fileName = "ComboData", menuName = "Create/Character/ComboData", order = 0)]
    public class CharacterComboDataSO : ScriptableObject
    {
        [SerializeField] private string m_comboName; // 连招名称
        [SerializeField] private string[] m_comboHitName; // 受击名称
        [SerializeField] private string[] m_comboParryName; // 格挡名称
        [SerializeField] private float m_damage; // 伤害
        [SerializeField] private float m_coldTime; // 衔接下一段攻击的间隔时间
        [SerializeField] private float m_comboPositionOffset; // 让这段攻击与目标之间保持最佳距离

        public string ComboName { get => m_comboName; }
        public string[] ComboHitName { get => m_comboHitName; }
        public string[] ComboParryName { get => m_comboParryName; }
        public float Damage { get => m_damage; }
        public float ColdTime { get => m_coldTime; }
        public float ComboPositionOffset { get => m_comboPositionOffset; }

        public int GetHitAndParryNameMaxCount() => ComboHitName.Length;
    }
}