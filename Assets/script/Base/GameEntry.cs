using HachiFramework;
namespace ActGame
{
    public static class GameEntry
    {
        public static AssetManager assetManager => HachiFramework.GameEntry.Instance.Get<AssetManager>();
        public static GamePoolManager poolManager => HachiFramework.GameEntry.Instance.Get<GamePoolManager>();
        public static UIManager uIManager => HachiFramework.GameEntry.Instance.Get<UIManager>();

        public static InputManager inputManager => HachiFramework.GameEntry.Instance.Get<InputManager>();
    }
}