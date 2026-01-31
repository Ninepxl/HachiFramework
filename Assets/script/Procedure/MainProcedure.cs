using Cysharp.Threading.Tasks;
using HachiFramework;
using UnityEngine;

namespace ActGame
{
    public class MainProcedure : ProcedureBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            LoadPlayerAndScene().Forget();
        }
        private async UniTaskVoid LoadPlayerAndScene()
        {
            await GameEntry.assetManager.LoadAssetAsync<GameObject>("Cube.prefab", null);
        }
    }
}