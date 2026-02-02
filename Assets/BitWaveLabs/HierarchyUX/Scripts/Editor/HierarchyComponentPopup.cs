using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyComponentPopup : EditorWindow
    {
        private Component _component;
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPosition;
        private bool _isPinned;

        public static void Show(Component component, Rect buttonRect)
        {
            // Convert button rect to screen coordinates
            Vector2 screenPos = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.yMax));

            HierarchyComponentPopup window = CreateInstance<HierarchyComponentPopup>();
            window._component = component;
            window.titleContent = new GUIContent(ObjectNames.NicifyVariableName(component.GetType().Name));

            // Position window below the button
            Rect windowRect = new Rect(screenPos.x, screenPos.y, 350, 400);
            window.position = windowRect;
            window.ShowUtility();
        }

        private void OnEnable()
        {
            if (_component)
                CreateEditor();
        }

        private void OnDisable()
        {
            DestroyEditor();
        }

        private void CreateEditor()
        {
            if (_editor)
                DestroyImmediate(_editor);

            if (_component)
                _editor = UnityEditor.Editor.CreateEditor(_component);
        }

        private void DestroyEditor()
        {
            if (_editor)
            {
                DestroyImmediate(_editor);
                _editor = null;
            }
        }

        private void OnGUI()
        {
            if (!_component)
            {
                EditorGUILayout.HelpBox("Component has been destroyed.", MessageType.Warning);
                return;
            }

            // Recreate editor if needed
            if (!_editor)
                CreateEditor();

            // Header with icon and controls
            DrawHeader();

            EditorGUILayout.Space(4);

            // Scrollable component inspector
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                if (_editor)
                    _editor.OnInspectorGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                // Component icon
                Texture icon = EditorGUIUtility.ObjectContent(_component, _component.GetType()).image;
                if (icon)
                    GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(18));

                // Component name
                GUILayout.Label(ObjectNames.NicifyVariableName(_component.GetType().Name), EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                // Enable toggle button (for Behaviours)
                if (_component is Behaviour behaviour)
                {
                    EditorGUI.BeginChangeCheck();
                    bool isEnabled = GUILayout.Toggle(behaviour.enabled,  behaviour.enabled ? "Enabled" : "Disabled", EditorStyles.toolbarButton,
                        GUILayout.Width(60));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(behaviour, isEnabled ? "Enable Component" : "Disable Component");
                        behaviour.enabled = isEnabled;
                        EditorUtility.SetDirty(behaviour);
                    }
                }

                // Pin toggle button
                _isPinned = GUILayout.Toggle(_isPinned, "Pin", EditorStyles.toolbarButton, GUILayout.Width(30));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnLostFocus()
        {
            // Close when losing focus, unless pinned
            if (!_isPinned)
                Close();
        }

        private void OnInspectorUpdate()
        {
            // Keep the inspector updated
            Repaint();
        }
    }
}