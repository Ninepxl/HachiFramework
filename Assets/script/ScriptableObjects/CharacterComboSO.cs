using System.Collections.Generic;
using UnityEngine;

namespace ActGame
{
    [CreateAssetMenu(fileName = "Combo", menuName = "Create/Character/Combo", order = 0)]
    public class CharacterComboSO : ScriptableObject
    {
        [SerializeField]
        List<CharacterComboDataSO> m_comboDatas = new List<CharacterComboDataSO>();

        public string GetComboName(int index)
        {
            if (m_comboDatas == null || m_comboDatas.Count == 0) return null;
            return m_comboDatas[index].ComboName;
        }

        public string GetComboHitName(int index, int hitIndex)
        {
            if (m_comboDatas == null || m_comboDatas.Count == 0) return null;
            if (m_comboDatas[index].ComboHitName == null || m_comboDatas[index].ComboHitName.Length == 0) return null;
            return m_comboDatas[index].ComboHitName[hitIndex];
        }

        public string GetComboParryName(int index, int parryIndex)
        {
            if (m_comboDatas == null || m_comboDatas.Count == 0) return null;
            if (m_comboDatas[index].ComboParryName == null || m_comboDatas[index].ComboParryName.Length == 0) return null;
            return m_comboDatas[index].ComboParryName[parryIndex];
        }

        public float GetDamage(int index)
        {
            if (m_comboDatas == null || m_comboDatas.Count == 0) return 0f;
            return m_comboDatas[index].Damage;
        }

        public float GetColdTime(int index)
        {
            if (m_comboDatas == null || m_comboDatas.Count == 0) return 0f;
            return m_comboDatas[index].ColdTime;
        }

        public float GetComboPositionOffset(int index)
        {
            if (m_comboDatas == null || m_comboDatas.Count == 0) return 0f;
            return m_comboDatas[index].ComboPositionOffset;
        }

        public int GetComboCount()
        {
            if (m_comboDatas == null) return 0;
            return m_comboDatas.Count;
        }

        public int GetHitAndParryNameMaxCount(int index)
        {
            if (m_comboDatas == null || m_comboDatas.Count == 0) return 0;
            return m_comboDatas[index].GetHitAndParryNameMaxCount();
        }

    }
}