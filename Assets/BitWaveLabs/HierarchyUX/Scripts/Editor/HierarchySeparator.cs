using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BitWaveLabs.HierarchyUX.Editor
{
    [InitializeOnLoad]
    public static class HierarchySeparator
    {
        public const string SeparatorDataBasePath = "Assets/BitWaveLabs/HierarchyUX/Data/";
        public const string SeparatorDataPath = SeparatorDataBasePath + "HierarchySeparatorData.asset";
        private static HierarchySeparatorData _data;

        static HierarchySeparator()
        {
            EditorApplication.hierarchyWindowItemOnGUI += DrawAlternatingBackground;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += DrawTreeLines;
            EditorApplication.hierarchyWindowItemOnGUI += DrawComponentIcons;
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

            return !GetData().IsSeparator(Selection.activeGameObject.GetInstanceID());
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

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
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

        private static void DrawTreeLines(int instanceID, Rect selectionRect)
        {
            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();

            if (!settings.ShowTreeLines)
                return;

            GameObject obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;

            if (!obj)
                return;

            Transform transform = obj.transform;

            // Don't draw for root objects
            if (!transform.parent)
                return;

            Color lineColor = new(0.5f, 0.5f, 0.5f, 0.5f);
            const float lineThickness = 1f;
            const float indent = 14f; // Unity's indent per level

            // Calculate depth
            int depth = 0;
            Transform current = transform.parent;
            while (current != null)
            {
                depth++;
                current = current.parent;
            }

            // Horizontal line to the item
            float xPos = selectionRect.x - 22f;
            float yCenter = selectionRect.y + selectionRect.height / 2f;

            Rect horizontalLine = new(xPos, yCenter, 8f, lineThickness);
            EditorGUI.DrawRect(horizontalLine, lineColor);

            // Vertical line from parent
            bool isLastChild = transform.GetSiblingIndex() == transform.parent.childCount - 1;
            float verticalHeight = isLastChild ? selectionRect.height / 2f + 1f : selectionRect.height;
            float verticalY = selectionRect.y;

            Rect verticalLine = new(xPos, verticalY, lineThickness, verticalHeight);
            EditorGUI.DrawRect(verticalLine, lineColor);

            // Draw continuation lines for ancestors
            current = transform.parent;
            int level = depth - 1;

            while (current && current.parent)
            {
                bool ancestorIsLastChild = current.GetSiblingIndex() == current.parent.childCount - 1;

                if (!ancestorIsLastChild)
                {
                    float ancestorX = selectionRect.x - 22f - (indent * (depth - level));
                    Rect continuationLine = new Rect(ancestorX, selectionRect.y, lineThickness, selectionRect.height);
                    EditorGUI.DrawRect(continuationLine, lineColor);
                }

                current = current.parent;
                level--;
            }
        }

        private static void DrawComponentIcons(int instanceID, Rect selectionRect)
        {
            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();

            if (!settings.ShowComponentIcons)
                return;

            GameObject obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;

            if (!obj)
                return;

            // Skip separators - they have custom rendering
            HierarchySeparatorData data = GetData();

            if (data && data.IsSeparator(instanceID))
                return;

            Component[] components = obj.GetComponents<Component>();

            if (components.Length == 0)
                return;

            // Find the primary component to display
            Component primaryComponent = null;

            // First, try to find an "interesting" component (skip infrastructure)
            foreach (Component comp in components)
            {
                if (!comp)
                    continue;

                if (IsInfrastructureComponent(comp))
                    continue;

                primaryComponent = comp;
                break;
            }

            Texture icon;

            // If no interesting component found, use Transform/RectTransform icon by type
            if (!primaryComponent)
            {
                RectTransform rectTransform = obj.GetComponent<RectTransform>();

                icon = rectTransform
                    ? EditorGUIUtility.ObjectContent(null, typeof(RectTransform)).image
                    : EditorGUIUtility.ObjectContent(null, typeof(Transform)).image;
            }
            else
            {
                icon = EditorGUIUtility.ObjectContent(primaryComponent, primaryComponent.GetType()).image;
            }

            if (!icon)
                return;

            // Calculate icon rect (Unity's default icon position)
            Rect iconRect = new(selectionRect.x - 2, selectionRect.y, 16, 16);

            // Draw background to cover the default icon
            Color bgColor = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f)
                : new Color(0.76f, 0.76f, 0.76f);
            EditorGUI.DrawRect(iconRect, bgColor);

            // Draw the component icon
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }

        private static bool IsInfrastructureComponent(Component comp)
        {
            // Components that are "infrastructure" - they support other components
            // but don't define the GameObject's primary purpose
            return comp is Transform
                or RectTransform
                or CanvasRenderer
                or CanvasScaler
                or GraphicRaycaster;
        }

        private static void DrawAlternatingBackground(int instanceID, Rect selectionRect)
        {
            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();

            if (!settings.ShowAlternatingRows)
                return;

            // Skip separators - they have custom backgrounds
            HierarchySeparatorData data = GetData();
            
            if (data && data.IsSeparator(instanceID))
                return;

            // Calculate row index based on Y position
            int rowIndex = Mathf.FloorToInt(selectionRect.y / selectionRect.height);

            // Only draw on odd rows
            if (rowIndex % 2 != 1)
                return;
            
            GameObject obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;
            
            if (!obj)
                return;

            // Skip if this object is selected
            if (Selection.activeGameObject == obj || Array.IndexOf(Selection.gameObjects, obj) >= 0)
                return;

            if (selectionRect.Contains(Event.current.mousePosition))
                return;

            // Subtle semi-transparent tint
            Color altColor = EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.04f)
                : new Color(0f, 0f, 0f, 0.04f);

            Rect fullWidthRect = new(0, selectionRect.y, selectionRect.xMax + 16, selectionRect.height);
            EditorGUI.DrawRect(fullWidthRect, altColor);
        }
    }
}