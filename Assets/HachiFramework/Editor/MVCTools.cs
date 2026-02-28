using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HachiFramework.Editor
{
    /// <summary>
    /// MVC 脚本生成工具
    /// 一键生成 Ctrl / View / Model 三个文件，并自动在 UIDefine.cs 中追加配置
    /// 菜单：Tools/UI MVC Tools
    /// </summary>
    public class MVCTools : EditorWindow
    {
        // ======================== 路径配置 ========================
        /// <summary>UI 脚本根目录（Ctrl/View/Model 生成到这里）</summary>
        private const string ScriptRootPath = "Assets/script/UI/";

        /// <summary>UI 预制体根目录</summary>
        private const string PrefabRootPath = "Assets/Res/Prefabs/UI/";

        /// <summary>UIDefine.cs 文件路径</summary>
        private const string UIDefinePath = "Assets/HachiFramework/Manager/UIManager/UIDefine.cs";

        // ======================== 输入字段 ========================
        private string m_UIName = "";
        private string m_Author = "Author";
        private UILayerOption m_Layer = UILayerOption.First;
        private bool m_CreateCtrl = true;
        private bool m_CreateView = true;
        private bool m_CreateModel = true;

        private enum UILayerOption
        {
            Hud,
            Base,
            First,
            Second,
            Float,
            Notice,
            Loading,
        }

        [MenuItem("Tools/UI MVC Tools")]
        public static void ShowWindow()
        {
            GetWindow<MVCTools>("UI MVC Tools");
        }

        private void OnGUI()
        {
            GUILayout.Label("UI MVC 脚本生成工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            m_Author = EditorGUILayout.TextField("作者", m_Author);
            GUILayout.Space(5);

            m_UIName = EditorGUILayout.TextField("面板名（如 Login）", m_UIName);
            GUILayout.Space(5);

            m_Layer = (UILayerOption)EditorGUILayout.EnumPopup("UI 层级", m_Layer);
            GUILayout.Space(10);

            m_CreateCtrl = EditorGUILayout.Toggle("生成 Ctrl", m_CreateCtrl);
            m_CreateView = EditorGUILayout.Toggle("生成 View", m_CreateView);
            m_CreateModel = EditorGUILayout.Toggle("生成 Model", m_CreateModel);

            GUILayout.Space(20);

            // 预览
            EditorGUILayout.LabelField("预览生成路径：", EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(m_UIName))
            {
                string dir = $"{ScriptRootPath}{m_UIName}/";
                if (m_CreateCtrl) EditorGUILayout.LabelField($"  {dir}{m_UIName}Ctrl.cs");
                if (m_CreateView) EditorGUILayout.LabelField($"  {dir}{m_UIName}View.cs");
                if (m_CreateModel) EditorGUILayout.LabelField($"  {dir}{m_UIName}Model.cs");
                EditorGUILayout.LabelField($"  UIDefine.cs → 追加 {m_UIName} 配置");
            }

            GUILayout.Space(20);

            GUI.enabled = !string.IsNullOrEmpty(m_UIName) && (m_CreateCtrl || m_CreateView || m_CreateModel);
            if (GUILayout.Button("生成", GUILayout.Height(30)))
            {
                Generate();
            }
            GUI.enabled = true;
        }

        private void Generate()
        {
            if (!ValidateName()) return;

            string scriptDir = Path.Combine(Application.dataPath, $"script/UI/{m_UIName}");
            if (!Directory.Exists(scriptDir))
            {
                Directory.CreateDirectory(scriptDir);
            }

            int created = 0;

            if (m_CreateCtrl)
            {
                CreateFile(scriptDir, $"{m_UIName}Ctrl.cs", GenerateCtrl());
                created++;
            }

            if (m_CreateView)
            {
                CreateFile(scriptDir, $"{m_UIName}View.cs", GenerateView());
                created++;
            }

            if (m_CreateModel)
            {
                CreateFile(scriptDir, $"{m_UIName}Model.cs", GenerateModel());
                created++;
            }

            // 追加 UIDefine 配置
            AppendUIDefine();

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("UI MVC Tools",
                $"{m_UIName} 生成完成！\n创建了 {created} 个脚本文件\n已更新 UIDefine.cs",
                "确定");
        }

        #region 验证

        private bool ValidateName()
        {
            if (string.IsNullOrWhiteSpace(m_UIName))
            {
                EditorUtility.DisplayDialog("错误", "面板名不能为空", "确定");
                return false;
            }

            if (!Regex.IsMatch(m_UIName, @"^[A-Z][a-zA-Z0-9]*$"))
            {
                EditorUtility.DisplayDialog("错误", "面板名必须以大写字母开头，只能包含字母和数字\n如：Login、Setting、MessageBox", "确定");
                return false;
            }

            // 检查是否已存在
            string scriptDir = Path.Combine(Application.dataPath, $"script/UI/{m_UIName}");
            if (Directory.Exists(scriptDir))
            {
                return EditorUtility.DisplayDialog("警告", $"目录 {m_UIName} 已存在，是否覆盖？", "覆盖", "取消");
            }

            return true;
        }

        #endregion

        #region 生成 Ctrl

        private string GenerateCtrl()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"// Auto generated by MVCTools - {m_Author} @ {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine($"// {m_UIName} 控制层");
            sb.AppendLine();
            sb.AppendLine("namespace HachiFramework");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {m_UIName}Ctrl : UICtrl");
            sb.AppendLine("    {");
            sb.AppendLine($"        private new {m_UIName}View view => base.view as {m_UIName}View;");
            if (m_CreateModel)
            {
                sb.AppendLine($"        private new {m_UIName}Model model => base.model as {m_UIName}Model;");
            }
            sb.AppendLine();
            sb.AppendLine("        public override void OnCreate()");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void BindUIEvent()");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void OnShow()");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void OnHide()");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void OnDestroy()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.OnDestroy();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        #endregion

        #region 生成 View

        private string GenerateView()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"// Auto generated by MVCTools - {m_Author} @ {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine($"// {m_UIName} 视图层");
            sb.AppendLine();
            sb.AppendLine("namespace HachiFramework");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {m_UIName}View : UIView");
            sb.AppendLine("    {");
            sb.AppendLine("        // 在这里声明 UI 组件引用");
            sb.AppendLine("        // public Button btn_xxx;");
            sb.AppendLine("        // public TextMeshProUGUI txt_xxx;");
            sb.AppendLine();
            sb.AppendLine("        public override void FindComponents()");
            sb.AppendLine("        {");
            sb.AppendLine("            // 在这里查找并缓存 UI 组件");
            sb.AppendLine("            // btn_xxx = transform.Find(\"btn_xxx\").GetComponent<Button>();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void OnDestroy()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.OnDestroy();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        #endregion

        #region 生成 Model

        private string GenerateModel()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"// Auto generated by MVCTools - {m_Author} @ {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine($"// {m_UIName} 数据层");
            sb.AppendLine();
            sb.AppendLine("namespace HachiFramework");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {m_UIName}Model : UIModel");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        #endregion

        #region 追加 UIDefine

        private void AppendUIDefine()
        {
            string fullPath = Path.Combine(Application.dataPath, UIDefinePath.Replace("Assets/", ""));
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[MVCTools] UIDefine.cs 不存在: {fullPath}");
                return;
            }

            string content = File.ReadAllText(fullPath);

            // 检查是否已注册
            if (content.Contains($"UIConfig.Create<{m_UIName}Ctrl"))
            {
                Debug.LogWarning($"[MVCTools] {m_UIName} 已在 UIDefine.cs 中注册，跳过");
                return;
            }

            // 找到当前最大 ID
            int maxId = 0;
            var idMatches = Regex.Matches(content, @"UIConfig\.Create[^(]*\(\s*(\d+)");
            foreach (Match match in idMatches)
            {
                int id = int.Parse(match.Groups[1].Value);
                if (id > maxId) maxId = id;
            }
            int newId = maxId + 1;

            // 构建 Ctrl/View/Model 泛型参数
            string ctrlType = m_CreateCtrl ? $"{m_UIName}Ctrl" : "UICtrl";
            string viewType = m_CreateView ? $"{m_UIName}View" : "UIView";
            string genericArgs = m_CreateModel
                ? $"{ctrlType}, {viewType}, {m_UIName}Model"
                : $"{ctrlType}, {viewType}";

            string layerName = m_Layer.ToString();
            string prefabPath = $"UI/UI{m_UIName}.prefab";

            // 生成静态字段
            string fieldLine = $"        public static readonly UIConfig {m_UIName} = UIConfig.Create<{genericArgs}>(\n" +
                               $"            {newId}, UILayer.{layerName}, \"{prefabPath}\"\n" +
                               $"        );";

            // 插入到 All 数组之前
            string allArrayPattern = @"(        /// <summary>\s*\n\s*/// 所有面板配置.*?\n\s*/// </summary>\s*\n\s*public static readonly UIConfig\[\] All = new UIConfig\[\]\s*\n\s*\{)";
            if (Regex.IsMatch(content, allArrayPattern, RegexOptions.Singleline))
            {
                content = Regex.Replace(content, allArrayPattern,
                    fieldLine + "\n\n$1", RegexOptions.Singleline);
            }

            // 在 All 数组中添加引用
            // 找到 All 数组的 { 后面，插入新条目
            string arrayContentPattern = @"(public static readonly UIConfig\[\] All = new UIConfig\[\]\s*\n\s*\{)(\s*)";
            content = Regex.Replace(content, arrayContentPattern,
                $"$1$2    {m_UIName},$2");

            File.WriteAllText(fullPath, content);
            Debug.Log($"[MVCTools] 已在 UIDefine.cs 中注册 {m_UIName}（ID={newId}）");
        }

        #endregion

        #region 工具方法

        private static void CreateFile(string dir, string fileName, string content)
        {
            string path = Path.Combine(dir, fileName);
            if (File.Exists(path))
            {
                Debug.LogWarning($"[MVCTools] 文件已存在，覆盖: {path}");
            }
            File.WriteAllText(path, content, Encoding.UTF8);
            Debug.Log($"[MVCTools] 创建文件: {path}");
        }

        #endregion
    }
}
