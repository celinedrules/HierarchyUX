using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public static class HierarchyComponentIcons
    {
        public static void Draw(int instanceID, Rect selectionRect)
        {
            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();

            if (!settings.ShowComponentIcons)
                return;

            GameObject obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;

            if (!obj)
                return;

            // Skip separators - they have custom rendering
            if (HierarchySeparator.IsSeparator(instanceID))
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
            return comp is Transform
                or RectTransform
                or CanvasRenderer
                or CanvasScaler
                or GraphicRaycaster;
        }
    }
}