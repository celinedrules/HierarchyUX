using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyFolderSettings : EditorWindow
    {
        private GameObject _targetObject;
        private string _targetObjectName;
        private Color _folderColor = Color.white;

        public static void Show(GameObject target, Color initialColor)
        {
            // Close any existing instance first
            if (HasOpenInstances<HierarchyFolderSettings>())
                GetWindow<HierarchyFolderSettings>().Close();

            HierarchyFolderSettings window = CreateInstance<HierarchyFolderSettings>();
            window.titleContent = new GUIContent("Folder Settings");
            window._targetObject = target;
            window._targetObjectName = target.name;
            window._folderColor = initialColor;
            window.minSize = new Vector2(300, 150);
            window.maxSize = new Vector2(400, 200);
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
            EditorGUILayout.LabelField("Folder Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _targetObjectName = EditorGUILayout.TextField("Name", _targetObjectName);

            EditorGUILayout.Space(5);

            _folderColor = EditorGUILayout.ColorField("Folder Color", _folderColor);

            GUILayout.FlexibleSpace();

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
            HierarchyFolderData data =
                AssetDatabase.LoadAssetAtPath<HierarchyFolderData>(HierarchyFolder.FolderDataPath);

            if (!data)
                return;

            if (_targetObject.name != _targetObjectName)
            {
                Undo.RecordObject(_targetObject, "Rename Folder");
                _targetObject.name = _targetObjectName;
                EditorUtility.SetDirty(_targetObject);
            }

            data.SetFolderColor(_targetObject.GetInstanceID(), _folderColor);
            
            _targetObject.transform.hideFlags = HideFlags.HideInInspector;

            // Update the inspector icon with the tinted color
            HierarchyFolder.SetInspectorIcon(_targetObject, _folderColor);

            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(_targetObject);
            AssetDatabase.SaveAssets();
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}