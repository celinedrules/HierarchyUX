using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyUXSettings : ScriptableObject
    {
        public bool showTreeLines = true;
        public bool showGameObjectIcons = true;
        public bool showButtons = true;
        public bool showComponentButtons = true;
        public bool showAlternatingRows = true;
        
        // Separator defaults
        public int defaultFontSize = 12;
        public Color defaultFontColor = Color.white;
        public Color defaultBackgroundColor = Color.gray;
        
        // Folder defaults (Windows folder yellow: #FFD54F)
        public Color defaultFolderColor = new(1f, 0.835f, 0.31f, 1f);
    }
}