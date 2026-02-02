using System;
using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public static class HierarchyAlternatingRows
    {
        public static void Draw(int instanceID, Rect selectionRect)
        {
            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();

            if (!settings.ShowAlternatingRows)
                return;

            // Skip separators - they have custom backgrounds
            if (HierarchySeparator.IsSeparator(instanceID))
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

            // Skip if hovering
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