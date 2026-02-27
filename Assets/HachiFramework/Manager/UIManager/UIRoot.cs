using System.Collections.Generic;
using UnityEngine;

namespace HachiFramework
{
    /// <summary>
    /// UIRoot 预制体根节点脚本
    /// 在 Inspector 中拖拽赋值 Canvas 列表和 UICamera
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        [Header("所有分层 Canvas（按层级顺序拖入）")]
        public List<Canvas> CanvasPanelList;

        [Header("UI 专用相机")]
        public Camera UICamera;
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
