using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public static class HierarchyComponentButtons
    {
        private const float IconSize = 16f;
        private const float IconSpacing = 2f;

        public static void Draw(int instanceID, Rect selectionRect)
        {
            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();

            if (!settings.ShowComponentButtons)
                return;

            GameObject obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;

            if (!obj)
                return;

            // Skip separators
            if (HierarchySeparator.IsSeparator(instanceID))
                return;

            Component[] components = obj.GetComponents<Component>();

            if (components.Length == 0)
                return;

            // First pass: count valid components to calculate total width
            List<Component> validComponents = new();
            foreach (Component comp in components)
            {
                if (!comp)
                    continue;

                if (IsInfrastructureComponent(comp))
                    continue;

                validComponents.Add(comp);
            }

            if (validComponents.Count == 0)
                return;

            // Calculate total width and starting position
            float totalWidth = (validComponents.Count * IconSize) + ((validComponents.Count - 1) * IconSpacing);
            float startX = selectionRect.xMax - HierarchyButtons.GetTotalButtonsWidth() - IconSpacing - totalWidth;

            // Draw icons left to right (in component order)
            float currentX = startX;

            foreach (Component comp in validComponents)
            {
                Texture icon = EditorGUIUtility.ObjectContent(comp, comp.GetType()).image;

                if (!icon)
                    continue;

                Rect iconRect = new Rect(currentX, selectionRect.y, IconSize, IconSize);
                bool isHovered = iconRect.Contains(Event.current.mousePosition);

                // Draw hover background
                if (isHovered)
                {
                    Color hoverColor = EditorGUIUtility.isProSkin
                        ? new Color(1f, 1f, 1f, 0.1f)
                        : new Color(0f, 0f, 0f, 0.1f);
                    EditorGUI.DrawRect(iconRect, hoverColor);
                }

                // Check if component is disabled (for Behaviours)
                Color previousColor = GUI.color;
                if (comp is Behaviour behaviour && !behaviour.enabled)
                    GUI.color = new Color(1f, 1f, 1f, 0.4f);

                // Draw button with tooltip and handle click
                string componentName = ObjectNames.NicifyVariableName(comp.GetType().Name);
                if (GUI.Button(iconRect, new GUIContent("", componentName), GUIStyle.none))
                {
                    HierarchyComponentPopup.Show(comp, iconRect);
                }

                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                GUI.color = previousColor;

                currentX += IconSize + IconSpacing;
            }
        }

        /// <summary>
        /// Returns the width occupied by component icons for a given GameObject.
        /// </summary>
        public static float GetComponentIconsWidth(GameObject obj)
        {
            if (!obj)
                return 0f;

            Component[] components = obj.GetComponents<Component>();
            int count = 0;

            foreach (Component comp in components)
            {
                if (!comp)
                    continue;

                if (IsInfrastructureComponent(comp))
                    continue;

                count++;
            }

            return count > 0 ? (count * IconSize) + ((count - 1) * IconSpacing) : 0f;
        }

        private static bool IsInfrastructureComponent(Component comp)
        {
            return comp is Transform
                or RectTransform
                or CanvasRenderer;
        }
    }
}