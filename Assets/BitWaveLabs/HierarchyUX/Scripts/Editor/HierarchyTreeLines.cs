using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public static class HierarchyTreeLines
    {
        public static void Draw(int instanceID, Rect selectionRect)
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
    }
}