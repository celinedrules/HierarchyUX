using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyComponentPopup : EditorWindow
    {
        private const float HeaderHeight = 18f;
        private const float MinWidth = 300f;
        private const float MinHeight = 600f;

        private static readonly Color BackgroundColor = new(0.2f, 0.2f, 0.2f);
        private static readonly Color HeaderColor = new(0.25f, 0.25f, 0.25f);
        private static readonly Color OutlineColor = new(0.1f, 0.1f, 0.1f);
        private static readonly Color SeparatorColor = new(0.2f, 0.2f, 0.2f);

        public static HierarchyComponentPopup FloatingInstance { get; private set; }

        private Component _component;
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPosition;

        // Dragging state
        private bool _isDragging;
        private Vector2 _dragStartMousePos;
        private Vector2 _dragStartWindowPos;

        public static void Show(Component component, Vector2 position)
        {
            if (!FloatingInstance)
            {
                FloatingInstance = CreateInstance<HierarchyComponentPopup>();
                FloatingInstance.position = new Rect(position.x, position.y, MinWidth, MinHeight);
                FloatingInstance.ShowPopup();
            }

            FloatingInstance.SetComponent(component);
            FloatingInstance.Focus();
        }

        public void SetComponent(Component component)
        {
            if (_editor)
                DestroyImmediate(_editor);

            _component = component;

            if (_component)
                _editor = UnityEditor.Editor.CreateEditor(_component);
        }

        private void OnGUI()
        {
            if (!_component)
            {
                Close();
                return;
            }

            DrawBackground();
            DrawHeader();
            DrawBody();
            DrawOutline();

            // Handle dragging
            HandleDragging();
        }

        private void DrawBackground()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), BackgroundColor);
        }

        private void DrawOutline()
        {
            Rect rect = new Rect(0, 0, position.width, position.height);

            // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), OutlineColor);
            // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), OutlineColor);
            // Left
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), OutlineColor);
            // Right
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), OutlineColor);
        }

        private void DrawHeader()
        {
            Rect headerRect = new Rect(1, 1, position.width - 2, HeaderHeight);

            // Background
            EditorGUI.DrawRect(headerRect, HeaderColor);

            // Bottom separator
            EditorGUI.DrawRect(new Rect(1, HeaderHeight, position.width - 2, 1), SeparatorColor);

            // Close button
            Rect closeButtonRect = new Rect(headerRect.xMax - 18, headerRect.y + 1, 16, 16);

            if (GUI.Button(closeButtonRect, GUIContent.none, GUIStyle.none))
                Close();

            Color closeIconColor = closeButtonRect.Contains(Event.current.mousePosition)
                ? new Color(0.9f, 0.9f, 0.9f)
                : new Color(0.65f, 0.65f, 0.65f);

            Color prevColor = GUI.color;
            GUI.color = closeIconColor;
            GUI.Label(closeButtonRect, EditorGUIUtility.IconContent("CrossIcon"));
            GUI.color = prevColor;
        }

        private void DrawBody()
        {
            if (!_editor)
                return;

            Rect bodyRect = new Rect(1, HeaderHeight + 1, position.width - 2, position.height - HeaderHeight - 2);

            GUILayout.BeginArea(bodyRect);

            EditorGUIUtility.labelWidth = Mathf.Max(position.width * 0.4f, 120f);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            _editor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();

            EditorGUIUtility.labelWidth = 0;

            GUILayout.EndArea();
        }

        private void HandleDragging()
        {
            Rect headerRect = new Rect(0, 0, position.width - 20, HeaderHeight); // Exclude close button area
            Event e = Event.current;
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            switch (e.type)
            {
                case EventType.MouseDown when headerRect.Contains(e.mousePosition):
                    _isDragging = true;
                    _dragStartMousePos = GUIUtility.GUIToScreenPoint(e.mousePosition);
                    _dragStartWindowPos = position.position;
                    GUIUtility.hotControl = controlId;
                    e.Use();
                    break;

                case EventType.MouseDrag when _isDragging:
                    Vector2 currentMousePos = GUIUtility.GUIToScreenPoint(e.mousePosition);
                    Vector2 delta = currentMousePos - _dragStartMousePos;
                    position = new Rect(_dragStartWindowPos + delta, position.size);
                    e.Use();
                    Repaint();
                    break;

                case EventType.MouseUp when _isDragging:
                    _isDragging = false;
                    GUIUtility.hotControl = 0;
                    e.Use();
                    break;
            }
        }

        private void OnDestroy()
        {
            if (_editor)
                DestroyImmediate(_editor);

            if (FloatingInstance == this)
                FloatingInstance = null;
        }

        private void OnLostFocus()
        {
            Close();
        }
    }
}