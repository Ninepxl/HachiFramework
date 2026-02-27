/// <summary>
/// 启动入口
/// 职责：初始化 YooAsset 资源系统，完成后加载 Main 场景
/// 独立于 GameEntry，确保资源系统在所有 Manager 之前就绪
/// 如有热更需求，可在此扩展热更状态机
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Cysharp.Threading.Tasks;

namespace HachiFramework
{
    public class LaunchEntry : MonoBehaviour
    {
        [Header("资源初始化完成后要加载的场景名")]
        [SerializeField] private string m_MainSceneName = "Main";

        /// <summary>
        /// 当前资源运行模式（正式发布时改为 HostPlayMode 或 OfflinePlayMode）
        /// </summary>
        private EPlayMode PlayMode => EPlayMode.EditorSimulateMode;
        private string PackageName => ResPackageDefine.DefaultPackage;

        private void Start()
        {
            InitAndEnterGame().Forget();
        }

        private async UniTaskVoid InitAndEnterGame()
        {
            bool success = await InitPackageAsync();
            if (!success)
            {
                Debug.LogError("[LaunchEntry] 资源初始化失败，无法进入游戏");
                return;
            }

            Debug.Log($"[LaunchEntry] 资源初始化完成，加载场景: {m_MainSceneName}");
            SceneManager.LoadScene(m_MainSceneName);
        }

        /// <summary>
        /// 初始化 YooAsset 资源包
        /// 如需热更，可在此方法中插入版本检查、下载等步骤
        /// </summary>
        private async UniTask<bool> InitPackageAsync()
        {
            // 1. 初始化资源系统
            YooAssets.Initialize();
            var package = YooAssets.TryGetPackage(PackageName) ?? YooAssets.CreatePackage(PackageName);
            YooAssets.SetDefaultPackage(package);

            // 2. 初始化资源包（编辑器模拟模式）
            // TODO: 正式发布时根据 PlayMode 切换不同的初始化参数
            var buildResult = EditorSimulateModeHelper.SimulateBuild(PackageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = fileSystemParams;

            var initOperation = package.InitializeAsync(createParameters);
            await initOperation.ToUniTask();

            if (initOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[LaunchEntry] 资源包初始化失败: {initOperation.Error}");
                return false;
            }
            Debug.Log("[LaunchEntry] 资源包初始化成功");

            // 3. 请求资源版本
            var requestVersionOp = package.RequestPackageVersionAsync();
            await requestVersionOp.ToUniTask();

            if (requestVersionOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[LaunchEntry] 请求资源版本失败: {requestVersionOp.Error}");
                return false;
            }
            Debug.Log($"[LaunchEntry] 资源版本: {requestVersionOp.PackageVersion}");

            // 4. 更新资源清单
            var updateManifestOp = package.UpdatePackageManifestAsync(requestVersionOp.PackageVersion);
            await updateManifestOp.ToUniTask();

            if (updateManifestOp.Status != EOperationStatus.Succeed)
            {
                Debug.LogError($"[LaunchEntry] 更新资源清单失败: {updateManifestOp.Error}");
                return false;
            }
            Debug.Log("[LaunchEntry] 资源清单更新成功");

            // TODO: 热更扩展点
            // 5. 检查热更资源
            // 6. 下载热更资源
            // 7. 下载完成

            return true;
        }
    }
}