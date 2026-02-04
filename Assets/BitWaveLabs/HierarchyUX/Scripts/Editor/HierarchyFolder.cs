using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public static class HierarchyFolder
    {
        public const string FolderDataPath = HierarchyUX.DataBasePath + "HierarchyFolderData.asset";
        private const string FolderIconPath = "Assets/BitWaveLabs/HierarchyUX/Resources/FolderIcon.png";
        
        private static HierarchyFolderData _data;
        private static Texture2D _folderIcon;
        private static Texture2D _customFolderIcon;

        public static bool IsFolder(int instanceID)
        {
            HierarchyFolderData data = GetData();
            return data != null && data.IsFolder(instanceID);
        }

        public static HierarchyFolderData GetData()
        {
            if (!_data)
            {
                _data = AssetDatabase.LoadAssetAtPath<HierarchyFolderData>(FolderDataPath);

                if (!_data)
                {
                    _data = ScriptableObject.CreateInstance<HierarchyFolderData>();

                    string directory = System.IO.Path.GetDirectoryName(FolderDataPath);

                    if (directory == null)
                    {
                        Debug.LogWarning("Can't find FolderData path: " + FolderDataPath);
                        return null;
                    }

                    if (!AssetDatabase.IsValidFolder(directory))
                    {
                        System.IO.Directory.CreateDirectory(directory);
                        AssetDatabase.Refresh();
                    }

                    AssetDatabase.CreateAsset(_data, FolderDataPath);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    _data.RebuildCache();
                }
            }

            return _data;
        }

        // Unity's built-in folder icon for hierarchy
        private static Texture2D GetFolderIcon()
        {
            if (!_folderIcon)
            {
                _folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
            }
            return _folderIcon;
        }

        // Custom white folder icon for tinting
        private static Texture2D GetCustomFolderIcon()
        {
            if (!_customFolderIcon)
            {
                _customFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(FolderIconPath);
            }
            return _customFolderIcon;
        }

        /// <summary>
        /// Creates a tinted copy of the custom folder icon.
        /// </summary>
        public static Texture2D CreateTintedFolderIcon(Color tint)
        {
            Texture2D sourceIcon = GetCustomFolderIcon();
            if (!sourceIcon)
                return null;

            // Create a new texture with the same dimensions
            Texture2D tintedIcon = new Texture2D(sourceIcon.width, sourceIcon.height, TextureFormat.ARGB32, false);
            
            // Get pixels and apply tint
            Color[] pixels = sourceIcon.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                // Multiply RGB by tint, preserve original alpha
                pixels[i] = new Color(
                    pixels[i].r * tint.r,
                    pixels[i].g * tint.g,
                    pixels[i].b * tint.b,
                    pixels[i].a
                );
            }
            
            tintedIcon.SetPixels(pixels);
            tintedIcon.Apply();

            return tintedIcon;
        }

        /// <summary>
        /// Updates the inspector icon with the specified tint color.
        /// </summary>
        public static void SetInspectorIcon(GameObject obj, Color color)
        {
            Texture2D tintedIcon = CreateTintedFolderIcon(color);
            if (tintedIcon)
            {
                EditorGUIUtility.SetIconForObject(obj, tintedIcon);
            }
        }

        [MenuItem("GameObject/Hierarchy UX/Create Folder", false, 2)]
        private static void CreateFolder()
        {
            GameObject selected = Selection.activeGameObject;

            if (!selected)
            {
                Debug.LogWarning("Please select a GameObject in the hierarchy first.");
                return;
            }

            HierarchyUXSettingsWindow.Settings settings = HierarchyUXSettingsWindow.GetSettings();
            
            HierarchyFolderData data = GetData();
            data.AddFolder(selected.GetInstanceID(), settings.DefaultFolderColor);

            // Set the folder icon in the inspector with the default color
            SetInspectorIcon(selected, settings.DefaultFolderColor);

            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(selected);
            AssetDatabase.SaveAssets();
            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem("GameObject/Hierarchy UX/Create Folder", true)]
        private static bool CreateFolderValidate()
        {
            if (!Selection.activeGameObject)
                return false;

            int instanceID = Selection.activeGameObject.GetInstanceID();

            if (GetData().IsFolder(instanceID))
                return false;

            if (HierarchySeparator.IsSeparator(instanceID))
                return false;

            return true;
        }

        [MenuItem("GameObject/Hierarchy UX/Edit Folder", false, 3)]
        private static void EditFolder()
        {
            GameObject selected = Selection.activeGameObject;

            if (!selected)
                return;

            HierarchyFolderData data = GetData();
            int id = selected.GetInstanceID();
            Color currentColor = data.GetColor(id);

            HierarchyFolderSettings.Show(selected, currentColor);
        }

        [MenuItem("GameObject/Hierarchy UX/Edit Folder", true)]
        private static bool EditFolderValidate()
        {
            if (!Selection.activeGameObject)
                return false;

            return GetData().IsFolder(Selection.activeGameObject.GetInstanceID());
        }

        [MenuItem("GameObject/Hierarchy UX/Remove Folder", false, 3)]
        private static void RemoveFolder()
        {
            GameObject selected = Selection.activeGameObject;

            if (!selected)
                return;

            HierarchyFolderData data = GetData();
            data.RemoveFolder(selected.GetInstanceID());

            // Remove the custom icon from the inspector
            EditorGUIUtility.SetIconForObject(selected, null);

            EditorUtility.SetDirty(data);
            EditorUtility.SetDirty(selected);
            AssetDatabase.SaveAssets();
            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem("GameObject/Hierarchy UX/Remove Folder", true)]
        private static bool RemoveFolderValidate()
        {
            if (!Selection.activeGameObject)
                return false;

            return GetData().IsFolder(Selection.activeGameObject.GetInstanceID());
        }

        public static void Draw(int instanceID, Rect selectionRect)
        {
            if (!IsFolder(instanceID))
                return;

            Texture2D icon = GetFolderIcon();
            if (!icon)
                return;

            // Get the folder color
            HierarchyFolderData data = GetData();
            Color folderColor = data.GetColor(instanceID);

            // Calculate icon rect (same position as HierarchyGameObjectIcons)
            Rect iconRect = new(selectionRect.x - 2, selectionRect.y, 16, 16);

            // Draw background to cover the default icon
            Color bgColor = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f)
                : new Color(0.76f, 0.76f, 0.76f);
            EditorGUI.DrawRect(iconRect, bgColor);

            // Draw the folder icon with tint color
            Color previousColor = GUI.color;
            GUI.color = folderColor;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }
    }
}