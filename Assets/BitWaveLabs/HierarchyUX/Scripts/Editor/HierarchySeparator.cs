using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public static class HierarchySeparator
    {
        public const string SeparatorDataPath = HierarchyUX.DataBasePath + "HierarchySeparatorData.asset";
        private static HierarchySeparatorData _data;

        public static bool IsSeparator(int instanceID)
        {
            HierarchySeparatorData data = GetData();
            return data != null && data.IsSeparator(instanceID);
        }

        private static HierarchySeparatorData GetData()
        {
            if (!_data)
            {
                _data = AssetDatabase.LoadAssetAtPath<HierarchySeparatorData>(SeparatorDataPath);

                if (!_data)
                {
                    _data = ScriptableObject.CreateInstance<HierarchySeparatorData>();

                    string directory = System.IO.Path.GetDirectoryName(SeparatorDataPath);

                    if (directory == null)
                    {
                        Debug.LogWarning("Can't find SeparatorData in " + SeparatorDataPath);
                        return null;
                    }

                    if (!AssetDatabase.IsValidFolder(directory))
                    {
                        System.IO.Directory.CreateDirectory(directory);
                        AssetDatabase.Refresh();
                    }

                    AssetDatabase.CreateAsset(_data, SeparatorDataPath);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    // Rebuild cache when data is loaded
                    _data.RebuildCache();
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

            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();
            HierarchySeparatorData data = GetData();

            data.SetSeparator(
                selected.GetInstanceID(),
                settings.DefaultBackgroundColor,
                settings.DefaultFontColor,
                settings.DefaultFontSize
            );

            selected.transform.hideFlags = HideFlags.HideInInspector;

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem("GameObject/Hierarchy UX/Create Separator", true)]
        private static bool CreateSeparatorValidate()
        {
            if (!Selection.activeGameObject)
                return false;

            int instanceID = Selection.activeGameObject.GetInstanceID();

            // Don't show if already a separator or if it's a folder
            if (GetData().IsSeparator(instanceID))
                return false;

            if (HierarchyFolder.IsFolder(instanceID))
                return false;

            return true;
        }

        [MenuItem("GameObject/Hierarchy UX/Edit Separator", false, 1)]
        private static void EditSeparator()
        {
            GameObject selected = Selection.activeGameObject;

            if (!selected)
                return;

            HierarchySeparatorData data = GetData();
            int id = selected.GetInstanceID();

            Color currentColor = data.GetColor(id);
            Color fontColor = data.GetFontColor(id);
            int fontSize = data.GetFontSize(id);

            HierarchySeparatorSettings.Show(selected, currentColor, fontColor, fontSize);
        }

        [MenuItem("GameObject/Hierarchy UX/Edit Separator", true)]
        private static bool EditSeparatorValidate()
        {
            if (!Selection.activeGameObject)
                return false;

            return GetData().IsSeparator(Selection.activeGameObject.GetInstanceID());
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

        public static void Draw(int instanceID, Rect selectionRect)
        {
            HierarchySeparatorData data = GetData();

            if (!data.IsSeparator(instanceID))
                return;

            GameObject obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;

            if (!obj)
                return;

            Color backgroundColor = data.GetColor(instanceID);
            Color fontColor = data.GetFontColor(instanceID);
            int fontSize = data.GetFontSize(instanceID);
            bool hasChildren = obj.transform.childCount > 0;

            // Draw full width background
            Rect fullWidthRect = new(32, selectionRect.y, selectionRect.xMax - 32 + 16, selectionRect.height);
            EditorGUI.DrawRect(fullWidthRect, backgroundColor);

            // Draw foldout arrow if has children
            if (hasChildren)
            {
                Rect foldoutRect = new(selectionRect.x - 14, selectionRect.y, 14, selectionRect.height);
                Color defaultColor = EditorGUIUtility.isProSkin
                    ? new Color(0.16f, 0.16f, 0.16f).gamma
                    : new Color(0.024f, 0.024f, 0.024f).gamma;

                GUIStyle arrowStyle = new(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = EditorStyles.foldout.fontSize,
                    normal = { textColor = defaultColor },
                    hover = { textColor = defaultColor }
                };

                bool expanded = IsExpanded(instanceID);
                string arrow = expanded ? "▼" : "▶";

                GUI.Label(foldoutRect, arrow, arrowStyle);
            }

            // Draw centered text
            GUIStyle style = new(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                normal = { textColor = fontColor.gamma }
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