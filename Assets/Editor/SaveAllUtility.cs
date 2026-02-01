using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class SaveAllUtility
    {
        [MenuItem("File/Save All #S", priority = 170)] // Ctrl+Shift+S (Cmd+Shift+S on Mac)
        public static void SaveAll()
        {
            // Save the active scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            // Save project assets (like saving the .meta and project settings)
            AssetDatabase.SaveAssets();

            // Optional feedback
            Debug.Log("All open scenes and assets saved!");
        }
    }
}