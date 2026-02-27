namespace HachiFramework
{
    /// <summary>
    /// UI 控制器基类
    /// 处理业务逻辑、绑定事件、响应生命周期
    /// </summary>
    public class UICtrl
    {
        public int uid;
        public UIView view;
        public UIModel model;
        public bool isShow;

        /// <summary>
        /// 面板创建时调用（仅一次），View 和 Model 已绑定
        /// </summary>
        public virtual void OnCreate()
        {
        }

        /// <summary>
        /// 绑定 UI 事件（按钮点击等），在 OnCreate 之后调用
        /// </summary>
        public virtual void BindUIEvent()
        {
        }

        /// <summary>
        /// 面板每次显示时调用（首次打开 + 从 Hide 恢复都会调用）
        /// </summary>
        public virtual void OnShow()
        {
        }

        /// <summary>
        /// 面板隐藏时调用
        /// </summary>
        public virtual void OnHide()
        {
        }

        /// <summary>
        /// 面板销毁时调用，清理资源
        /// </summary>
        public virtual void OnDestroy()
        {
            view = null;
            model = null;
            isShow = false;
        }

        /// <summary>
        /// 关闭自身的快捷方法
        /// </summary>
        public void Close()
        {
            GameEntry.Instance.Get<UIManager>()?.CloseUI(uid);
        }
    }
}