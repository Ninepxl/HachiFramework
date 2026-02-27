namespace HachiFramework
{
    /// <summary>
    /// UI 面板 Update 接口
    /// UICtrl 子类实现此接口后，UIManager 会在每帧对显示中的面板调用 OnUpdate
    /// </summary>
    public interface IUIUpdate
    {
        void OnUpdate();
    }
}
