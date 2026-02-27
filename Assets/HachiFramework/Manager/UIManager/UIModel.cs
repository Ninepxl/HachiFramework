namespace HachiFramework
{
    /// <summary>
    /// UI 数据层基类
    /// 存储面板的状态数据，子类按需扩展字段
    /// </summary>
    public class UIModel
    {
        /// <summary>
        /// 面板打开时传入的参数
        /// </summary>
        public object openParams;

        /// <summary>
        /// 数据重置（面板关闭时可选调用）
        /// </summary>
        public virtual void Reset()
        {
            openParams = null;
        }
    }
}