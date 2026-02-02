using UnityEditor;

namespace BitWaveLabs.HierarchyUX.Editor
{
    [InitializeOnLoad]
    public static class HierarchyUX
    {
        public const string DataBasePath = "Assets/BitWaveLabs/HierarchyUX/Data/";

        static HierarchyUX()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyAlternatingRows.Draw;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchySeparator.Draw;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyTreeLines.Draw;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyComponentIcons.Draw;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyButtons.Draw;
        }
    }
}