using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class HierarchySeparator
    {
        private const string SeparatorDataPath = "Assets/Editor/HierarchySeparatorData.asset";
        private static HierarchySeparatorData _data;

        static HierarchySeparator() => EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

        private static HierarchySeparatorData GetData()
        {
            if (!_data)
            {
                _data = AssetDatabase.LoadAssetAtPath<HierarchySeparatorData>(SeparatorDataPath);

                if (!_data)
                {
                    _data = ScriptableObject.CreateInstance<HierarchySeparatorData>();

                    string directory = System.IO.Path.GetDirectoryName(SeparatorDataPath);

                    if (!AssetDatabase.IsValidFolder(directory))
                    {
                        System.IO.Directory.CreateDirectory(directory);
                        AssetDatabase.Refresh();
                    }

                    AssetDatabase.CreateAsset(_data, SeparatorDataPath);
                    AssetDatabase.SaveAssets();
                }
            }

            return _data;
        }

        [MenuItem("GameObject/Hierarchy UX/Create Separator", false, 0)]
        private static void CreateSeparator()
        {
            GameObject selected = Selection.activeGameObject;

            if (!selected)
            {
                Debug.LogWarning("Please select a GameObject in the hierarchy first.");
                return;
            }

            Color currentColor = GetData().GetColor(selected.GetInstanceID());
            ColorPickerWindow.Show(selected, currentColor);
        }

        [MenuItem("GameObject/Hierarchy UX/Remove Separator", false, 1)]
        private static void RemoveSeparator()
        {
            GameObject selected = Selection.activeGameObject;

            if (!selected)
                return;

            selected.transform.hideFlags = HideFlags.None;

            HierarchySeparatorData data = GetData();
            data.RemoveSeparator(selected.GetInstanceID());

            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(selected);
            AssetDatabase.SaveAssets();
            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem("GameObject/Hierarchy UX/Remove Separator", true)]
        private static bool RemoveSeparatorValidate()
        {
            if (!Selection.activeGameObject)
                return false;

            return GetData().IsSeparator(Selection.activeGameObject.GetInstanceID());
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            HierarchySeparatorData data = GetData();

            if (!data.IsSeparator(instanceID))
                return;

            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (!obj)
                return;

            Color backgroundColor = data.GetColor(instanceID);
            bool hasChildren = obj.transform.childCount > 0;

            // Draw full width background
            Rect fullWidthRect = new(32, selectionRect.y, selectionRect.xMax - 32 + 16, selectionRect.height);
            EditorGUI.DrawRect(fullWidthRect, backgroundColor);

            // Calculate text color
            float brightness = backgroundColor.r * 0.299f + backgroundColor.g * 0.587f + backgroundColor.b * 0.114f;
            Color textColor = brightness > 0.5f ? Color.black : Color.white;

            // Draw foldout arrow if has children
            if (hasChildren)
            {
                Rect foldoutRect = new(selectionRect.x - 14, selectionRect.y, 14, selectionRect.height);

                GUIStyle arrowStyle = new(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    normal = { textColor = textColor }
                };

                bool expanded = IsExpanded(instanceID);
                string arrow = expanded ? "▼" : "▶";

                GUI.Label(foldoutRect, arrow, arrowStyle);
            }

            // Draw centered text
            GUIStyle style = new(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textColor }
            };

            EditorGUI.LabelField(fullWidthRect, obj.name, style);
        }

        private static bool IsExpanded(int instanceID)
        {
            Type hierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            EditorWindow window = EditorWindow.GetWindow(hierarchyWindowType, false, null, false);

            if (!window)
                return false;

            FieldInfo sceneHierarchyField = hierarchyWindowType.GetField("m_SceneHierarchy",
                BindingFlags.Instance | BindingFlags.NonPublic);
            object sceneHierarchy = sceneHierarchyField?.GetValue(window);

            if (sceneHierarchy == null)
                return false;

            FieldInfo treeViewStateField = sceneHierarchy.GetType().GetField("m_TreeViewState",
                BindingFlags.Instance | BindingFlags.NonPublic);
            object treeViewState = treeViewStateField?.GetValue(sceneHierarchy);

            if (treeViewState == null)
                return false;

            PropertyInfo expandedIDsProperty = treeViewState.GetType().GetProperty("expandedIDs",
                BindingFlags.Instance | BindingFlags.Public);
            IList expandedIDs = expandedIDsProperty?.GetValue(treeViewState) as IList;

            if (expandedIDs == null)
                return false;

            foreach (object entityId in expandedIDs)
            {
                FieldInfo dataField =
                    entityId.GetType().GetField("m_Data", BindingFlags.Instance | BindingFlags.NonPublic);

                if (dataField != null)
                {
                    int id = (int)dataField.GetValue(entityId);

                    if (id == instanceID)
                        return true;
                }
            }

            return false;
        }
    }
}