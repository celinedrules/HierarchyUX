using System.Collections.Generic;
using UnityEngine;

namespace Editor
{
    public class HierarchySeparatorData : ScriptableObject
    {
        [System.Serializable]
        public class SeparatorEntry
        {
            public int instanceID;
            public Color color = Color.gray;
            public Color fontColor = Color.white;
            public int fontSize = 12;
        }

        public List<SeparatorEntry> separators = new();

        public bool IsSeparator(int instanceID) => separators.Exists(s => s.instanceID == instanceID);

        public Color GetColor(int instanceID)
        {
            SeparatorEntry entry = separators.Find(s => s.instanceID == instanceID);
            return entry?.color ?? Color.gray;
        }

        public Color GetFontColor(int instanceID)
        {
            SeparatorEntry entry = separators.Find(s => s.instanceID == instanceID);
            return entry?.fontColor ?? Color.white;
        }

        public int GetFontSize(int instanceID)
        {
            SeparatorEntry entry = separators.Find(s => s.instanceID == instanceID);
            return entry?.fontSize ?? 12;
        }

        public void SetSeparator(int instanceID, Color color, Color fontColor, int fontSize)
        {
            SeparatorEntry entry = separators.Find(s => s.instanceID == instanceID);
        
            if (entry != null)
            {
                entry.color = color;
                entry.fontColor = fontColor;
                entry.fontSize = fontSize;
            }
            else
            {
                separators.Add(new SeparatorEntry
                {
                    instanceID = instanceID,
                    color = color,
                    fontColor = fontColor,
                    fontSize = fontSize
                });
            }
        }

        public void RemoveSeparator(int instanceID)
        {
            separators.RemoveAll(s => s.instanceID == instanceID);
        }
    }
}