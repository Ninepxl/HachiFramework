using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace HachiFramework.Editor
{
    [Overlay(typeof(SceneView), "Scene Launcher", defaultDisplay = true)]
    public class ScenePlayOverlay : ToolbarOverlay
    {
        ScenePlayOverlay() : base(SceneDropdown.Id) { }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    public class SceneDropdown : EditorToolbarDropdown
    {
        public const string Id = "ScenePlayOverlay/SceneDropdown";

        private const string PrefKey = "HachiFramework_StartScene";

        public SceneDropdown()
        {
            string saved = EditorPrefs.GetString(PrefKey, "");
            text = string.IsNullOrEmpty(saved) ? "Select Scene" : Path.GetFileNameWithoutExtension(saved);
            tooltip = "Choose a scene to play from";
            clicked += ShowDropdown;
        }

        private void ShowDropdown()
        {
            var menu = new GenericMenu();

            string[] guids = AssetDatabase.FindAssets("t:Scene");
            string[] scenePaths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                .OrderBy(p => p)
                .ToArray();

            string current = EditorPrefs.GetString(PrefKey, "");

            foreach (string path in scenePaths)
            {
                string sceneName = Path.GetFileNameWithoutExtension(path);
                string captured = path;
                menu.AddItem(
                    new GUIContent(sceneName),
                    captured == current,
                    () => OnSceneSelected(captured));
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Play Selected Scene"), false, PlaySelectedScene);

            menu.ShowAsContext();
        }

        private void OnSceneSelected(string scenePath)
        {
            EditorPrefs.SetString(PrefKey, scenePath);
            text = Path.GetFileNameWithoutExtension(scenePath);
        }

        private static void PlaySelectedScene()
        {
            string scenePath = EditorPrefs.GetString(PrefKey, "");
            if (string.IsNullOrEmpty(scenePath))
            {
                EditorUtility.DisplayDialog("Scene Launcher", "Please select a scene first.", "OK");
                return;
            }

            if (EditorApplication.isPlaying)
                return;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
                EditorApplication.isPlaying = true;
            }
        }
    }
}
