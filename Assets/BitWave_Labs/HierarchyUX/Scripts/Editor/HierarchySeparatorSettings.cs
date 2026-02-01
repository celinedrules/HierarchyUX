using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchySeparatorSettings : EditorWindow
    {
        private GameObject _targetObject;
        private Color _selectedColor = Color.cyan;
        private Color _fontColor = Color.white;
        private int _fontSize = 12;

        public static void Show(GameObject target, Color initialColor, Color initialFontColor, int initialFontSize)
        {
            // Close any existing instance first
            if (HasOpenInstances<HierarchySeparatorSettings>())
                GetWindow<HierarchySeparatorSettings>().Close();

            HierarchySeparatorSettings window = CreateInstance<HierarchySeparatorSettings>();
            window.titleContent = new GUIContent("Separator Settings");
            window._targetObject = target;
            window._selectedColor = initialColor;
            window._fontColor = initialFontColor;
            window._fontSize = initialFontSize;
            window.minSize = new Vector2(250, 170);
            window.maxSize = new Vector2(250, 170);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            if (!_targetObject)
            {
                Close();
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Separator Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _fontSize = EditorGUILayout.IntField("Font Size", _fontSize);
            //_fontSize = Mathf.Clamp(_fontSize, 8, 24);
            _fontColor = EditorGUILayout.ColorField("Font Color", _fontColor);

            EditorGUILayout.Space(5);

            _selectedColor = EditorGUILayout.ColorField("Background Color", _selectedColor);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply", GUILayout.Height(30)))
                {
                    ApplySettings();
                    Close();
                }

                if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                    Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ApplySettings()
        {
            HierarchySeparatorData data =
                AssetDatabase.LoadAssetAtPath<HierarchySeparatorData>(HierarchySeparator.SeparatorDataPath);

            if (!data)
            {
                data = CreateInstance<HierarchySeparatorData>();

                if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                    AssetDatabase.CreateFolder("Assets", "Editor");

                AssetDatabase.CreateAsset(data, "Assets/Editor/HierarchySeparatorData.asset");
            }

            data.SetSeparator(_targetObject.GetInstanceID(), _selectedColor, _fontColor, _fontSize);

            _targetObject.transform.hideFlags = HideFlags.HideInInspector;

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}