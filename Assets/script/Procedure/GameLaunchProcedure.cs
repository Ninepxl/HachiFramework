using UnityEngine;
using HachiFramework;
using YooAsset;
using Cysharp.Threading.Tasks;

namespace ActGame
{
    public class GameLaunchProcedure : ProcedureBase
    {
        private bool m_isInitialized = false;
        private EPlayMode PlayMode => EPlayMode.EditorSimulateMode;
        private string PackageName => ResPackageDefine.DefaultPackage;

        public override void OnEnter()
        {
            base.OnEnter();
            m_isInitialized = false;
            InitPackAsync().Forget();
        }

        public override void OnUpdate()
        {
            if (m_isInitialized)
            {
                ChangeState<MainProcedure>();
            }
        }

        private async UniTaskVoid InitPackAsync()
        {
            // 初始化资源系统
            YooAssets.Initialize();
            var package = YooAssets.TryGetPackage(PackageName) ?? YooAssets.CreatePackage(PackageName);
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(package);

            var buildResult = EditorSimulateModeHelper.SimulateBuild(PackageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = fileSystemParams;

            var initOperation = package.InitializeAsync(createParameters);
            await initOperation.ToUniTask();

            if (initOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
                return;
            }
            Debug.Log("资源包初始化成功！");

            // 请求资源版本
            var requestVersionOp = package.RequestPackageVersionAsync();
            await requestVersionOp.ToUniTask();

            if (requestVersionOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"请求资源版本失败：{requestVersionOp.Error}");
                return;
            }
            Debug.Log($"资源版本: {requestVersionOp.PackageVersion}");

            // 更新资源清单
            var updateManifestOp = package.UpdatePackageManifestAsync(requestVersionOp.PackageVersion);
            await updateManifestOp.ToUniTask();

            if (updateManifestOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"更新资源清单失败：{updateManifestOp.Error}");
                return;
            }
            Debug.Log("资源清单更新成功！");

            m_isInitialized = true;
        }
    }
}
