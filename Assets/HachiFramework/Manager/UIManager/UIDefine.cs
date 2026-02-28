using System;

namespace HachiFramework
{
    /// <summary>
    /// UI 层级枚举，名字必须和 UIRoot 预制体中 Canvas 节点名一致
    /// </summary>
    public enum UILayer
    {
        Hud,
        Base,
        First,
        Second,
        Float,
        Notice,
        Loading,
    }

    /// <summary>
    /// 单个面板的静态配置（不可变）
    /// 通过 UIConfig.Create 泛型方法创建，编译期保证类型安全，运行时零反射
    /// </summary>
    public class UIConfig
    {
        public readonly int ID;
        public readonly UILayer Layer;
        public readonly string PrefabPath;
        public readonly Func<UICtrl> CreateCtrl;
        public readonly Func<UIView> CreateView;
        public readonly Func<UIModel> CreateModel;

        private UIConfig(int id, UILayer layer, string prefabPath,
            Func<UICtrl> createCtrl, Func<UIView> createView, Func<UIModel> createModel)
        {
            ID = id;
            Layer = layer;
            PrefabPath = prefabPath;
            CreateCtrl = createCtrl;
            CreateView = createView;
            CreateModel = createModel;
        }

        /// <summary>
        /// 创建面板配置（带 Model）
        /// </summary>
        public static UIConfig Create<TCtrl, TView, TModel>(int id, UILayer layer, string prefabPath)
            where TCtrl : UICtrl, new()
            where TView : UIView, new()
            where TModel : UIModel, new()
        {
            return new UIConfig(id, layer, prefabPath, () => new TCtrl(), () => new TView(), () => new TModel());
        }

        /// <summary>
        /// 创建面板配置（无 Model，使用默认 UIModel）
        /// </summary>
        public static UIConfig Create<TCtrl, TView>(int id, UILayer layer, string prefabPath)
            where TCtrl : UICtrl, new()
            where TView : UIView, new()
        {
            return new UIConfig(id, layer, prefabPath, () => new TCtrl(), () => new TView(), () => new UIModel());
        }
    }

    /// <summary>
    /// 所有面板的配置注册表
    /// 新增面板：1.加静态字段 2.加到 All 数组
    /// 
    /// 示例：
    /// public static readonly UIConfig Login = UIConfig.Create&lt;LoginCtrl, LoginView, LoginModel&gt;(
    ///     1, UILayer.First, "UI/UILogin.prefab"
    /// );
    /// </summary>
    public static class UIDefine
    {
        // public static readonly UIConfig Login = UIConfig.Create<LoginCtrl, LoginView, LoginModel>(
        //     1, UILayer.First, "UI/UILogin.prefab"
        // );

        public static readonly UIConfig MenuUI = UIConfig.Create<MenuUICtrl, MenuUIView, MenuUIModel>(
            1, UILayer.First, "Prefabs/UI/UIMenuPanel.prefab"
        );

        /// <summary>
        /// 所有面板配置，UIManager 启动时遍历注册
        /// </summary>
        public static readonly UIConfig[] All = new UIConfig[]
        {
                MenuUI,
            // Login,
        };
    }
}