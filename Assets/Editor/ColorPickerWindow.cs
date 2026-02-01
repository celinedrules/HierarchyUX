using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ColorPickerWindow : EditorWindow
    {
        private GameObject _targetObject;
        private Color _selectedColor = Color.cyan;

        public static void Show(GameObject target, Color initialColor)
        {
            ColorPickerWindow window = GetWindow<ColorPickerWindow>("Pick Separator Color");
            window._targetObject = target;
            window._selectedColor = initialColor;
            window.minSize = new Vector2(250, 100);
            window.maxSize = new Vector2(250, 100);
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
            EditorGUILayout.LabelField("Select Separator Color", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _selectedColor = EditorGUILayout.ColorField("Color", _selectedColor);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply", GUILayout.Height(30)))
                {
                    ApplyColor();
                    Close();
                }

                if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                    Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ApplyColor()
        {
            HierarchySeparatorData data = AssetDatabase.LoadAssetAtPath<HierarchySeparatorData>("Assets/Editor/HierarchySeparatorData.asset");
            
            if (!data)
            {
                data = CreateInstance<HierarchySeparatorData>();
            
                if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                    AssetDatabase.CreateFolder("Assets", "Editor");
            
                AssetDatabase.CreateAsset(data, "Assets/Editor/HierarchySeparatorData.asset");
            }

            data.SetSeparator(_targetObject.GetInstanceID(), _selectedColor);

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}