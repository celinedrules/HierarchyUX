using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyComponentPopup : EditorWindow
    {
        private const float HeaderHeight = 24f;
        private const float ButtonSize = 20f;
        private const float MinWidth = 300f;
        private const float MinHeight = 600f;

        private static readonly Color BackgroundColor = new(0.2f, 0.2f, 0.2f);
        private static readonly Color HeaderColor = new(0.25f, 0.25f, 0.25f);
        private static readonly Color OutlineColor = new(0.1f, 0.1f, 0.1f);
        private static readonly Color SeparatorColor = new(0.2f, 0.2f, 0.2f);

        public static HierarchyComponentPopup Instance { get; private set; }

        private Component _component;
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPosition;
        private bool _isPinned;

        // Dragging state
        private bool _isDragging;
        private Vector2 _dragStartMousePos;
        private Vector2 _dragStartWindowPos;

        public static void Show(Component component, Vector2 position)
        {
            if (!Instance)
            {
                Instance = CreateInstance<HierarchyComponentPopup>();
                Instance.position = new Rect(position.x, position.y, MinWidth, MinHeight);
                Instance.ShowPopup();
            }

            Instance.SetComponent(component);
            Instance.Focus();
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
            EditorGUI.DrawRect(new Rect(1, HeaderHeight + 1, position.width - 2, 1), SeparatorColor);

            // Center buttons vertically in header
            float buttonY = headerRect.y + (HeaderHeight - ButtonSize) / 2f;

            // Close button (rightmost)
            Rect closeButtonRect = new Rect(headerRect.xMax - ButtonSize - 2, buttonY, ButtonSize, ButtonSize);

            // Pin button (left of close button)
            Rect pinButtonRect = new Rect(closeButtonRect.x - ButtonSize - 2, buttonY, ButtonSize, ButtonSize);

            // Component icon (top left)
            Rect iconRect = new Rect(4, (HeaderHeight - 16) / 2f + 1, 16, 16);
            Texture icon = EditorGUIUtility.ObjectContent(_component, _component.GetType()).image;
            if (icon)
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

            // Track where the name label should start
            float nameLabelX = iconRect.xMax + 4;

            // Enable checkbox (only for Behaviours)
            if (_component is Behaviour behaviour)
            {
                Rect toggleRect = new Rect(iconRect.xMax + 4, (HeaderHeight - 14) / 2f + 1, 14, 14);

                EditorGUI.BeginChangeCheck();
                bool isEnabled = GUI.Toggle(toggleRect, behaviour.enabled, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(behaviour, isEnabled ? "Enable Component" : "Disable Component");
                    behaviour.enabled = isEnabled;
                    EditorUtility.SetDirty(behaviour);
                }

                nameLabelX = toggleRect.xMax + 4;
            }

            // Component name (bold, white)
            string componentName = ObjectNames.NicifyVariableName(_component.GetType().Name);
            Rect nameRect = new Rect(nameLabelX, 1, pinButtonRect.x - nameLabelX - 4, HeaderHeight);
            
            GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                active = { textColor = Color.white },
                focused = { textColor = Color.white }
            };
            GUI.Label(nameRect, componentName, nameStyle);

            // Draw buttons
            DrawCloseButton(closeButtonRect);
            DrawPinButton(pinButtonRect);
        }

        private void DrawCloseButton(Rect buttonRect)
        {
            if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                Close();

            Color iconColor = buttonRect.Contains(Event.current.mousePosition)
                ? new Color(1f, 0.0f, 0.0f)
                : new Color(0.65f, 0.65f, 0.65f);

            Color prevColor = GUI.color;
            GUI.color = iconColor;
            GUI.Label(buttonRect, EditorGUIUtility.IconContent("CrossIcon"));
            GUI.color = prevColor;
        }

        private void DrawPinButton(Rect buttonRect)
        {
            if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                _isPinned = !_isPinned;

            bool isHovered = buttonRect.Contains(Event.current.mousePosition);

            Color iconColor;
            if (_isPinned)
                iconColor = new Color(0.5f, 0.8f, 1f); // Blue tint when pinned
            else if (isHovered)
                iconColor = new Color(0.9f, 0.9f, 0.9f);
            else
                iconColor = new Color(0.65f, 0.65f, 0.65f);

            Color prevColor = GUI.color;
            GUI.color = iconColor;

            // Use different icon based on pinned state
            string iconName = _isPinned ? "pinned" : "pin";
            GUI.Label(buttonRect, EditorGUIUtility.IconContent(iconName));

            GUI.color = prevColor;
        }

        private void DrawBody()
        {
            if (!_editor)
                return;

            Rect bodyRect = new Rect(7, HeaderHeight + 7, position.width - 14, position.height - HeaderHeight - 3);

            GUILayout.BeginArea(bodyRect);

            // Ensure we're using inspector mode (not hierarchy mode) for proper font sizes
            EditorGUIUtility.hierarchyMode = false;
            EditorGUIUtility.wideMode = position.width > 330f;
            EditorGUIUtility.labelWidth = Mathf.Max(position.width * 0.45f, 120f);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Draw the inspector with default styles
            _editor.OnInspectorGUI();

            EditorGUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        private void HandleDragging()
        {
            Rect headerRect = new Rect(0, 0, position.width - 50, HeaderHeight); // Exclude button area
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

            if (Instance == this)
                Instance = null;
        }

        private void OnLostFocus()
        {
            if (!_isPinned)
                Close();
        }
    }
}