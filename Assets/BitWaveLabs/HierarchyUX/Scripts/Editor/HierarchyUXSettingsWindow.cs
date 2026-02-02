using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyUXSettingsWindow : EditorWindow
    {
        private const string SettingsAssetPath = HierarchyUX.DataBasePath + "HierarchyUXSettings.asset";
        private const string PrefsPrefix = "HierarchyUX_";

        private bool _useProjectSettings;
        private bool _showTreeLines = true;
        private bool _showGameObjectIcons = true;
        private bool _showButtons = true;
        private bool _showAlternatingRows = true;
        private int _defaultFontSize = 12;
        private Color _defaultFontColor = Color.white;
        private Color _defaultBackgroundColor = Color.gray;

        public struct Settings
        {
            public bool ShowTreeLines;
            public bool ShowGameObjectIcons;
            public bool ShowButtons;
            public bool ShowAlternatingRows;
            public int DefaultFontSize;
            public Color DefaultFontColor;
            public Color DefaultBackgroundColor;
        }
        
        public static Settings GetSettings()
        {
            bool useProjectSettings = EditorPrefs.GetBool(PrefsPrefix + "UseProjectSettings", false);

            if (useProjectSettings)
            {
                HierarchyUXSettings settings = AssetDatabase.LoadAssetAtPath<HierarchyUXSettings>(SettingsAssetPath);
                
                if (settings)
                {
                    return new Settings
                    {
                        ShowTreeLines = settings.showTreeLines,
                        ShowGameObjectIcons = settings.showGameObjectIcons,
                        ShowButtons = settings.showButtons,
                        ShowAlternatingRows = settings.showAlternatingRows,
                        DefaultFontSize = settings.defaultFontSize,
                        DefaultFontColor = settings.defaultFontColor,
                        DefaultBackgroundColor = settings.defaultBackgroundColor
                    };
                }
            }

            // Load from EditorPrefs (or return defaults if not set)
            return new Settings
            {
                ShowTreeLines = EditorPrefs.GetBool(PrefsPrefix + "ShowTreeLines", true),
                ShowGameObjectIcons = EditorPrefs.GetBool(PrefsPrefix + "ShowGameObjectIcons", true),
                ShowButtons = EditorPrefs.GetBool(PrefsPrefix + "ShowButtons", true),
                ShowAlternatingRows = EditorPrefs.GetBool(PrefsPrefix + "ShowAlternatingRows", true),
                DefaultFontSize = EditorPrefs.GetInt(PrefsPrefix + "DefaultFontSize", 12),
                DefaultFontColor = LoadColorFromPrefsStatic("DefaultFontColor", Color.white),
                DefaultBackgroundColor = LoadColorFromPrefsStatic("DefaultBackgroundColor", Color.gray)
            };
        }

        private static Color LoadColorFromPrefsStatic(string key, Color defaultValue)
        {
            float r = EditorPrefs.GetFloat(PrefsPrefix + key + "_R", defaultValue.r);
            float g = EditorPrefs.GetFloat(PrefsPrefix + key + "_G", defaultValue.g);
            float b = EditorPrefs.GetFloat(PrefsPrefix + key + "_B", defaultValue.b);
            float a = EditorPrefs.GetFloat(PrefsPrefix + key + "_A", defaultValue.a);
            return new Color(r, g, b, a);
        }
        
        [MenuItem("Tools/Hierarchy UX")]
        private static void ShowWindow()
        {
            HierarchyUXSettingsWindow window = GetWindow<HierarchyUXSettingsWindow>();
            window.titleContent = new GUIContent("Hierarchy UX Settings");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Hierarchy UX Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            _useProjectSettings = EditorGUILayout.Toggle("Use Project Settings", _useProjectSettings);
            if (EditorGUI.EndChangeCheck())
            {
                // Save the storage preference immediately, then reload settings from the new source
                EditorPrefs.SetBool(PrefsPrefix + "UseProjectSettings", _useProjectSettings);
                LoadSettings();
            }

            EditorGUILayout.HelpBox(
                _useProjectSettings
                    ? "Settings will be saved to a ScriptableObject in the project (shareable with team)."
                    : "Settings will be saved to EditorPrefs (user-specific, persists across projects).",
                MessageType.Info);

            EditorGUILayout.Space(10);

            _showTreeLines = EditorGUILayout.Toggle("Show Tree Lines", _showTreeLines);
            _showGameObjectIcons = EditorGUILayout.Toggle("Show GameObject Icons", _showGameObjectIcons);
            _showButtons = EditorGUILayout.Toggle("Show Buttons", _showButtons);
            _showAlternatingRows = EditorGUILayout.Toggle("Show Alternating Rows", _showAlternatingRows);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Separator Defaults", EditorStyles.boldLabel);

            _defaultFontSize = EditorGUILayout.IntField("Default Font Size", _defaultFontSize);
            _defaultFontColor = EditorGUILayout.ColorField("Default Font Color", _defaultFontColor);
            _defaultBackgroundColor = EditorGUILayout.ColorField("Default Background Color", _defaultBackgroundColor);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save Hierarchy UX Settings", GUILayout.Height(30)))
            {
                SaveSettings();
            }
        }

        private void LoadSettings()
        {
            _useProjectSettings = EditorPrefs.GetBool(PrefsPrefix + "UseProjectSettings", false);

            if (_useProjectSettings)
                LoadFromScriptableObject();
            else
                LoadFromEditorPrefs();
        }

        private void SaveSettings()
        {
            if (_useProjectSettings)
                SaveToScriptableObject();
            else
                SaveToEditorPrefs();

            Debug.Log("Hierarchy UX Settings saved.");
            EditorApplication.RepaintHierarchyWindow();
        }

        private void LoadFromEditorPrefs()
        {
            _showTreeLines = EditorPrefs.GetBool(PrefsPrefix + "ShowTreeLines", true);
            _showGameObjectIcons = EditorPrefs.GetBool(PrefsPrefix + "ShowGameObjectIcons", true);
            _showButtons = EditorPrefs.GetBool(PrefsPrefix + "ShowButtons", true);
            _showAlternatingRows = EditorPrefs.GetBool(PrefsPrefix + "ShowAlternatingRows", true);
            _defaultFontSize = EditorPrefs.GetInt(PrefsPrefix + "DefaultFontSize", 12);
            _defaultFontColor = LoadColorFromPrefs("DefaultFontColor", Color.white);
            _defaultBackgroundColor = LoadColorFromPrefs("DefaultBackgroundColor", Color.gray);
        }

        private void SaveToEditorPrefs()
        {
            EditorPrefs.SetBool(PrefsPrefix + "ShowTreeLines", _showTreeLines);
            EditorPrefs.SetBool(PrefsPrefix + "ShowGameObjectIcons", _showGameObjectIcons);
            EditorPrefs.SetBool(PrefsPrefix + "ShowButtons", _showButtons);
            EditorPrefs.SetBool(PrefsPrefix + "ShowAlternatingRows", _showAlternatingRows);
            EditorPrefs.SetInt(PrefsPrefix + "DefaultFontSize", _defaultFontSize);
            SaveColorToPrefs("DefaultFontColor", _defaultFontColor);
            SaveColorToPrefs("DefaultBackgroundColor", _defaultBackgroundColor);
        }

        private void LoadFromScriptableObject()
        {
            HierarchyUXSettings settings = AssetDatabase.LoadAssetAtPath<HierarchyUXSettings>(SettingsAssetPath);

            if (settings)
            {
                _showTreeLines = settings.showTreeLines;
                _showGameObjectIcons = settings.showGameObjectIcons;
                _showButtons = settings.showButtons;
                _showAlternatingRows = settings.showAlternatingRows;
                _defaultFontSize = settings.defaultFontSize;
                _defaultFontColor = settings.defaultFontColor;
                _defaultBackgroundColor = settings.defaultBackgroundColor;
            }
        }

        private void SaveToScriptableObject()
        {
            HierarchyUXSettings settings = AssetDatabase.LoadAssetAtPath<HierarchyUXSettings>(SettingsAssetPath);

            if (!settings)
            {
                settings = CreateInstance<HierarchyUXSettings>();

                string directory = System.IO.Path.GetDirectoryName(SettingsAssetPath);
                if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            }

            settings.showTreeLines = _showTreeLines;
            settings.showGameObjectIcons = _showGameObjectIcons;
            settings.showButtons = _showButtons;
            settings.showAlternatingRows = _showAlternatingRows;
            settings.defaultFontSize = _defaultFontSize;
            settings.defaultFontColor = _defaultFontColor;
            settings.defaultBackgroundColor = _defaultBackgroundColor;

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private Color LoadColorFromPrefs(string key, Color defaultValue)
        {
            float r = EditorPrefs.GetFloat(PrefsPrefix + key + "_R", defaultValue.r);
            float g = EditorPrefs.GetFloat(PrefsPrefix + key + "_G", defaultValue.g);
            float b = EditorPrefs.GetFloat(PrefsPrefix + key + "_B", defaultValue.b);
            float a = EditorPrefs.GetFloat(PrefsPrefix + key + "_A", defaultValue.a);
            return new Color(r, g, b, a);
        }

        private void SaveColorToPrefs(string key, Color color)
        {
            EditorPrefs.SetFloat(PrefsPrefix + key + "_R", color.r);
            EditorPrefs.SetFloat(PrefsPrefix + key + "_G", color.g);
            EditorPrefs.SetFloat(PrefsPrefix + key + "_B", color.b);
            EditorPrefs.SetFloat(PrefsPrefix + key + "_A", color.a);
        }
    }
}