using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public static class HierarchySeparator
{
    private const string SeparatorDataPath = "Assets/Editor/HierarchySeparatorData.asset";
    private static HierarchySeparatorData _data;
    
    // Track visible items - use previous frame's data for checking
    private static HashSet<int> _visibleItemsCurrent = new HashSet<int>();
    private static HashSet<int> _visibleItemsPrevious = new HashSet<int>();
    private static int _lastFrameCount = -1;

    static HierarchySeparator()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static HierarchySeparatorData GetData()
    {
        if (_data == null)
        {
            _data = AssetDatabase.LoadAssetAtPath<HierarchySeparatorData>(SeparatorDataPath);
            if (_data == null)
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
        if (selected == null)
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
        if (selected == null) return;

        selected.transform.hideFlags = HideFlags.None;

        var data = GetData();
        data.RemoveSeparator(selected.GetInstanceID());
        EditorUtility.SetDirty(data);
        EditorUtility.SetDirty(selected);
        AssetDatabase.SaveAssets();
        EditorApplication.RepaintHierarchyWindow();
    }

    [MenuItem("GameObject/Hierarchy UX/Remove Separator", true)]
    private static bool RemoveSeparatorValidate()
    {
        if (Selection.activeGameObject == null) return false;
        return GetData().IsSeparator(Selection.activeGameObject.GetInstanceID());
    }

     private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        // Swap buffers at start of new frame
        if (Time.frameCount != _lastFrameCount)
        {
            _visibleItemsPrevious = _visibleItemsCurrent;
            _visibleItemsCurrent = new HashSet<int>();
            _lastFrameCount = Time.frameCount;
        }
        
        // Track this item as visible for next frame
        _visibleItemsCurrent.Add(instanceID);
        
        var data = GetData();
        if (!data.IsSeparator(instanceID)) return;

        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        Color backgroundColor = data.GetColor(instanceID);
        bool hasChildren = obj.transform.childCount > 0;

        // Check expanded state using previous frame's data
        bool isExpanded = hasChildren && IsExpanded(instanceID);

        // Draw full width background
        Rect fullWidthRect = new Rect(
            32,
            selectionRect.y,
            selectionRect.xMax - 32 + 16,
            selectionRect.height
        );

        EditorGUI.DrawRect(fullWidthRect, backgroundColor);

        // Calculate text color
        float brightness = backgroundColor.r * 0.299f + backgroundColor.g * 0.587f + backgroundColor.b * 0.114f;
        Color textColor = brightness > 0.5f ? Color.black : Color.white;

        // Draw foldout arrow if has children
        if (hasChildren)
        {
            Rect foldoutRect = new Rect(
                selectionRect.x - 14,
                selectionRect.y,
                14,
                selectionRect.height
            );

            GUIStyle arrowStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = textColor }
            };

            bool expanded = IsExpanded(instanceID);
            Debug.Log(expanded);
            string arrow = expanded ? "▼" : "▶";

            GUI.Label(foldoutRect, arrow, arrowStyle);
        }

        // Draw centered text
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = textColor }
        };

        EditorGUI.LabelField(fullWidthRect, obj.name, style);
    }
    
     
     private static void SetExpanded(int instanceID, bool expanded)
    {
        var hierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var window = EditorWindow.GetWindow(hierarchyWindowType, false, null, false);
    
        if (window == null) return;

        var sceneHierarchyProperty = hierarchyWindowType.GetProperty("sceneHierarchy", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var sceneHierarchy = sceneHierarchyProperty?.GetValue(window);
    
        if (sceneHierarchy == null) return;

        var setExpandedMethod = sceneHierarchy.GetType().GetMethod("ExpandTreeViewItem", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    
        setExpandedMethod?.Invoke(sceneHierarchy, new object[] { instanceID, expanded });
    }

    private static bool IsExpanded(int instanceID)
    {
        var hierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var window = EditorWindow.GetWindow(hierarchyWindowType, false, null, false);

        if (window == null) return false;

        var sceneHierarchyField = hierarchyWindowType.GetField("m_SceneHierarchy",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var sceneHierarchy = sceneHierarchyField?.GetValue(window);

        if (sceneHierarchy == null) return false;

        var treeViewStateField = sceneHierarchy.GetType().GetField("m_TreeViewState",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var treeViewState = treeViewStateField?.GetValue(sceneHierarchy);

        if (treeViewState == null) return false;

        var expandedIDsProperty = treeViewState.GetType().GetProperty("expandedIDs",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        var expandedIDs = expandedIDsProperty?.GetValue(treeViewState) as System.Collections.IList;

        if (expandedIDs == null) return false;

        foreach (var entityId in expandedIDs)
        {
            var dataField = entityId.GetType().GetField("m_Data",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (dataField != null)
            {
                var id = (int)dataField.GetValue(entityId);
                if (id == instanceID) return true;
            }
        }

        return false;
    }
}

public class HierarchySeparatorData : ScriptableObject
{
    [System.Serializable]
    public class SeparatorEntry
    {
        public int instanceID;
        public string guid; // For persistence across sessions
        public Color color = Color.gray;
    }

    public List<SeparatorEntry> separators = new List<SeparatorEntry>();

    public bool IsSeparator(int instanceID)
    {
        return separators.Exists(s => s.instanceID == instanceID);
    }

    public Color GetColor(int instanceID)
    {
        var entry = separators.Find(s => s.instanceID == instanceID);
        return entry?.color ?? Color.gray;
    }

    public void SetSeparator(int instanceID, Color color)
    {
        var entry = separators.Find(s => s.instanceID == instanceID);
        if (entry != null)
        {
            entry.color = color;
        }
        else
        {
            separators.Add(new SeparatorEntry { instanceID = instanceID, color = color });
        }
    }

    public void RemoveSeparator(int instanceID)
    {
        separators.RemoveAll(s => s.instanceID == instanceID);
    }
}

public class ColorPickerWindow : EditorWindow
{
    private GameObject _targetObject;
    private Color _selectedColor = Color.cyan;

    public static void Show(GameObject target, Color initialColor)
    {
        var window = GetWindow<ColorPickerWindow>("Pick Separator Color");
        window._targetObject = target;
        window._selectedColor = initialColor;
        window.minSize = new Vector2(250, 100);
        window.maxSize = new Vector2(250, 100);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        if (_targetObject == null)
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
        if (GUILayout.Button("Apply", GUILayout.Height(30)))
        {
            ApplyColor();
            Close();
        }
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ApplyColor()
    {
        var data = AssetDatabase.LoadAssetAtPath<HierarchySeparatorData>("Assets/Editor/HierarchySeparatorData.asset");
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<HierarchySeparatorData>();
            
            if (!AssetDatabase.IsValidFolder("Assets/Editor"))
            {
                AssetDatabase.CreateFolder("Assets", "Editor");
            }
            
            AssetDatabase.CreateAsset(data, "Assets/Editor/HierarchySeparatorData.asset");
        }

        data.SetSeparator(_targetObject.GetInstanceID(), _selectedColor);
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        EditorApplication.RepaintHierarchyWindow();
    }
}