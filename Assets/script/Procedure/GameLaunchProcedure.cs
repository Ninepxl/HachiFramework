using UnityEngine;
using HachiFramework;

namespace ActGame
{
    /// <summary>
    /// 游戏启动流程（YooAsset 已由 LaunchEntry 初始化完成）
    /// 可在此做框架层面的准备工作，完成后进入 MainProcedure
    /// </summary>
    public class GameLaunchProcedure : ProcedureBase
    {
        public override void OnEnter()
        {
            base.OnEnter();

            // YooAsset 已就绪，可以安全加载资源
            // 如有需要可在此做额外的框架初始化

            Debug.Log("[GameLaunchProcedure] 框架就绪，进入主流程");
            ChangeState<MainProcedure>();
        }
    }
}
