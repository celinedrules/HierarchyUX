using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyUXSettings : ScriptableObject
    {
        public bool showTreeLines = true;
        public bool showComponentIcons = true;
        public bool showAlternatingRows = true;
        public int defaultFontSize = 12;
        public Color defaultFontColor = Color.white;
        public Color defaultBackgroundColor = Color.gray;
    }
}