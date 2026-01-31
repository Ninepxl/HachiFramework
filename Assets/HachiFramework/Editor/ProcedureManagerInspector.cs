using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HachiFramework.Editor
{
    [CustomEditor(typeof(ProcedureManager))]
    public class ProcedureManagerInspector : UnityEditor.Editor
    {
        private SerializedProperty m_availableProcedures;
        private SerializedProperty m_entranceProcedure;

        // 所有的流程
        private List<string> m_allProcedureTypes = new List<string>();

        private void OnEnable()
        {
            // 绑定属性
            m_availableProcedures = serializedObject.FindProperty("m_availableProcedureTypeNames");
            m_entranceProcedure = serializedObject.FindProperty("m_entranceProcedureTypeName");

            // 扫描工程中所有继承自 ProcedureBase 的类
            ScanProcedures();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ProcedureManager manager = (ProcedureManager)target;

            GUILayout.Space(10);
            GUILayout.Label("可用流程列表 (Available Procedures)", EditorStyles.boldLabel);

            // 绘制勾选列表
            EditorGUILayout.BeginVertical("helpBox");
            for (int i = 0; i < m_allProcedureTypes.Count; i++)
            {
                string typeName = m_allProcedureTypes[i];
                bool isSelected = IsProcedureSelected(typeName);

                bool toggle = EditorGUILayout.ToggleLeft(typeName, isSelected);
                if (toggle != isSelected)
                {
                    UpdateProcedureSelection(typeName, toggle);
                }
            }
            EditorGUILayout.EndVertical();

            // 绘制入口下拉框
            GUILayout.Space(10);
            List<string> selectedList = GetSelectedProcedures();
            if (selectedList.Count > 0)
            {
                int currentIndex = selectedList.IndexOf(m_entranceProcedure.stringValue);
                int nextIndex = EditorGUILayout.Popup("入口流程 (Entrance)", currentIndex < 0 ? 0 : currentIndex, selectedList.ToArray());
                m_entranceProcedure.stringValue = selectedList[nextIndex];
            }
            else
            {
                EditorGUILayout.HelpBox("请至少勾选一个流程节点", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 获取所有的流程 
        /// </summary>
        private void ScanProcedures()
        {
            m_allProcedureTypes.Clear();
            // 扫描当前域中所有程序集
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(ProcedureBase)) && !t.IsAbstract);

            foreach (var t in types)
            {
                m_allProcedureTypes.Add(t.FullName); // 使用全路径类名，防止跨程序集找不到
            }
        }

        private bool IsProcedureSelected(string typeName)
        {
            for (int i = 0; i < m_availableProcedures.arraySize; i++)
            {
                if (m_availableProcedures.GetArrayElementAtIndex(i).stringValue == typeName) return true;
            }
            return false;
        }

        private void UpdateProcedureSelection(string typeName, bool add)
        {
            if (add)
            {
                int index = m_availableProcedures.arraySize;
                m_availableProcedures.InsertArrayElementAtIndex(index);
                m_availableProcedures.GetArrayElementAtIndex(index).stringValue = typeName;
            }
            else
            {
                for (int i = 0; i < m_availableProcedures.arraySize; i++)
                {
                    if (m_availableProcedures.GetArrayElementAtIndex(i).stringValue == typeName)
                    {
                        m_availableProcedures.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }
        }

        private List<string> GetSelectedProcedures()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < m_availableProcedures.arraySize; i++)
            {
                list.Add(m_availableProcedures.GetArrayElementAtIndex(i).stringValue);
            }
            return list;
        }
    }
}