using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public static class HierarchyButtons
    {
        private const float ButtonSize = 16f;
        private const float RightPadding = 4f;
        private const float ButtonSpacing = 2f;

        public static void Draw(int instanceID, Rect selectionRect)
        {
            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();

            if (!settings.ShowButtons)
                return;
            
            GameObject obj = EditorUtility.EntityIdToObject(instanceID) as GameObject;

            if (!obj)
                return;

            // Skip separators
            if (HierarchySeparator.IsSeparator(instanceID))
                return;

            // Calculate button positions (right to left)
            float visibilityX = selectionRect.xMax - ButtonSize - RightPadding;
            float lockX = visibilityX - ButtonSize - ButtonSpacing;

            Rect visibilityRect = new Rect(visibilityX, selectionRect.y, ButtonSize, ButtonSize);
            Rect lockRect = new Rect(lockX, selectionRect.y, ButtonSize, ButtonSize);

            // Draw visibility button
            DrawVisibilityButton(obj, visibilityRect);
            
            // Draw lock button
            DrawLockButton(obj, lockRect);
        }

        private static void DrawVisibilityButton(GameObject obj, Rect buttonRect)
        {
            Texture icon = obj.activeSelf
                ? EditorGUIUtility.IconContent("d_scenevis_visible_hover").image
                : EditorGUIUtility.IconContent("d_scenevis_hidden_hover").image;

            DrawButtonBackground(buttonRect);

            if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
            {
                Undo.RecordObject(obj, obj.activeSelf ? "Disable GameObject" : "Enable GameObject");
                obj.SetActive(!obj.activeSelf);
                EditorUtility.SetDirty(obj);
            }

            if (icon)
            {
                Color previousColor = GUI.color;
                if (!obj.activeSelf)
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);

                GUI.DrawTexture(buttonRect, icon, ScaleMode.ScaleToFit);
                GUI.color = previousColor;
            }
        }

        private static void DrawLockButton(GameObject obj, Rect buttonRect)
        {
            bool isLocked = (obj.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable;

            Texture icon = isLocked
                ? EditorGUIUtility.IconContent("IN LockButton on").image
                : EditorGUIUtility.IconContent("IN LockButton").image;

            DrawButtonBackground(buttonRect);

            if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
            {
                Undo.RecordObject(obj, isLocked ? "Unlock GameObject" : "Lock GameObject");
                
                if (isLocked)
                    obj.hideFlags &= ~HideFlags.NotEditable;
                else
                    obj.hideFlags |= HideFlags.NotEditable;
                
                EditorUtility.SetDirty(obj);
            }

            if (icon)
            {
                Color previousColor = GUI.color;
                if (isLocked)
                    GUI.color = new Color(1f, 0.6f, 0.6f, 1f); // Reddish tint when locked

                GUI.DrawTexture(buttonRect, icon, ScaleMode.ScaleToFit);
                GUI.color = previousColor;
            }
        }

        private static void DrawButtonBackground(Rect buttonRect)
        {
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                Color hoverColor = EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, 0.1f)
                    : new Color(0f, 0f, 0f, 0.1f);
                EditorGUI.DrawRect(buttonRect, hoverColor);
            }
        }

        /// <summary>
        /// Returns the total width occupied by all buttons.
        /// </summary>
        public static float GetTotalButtonsWidth()
        {
            return (ButtonSize * 2) + ButtonSpacing + RightPadding;
        }
        
        /// <summary>
        /// Check if a GameObject is currently locked.
        /// </summary>
        public static bool IsLocked(GameObject obj)
        {
            return (obj.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable;
        }
    }
}