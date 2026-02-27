using UnityEngine;

namespace HachiFramework
{
    /// <summary>
    /// 面板运行时状态
    /// </summary>
    public enum UIState
    {
        None,       // 未加载
        Loading,    // 加载中
        Show,       // 显示中
        Hide,       // 已隐藏（未销毁，可快速重新显示）
    }

    /// <summary>
    /// 单个面板的运行时完整信息
    /// </summary>
    public class UIInfo
    {
        public int uid;
        public UIConfig config;
        public UIState state;
        public UICtrl ctrl;
        public UIView view;
        public UIModel model;
        public GameObject go;
        public CanvasGroup canvasGroup;

        public UIInfo(UIConfig config)
        {
            this.uid = config.ID;
            this.config = config;
            this.state = UIState.None;
        }

        /// <summary>
        /// 清理运行时引用（面板销毁时调用）
        /// </summary>
        public void Clear()
        {
            ctrl = null;
            view = null;
            model = null;
            go = null;
            canvasGroup = null;
            state = UIState.None;
        }
    }
}