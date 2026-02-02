using UnityEngine;
using UnityEngine.Serialization;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyUXSettings : ScriptableObject
    {
        public bool showTreeLines = true;
        public bool showGameObjectIcons = true;
        public bool showButtons = true;
        public bool showComponentButtons = true;
        public bool showAlternatingRows = true;
        public int defaultFontSize = 12;
        public Color defaultFontColor = Color.white;
        public Color defaultBackgroundColor = Color.gray;
    }
}