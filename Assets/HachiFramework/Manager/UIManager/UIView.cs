using UnityEngine;

namespace HachiFramework
{
    /// <summary>
    /// UI 视图层基类
    /// 持有 GameObject/Transform 引用，负责查找 UI 组件
    /// </summary>
    public class UIView
    {
        public GameObject gameObject;
        public Transform transform;
        public CanvasGroup canvasGroup;

        /// <summary>
        /// 由 UIManager 在面板加载完成后调用，传入实例化的 GameObject
        /// </summary>
        public void Init(GameObject go)
        {
            gameObject = go;
            transform = go.transform;
            canvasGroup = go.GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 子类重写，查找并缓存 UI 组件引用
        /// 如：btn_login = transform.Find("btn_login").GetComponent&lt;Button&gt;();
        /// </summary>
        public virtual void FindComponents()
        {
        }

        /// <summary>
        /// 面板销毁时调用，清理引用
        /// </summary>
        public virtual void OnDestroy()
        {
            gameObject = null;
            transform = null;
            canvasGroup = null;
        }
    }
}