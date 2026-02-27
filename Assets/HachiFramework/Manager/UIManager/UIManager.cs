using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace HachiFramework
{
    /// <summary>
    /// UI 管理器
    /// 职责：加载 UIRoot、管理 Canvas 层级、面板生命周期调度
    /// </summary>
    public class UIManager : MonoBehaviour, IGameManager, IUpdate
    {
        // UIRoot 引用（在 Inspector 中拖入场景中的 UIRoot）
        [SerializeField] private UIRoot m_UIRoot;
        private Camera m_UICamera;

        // 容器：层级名 → Canvas RectTransform
        private readonly Dictionary<string, RectTransform> m_CanvasDict = new();

        // 内容：面板ID → 运行时信息
        private readonly Dictionary<int, UIInfo> m_UIInfoDict = new();

        // First/Second 层的打开栈
        private readonly Stack<int> m_PanelStack = new();

        // 屏幕适配
        private CanvasScaler m_CanvasScaler;
        public float ScaleFactor { get; private set; }
        private float m_ScreenWidth;
        private float m_ScreenHeight;

        #region 生命周期

        public void OnAwake()
        {
            InitUIRoot();
            RegisterAll();
            Debug.Log("[UIManager] Initialized");
        }

        public void OnStart()
        {
            m_ScreenWidth = Screen.width;
            m_ScreenHeight = Screen.height;
            ResetCanvasScaler();
        }

        public void OnUpdate()
        {
            // 监听分辨率变化
            if (!Mathf.Approximately(m_ScreenWidth, Screen.width) ||
                !Mathf.Approximately(m_ScreenHeight, Screen.height))
            {
                ResetCanvasScaler();
            }
        }

        public void OnDispose()
        {
            CloseAll();
            m_UIInfoDict.Clear();
            m_CanvasDict.Clear();
        }

        #endregion

        #region 初始化

        private void InitUIRoot()
        {
            if (m_UIRoot == null)
            {
                Debug.LogError("[UIManager] UIRoot 未赋值，请在 Inspector 中拖入场景中的 UIRoot");
                return;
            }

            m_UICamera = m_UIRoot.UICamera;

            // 收集所有 Canvas 到字典
            foreach (var canvas in m_UIRoot.CanvasPanelList)
            {
                m_CanvasDict.Add(canvas.gameObject.name, canvas.GetComponent<RectTransform>());
            }

            m_CanvasScaler = m_UIRoot.CanvasPanelList[0].GetComponent<CanvasScaler>();
            DontDestroyOnLoad(m_UIRoot.gameObject);
        }

        private void RegisterAll()
        {
            foreach (var config in UIDefine.All)
            {
                Register(config);
            }
        }

        #endregion

        #region 注册

        /// <summary>
        /// 注册面板配置
        /// </summary>
        public void Register(UIConfig config)
        {
            if (m_UIInfoDict.ContainsKey(config.ID))
            {
                Debug.LogError($"[UIManager] 重复注册面板 ID:{config.ID}");
                return;
            }
            m_UIInfoDict.Add(config.ID, new UIInfo(config));
        }

        #endregion

        #region 打开 / 关闭

        /// <summary>
        /// 打开面板
        /// </summary>
        public void OpenUI(UIConfig config, object param = null)
        {
            if (!m_UIInfoDict.TryGetValue(config.ID, out var info))
            {
                Debug.LogError($"[UIManager] 面板未注册 ID:{config.ID}");
                return;
            }

            // 已经在显示中，直接返回
            if (info.state == UIState.Show)
            {
                return;
            }

            // 已隐藏，直接重新显示
            if (info.state == UIState.Hide)
            {
                Show(info, param);
                return;
            }

            // 正在加载中，忽略
            if (info.state == UIState.Loading)
            {
                return;
            }

            // 首次打开，加载预制体
            info.state = UIState.Loading;
            LoadAndCreatePanel(info, param);
        }
        public async UniTaskVoid OpenUIAsync(UIConfig config, object param = null)
        {
            if (!m_UIInfoDict.TryGetValue(config.ID, out var info))
            {
                Debug.LogError($"[UIManager] 面板未注册 ID:{config.ID}");
                return;
            }

            // 已经在显示中，直接返回
            if (info.state == UIState.Show)
            {
                return;
            }

            // 已隐藏，直接重新显示
            if (info.state == UIState.Hide)
            {
                Show(info, param);
                return;
            }

            // 正在加载中，忽略
            if (info.state == UIState.Loading)
            {
                return;
            }
            info.state = UIState.Loading;
            LoadAndCreatePanelAsync(info, null).Forget();
        }
        /// <summary>
        /// 关闭面板（销毁）
        /// </summary>
        public void CloseUI(int uid)
        {
            if (!m_UIInfoDict.TryGetValue(uid, out var info))
            {
                return;
            }

            if (info.state == UIState.None || info.state == UIState.Loading)
            {
                info.state = UIState.None;
                return;
            }

            // 从栈中移除
            RemoveFromStack(uid);

            // 调用生命周期
            if (info.ctrl != null)
            {
                info.ctrl.isShow = false;
                info.ctrl.OnHide();
                info.ctrl.OnDestroy();
            }

            info.view?.OnDestroy();

            // 释放资源
            if (info.go != null)
            {
                var assetManager = GameEntry.Instance.Get<AssetManager>();
                assetManager.ReleaseObj(info.go);
            }

            info.Clear();
        }

        /// <summary>
        /// 通过 UIConfig 关闭面板
        /// </summary>
        public void CloseUI(UIConfig config)
        {
            CloseUI(config.ID);
        }

        /// <summary>
        /// 隐藏面板（不销毁，可快速恢复）
        /// </summary>
        public void HideUI(UIConfig config)
        {
            if (!m_UIInfoDict.TryGetValue(config.ID, out var info))
            {
                return;
            }

            if (info.state != UIState.Show)
            {
                return;
            }

            RemoveFromStack(config.ID);

            if (info.canvasGroup != null)
            {
                info.canvasGroup.alpha = 0f;
                info.canvasGroup.blocksRaycasts = false;
            }
            else if (info.go != null)
            {
                info.go.SetActive(false);
            }

            info.state = UIState.Hide;
            info.ctrl.isShow = false;
            info.ctrl.OnHide();
        }

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAll()
        {
            // 复制 key 列表，避免遍历时修改字典
            var keys = new List<int>(m_UIInfoDict.Keys);
            foreach (var uid in keys)
            {
                CloseUI(uid);
            }
            m_PanelStack.Clear();
        }

        /// <summary>
        /// 面板是否正在显示
        /// </summary>
        public bool IsShow(UIConfig config)
        {
            if (m_UIInfoDict.TryGetValue(config.ID, out var info))
            {
                return info.state == UIState.Show;
            }
            return false;
        }

        /// <summary>
        /// 获取面板的 Ctrl
        /// </summary>
        public T GetCtrl<T>(UIConfig config) where T : UICtrl
        {
            if (m_UIInfoDict.TryGetValue(config.ID, out var info))
            {
                return info.ctrl as T;
            }
            return null;
        }

        #endregion

        #region 内部方法

        private void LoadAndCreatePanel(UIInfo info, object param)
        {
            var assetManager = GameEntry.Instance.Get<AssetManager>();
            var layerName = info.config.Layer.ToString();

            if (!m_CanvasDict.TryGetValue(layerName, out var canvasRect))
            {
                Debug.LogError($"[UIManager] 找不到层级 Canvas: {layerName}");
                info.state = UIState.None;
                return;
            }

            var go = assetManager.LoadAsset<GameObject>(info.config.PrefabPath, canvasRect);
            if (go == null)
            {
                Debug.LogError($"[UIManager] 加载面板失败: {info.config.PrefabPath}");
                info.state = UIState.None;
                return;
            }

            // 如果在加载期间被关闭了
            if (info.state == UIState.None)
            {
                assetManager.ReleaseObj(go);
                return;
            }

            // 设置 RectTransform 铺满父节点
            var rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.localPosition = Vector3.zero;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            // 确保有 CanvasGroup（用于显隐控制）
            var canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = go.AddComponent<CanvasGroup>();
            }

            // 先隐藏，等生命周期走完再显示
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            // 填充 UIInfo
            info.go = go;
            info.canvasGroup = canvasGroup;

            // 通过 UIConfig 的工厂方法创建 MVC 实例（泛型，零反射）
            CreateMVC(info, param);
        }

        private async UniTaskVoid LoadAndCreatePanelAsync(UIInfo info, object param)
        {
            var assetManager = GameEntry.Instance.Get<AssetManager>();
            var layerName = info.config.Layer.ToString();

            if (!m_CanvasDict.TryGetValue(layerName, out var canvasRect))
            {
                Debug.LogError($"[UIManager] 找不到层级 Canvas: {layerName}");
                info.state = UIState.None;
                return;
            }

            var go = await assetManager.LoadAssetAsync<GameObject>(info.config.PrefabPath, canvasRect);
            if (go == null)
            {
                Debug.LogError($"[UIManager] 加载面板失败: {info.config.PrefabPath}");
                info.state = UIState.None;
                return;
            }

            // 如果在加载期间被关闭了
            if (info.state == UIState.None)
            {
                assetManager.ReleaseObj(go);
                return;
            }

            // 设置 RectTransform 铺满父节点
            var rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.localPosition = Vector3.zero;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            // 确保有 CanvasGroup（用于显隐控制）
            var canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = go.AddComponent<CanvasGroup>();
            }

            // 先隐藏，等生命周期走完再显示
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            // 填充 UIInfo
            info.go = go;
            info.canvasGroup = canvasGroup;

            // 通过 UIConfig 的工厂方法创建 MVC 实例（泛型，零反射）
            CreateMVC(info, param);
        }

        private void CreateMVC(UIInfo info, object param)
        {
            // 通过 UIConfig 中注册的工厂委托创建实例
            info.ctrl = info.config.CreateCtrl();
            info.view = info.config.CreateView();
            info.model = info.config.CreateModel();

            // 初始化 View
            info.view.Init(info.go);
            info.view.FindComponents();

            // 初始化 Ctrl
            info.ctrl.uid = info.uid;
            info.ctrl.view = info.view;
            info.ctrl.model = info.model;

            if (info.model != null)
            {
                info.model.openParams = param;
            }

            info.ctrl.OnCreate();
            info.ctrl.BindUIEvent();

            // 显示
            Show(info, param);
        }

        private void Show(UIInfo info, object param)
        {
            if (info.model != null)
            {
                info.model.openParams = param;
            }

            // 压栈
            var layer = info.config.Layer;
            if ((layer == UILayer.First || layer == UILayer.Second) && !m_PanelStack.Contains(info.uid))
            {
                m_PanelStack.Push(info.uid);
            }

            // 显示
            if (info.canvasGroup != null)
            {
                info.canvasGroup.alpha = 1f;
                info.canvasGroup.blocksRaycasts = true;
            }
            else if (info.go != null)
            {
                info.go.SetActive(true);
            }

            // 置顶
            info.go.transform.SetAsLastSibling();

            info.state = UIState.Show;

            if (info.ctrl != null)
            {
                info.ctrl.isShow = true;
                info.ctrl.OnShow();
            }
        }

        private void RemoveFromStack(int uid)
        {
            if (m_PanelStack.Count == 0) return;

            // 如果是栈顶直接弹出
            if (m_PanelStack.Peek() == uid)
            {
                m_PanelStack.Pop();
                return;
            }

            // 不在栈顶，重建栈（移除指定元素）
            var temp = new Stack<int>();
            while (m_PanelStack.Count > 0)
            {
                var top = m_PanelStack.Pop();
                if (top != uid)
                {
                    temp.Push(top);
                }
            }
            while (temp.Count > 0)
            {
                m_PanelStack.Push(temp.Pop());
            }
        }

        #endregion

        #region 屏幕适配

        private void ResetCanvasScaler()
        {
            m_ScreenWidth = Screen.width;
            m_ScreenHeight = Screen.height;

            // 横屏：宽高比 < 16:9 匹配宽度，否则匹配高度
            var match = m_ScreenWidth / m_ScreenHeight < 16f / 9f ? 0f : 1f;

            ScaleFactor = match == 0f
                ? m_ScreenWidth / m_CanvasScaler.referenceResolution.x
                : m_ScreenHeight / m_CanvasScaler.referenceResolution.y;

            foreach (var kvp in m_CanvasDict)
            {
                var scaler = kvp.Value.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.matchWidthOrHeight = match;
                }
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取 UI 相机
        /// </summary>
        public Camera GetUICamera() => m_UICamera;

        /// <summary>
        /// 控制某层 Canvas 的显隐
        /// </summary>
        public void SetCanvasVisible(UILayer layer, bool visible)
        {
            var layerName = layer.ToString();
            if (m_CanvasDict.TryGetValue(layerName, out var rect))
            {
                rect.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 获取指定层级的 Canvas Transform
        /// </summary>
        public RectTransform GetCanvasTransform(UILayer layer)
        {
            var layerName = layer.ToString();
            m_CanvasDict.TryGetValue(layerName, out var rect);
            return rect;
        }

        #endregion
    }
}