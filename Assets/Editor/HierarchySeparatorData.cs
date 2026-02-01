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
        }

        public List<SeparatorEntry> separators = new();

        public bool IsSeparator(int instanceID) => separators.Exists(s => s.instanceID == instanceID);

        public Color GetColor(int instanceID)
        {
            SeparatorEntry entry = separators.Find(s => s.instanceID == instanceID);
            return entry?.color ?? Color.gray;
        }

        public void SetSeparator(int instanceID, Color color)
        {
            SeparatorEntry entry = separators.Find(s => s.instanceID == instanceID);
            
            if (entry != null)
                entry.color = color;
            else
                separators.Add(new SeparatorEntry { instanceID = instanceID, color = color });
        }

        public void RemoveSeparator(int instanceID)
        {
            separators.RemoveAll(s => s.instanceID == instanceID);
        }
    }
}