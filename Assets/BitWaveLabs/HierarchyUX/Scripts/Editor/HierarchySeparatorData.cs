using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchySeparatorData : ScriptableObject
    {
        [System.Serializable]
        public class SeparatorEntry
        {
            public string globalObjectId; // Persistent identifier
            public Color color = Color.gray;
            public Color fontColor = Color.white;
            public int fontSize = 12;
            
            // Runtime cache - not serialized
            [System.NonSerialized] public int cachedInstanceID;
            [System.NonSerialized] public bool isCacheValid;
        }

        public List<SeparatorEntry> separators = new();
        
        // Runtime lookup cache
        private Dictionary<int, SeparatorEntry> _instanceIDCache = new();
        private bool _cacheInitialized;

        private void OnEnable()
        {
            RebuildCache();
        }

        public void RebuildCache()
        {
            _instanceIDCache.Clear();
            
            foreach (var entry in separators)
            {
                if (string.IsNullOrEmpty(entry.globalObjectId)) continue;
                
                if (GlobalObjectId.TryParse(entry.globalObjectId, out GlobalObjectId id))
                {
                    Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                    if (obj is GameObject go)
                    {
                        entry.cachedInstanceID = go.GetInstanceID();
                        entry.isCacheValid = true;
                        _instanceIDCache[entry.cachedInstanceID] = entry;
                    }
                }
            }
            
            _cacheInitialized = true;
        }

        private void EnsureCache()
        {
            if (!_cacheInitialized)
                RebuildCache();
        }

        public bool IsSeparator(int instanceID)
        {
            EnsureCache();
            return _instanceIDCache.ContainsKey(instanceID);
        }

        public Color GetColor(int instanceID)
        {
            EnsureCache();
            return _instanceIDCache.TryGetValue(instanceID, out var entry) ? entry.color : Color.gray;
        }

        public Color GetFontColor(int instanceID)
        {
            EnsureCache();
            return _instanceIDCache.TryGetValue(instanceID, out var entry) ? entry.fontColor : Color.white;
        }

        public int GetFontSize(int instanceID)
        {
            EnsureCache();
            return _instanceIDCache.TryGetValue(instanceID, out var entry) ? entry.fontSize : 12;
        }

        public void SetSeparator(int instanceID, Color color, Color fontColor, int fontSize)
        {
            EnsureCache();
            
            // Get the GlobalObjectId for this instance
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj == null) return;
            
            GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            string globalIdString = globalId.ToString();
            
            // Find existing entry by globalObjectId
            SeparatorEntry entry = separators.Find(s => s.globalObjectId == globalIdString);
            
            if (entry != null)
            {
                entry.color = color;
                entry.fontColor = fontColor;
                entry.fontSize = fontSize;
            }
            else
            {
                entry = new SeparatorEntry
                {
                    globalObjectId = globalIdString,
                    color = color,
                    fontColor = fontColor,
                    fontSize = fontSize,
                    cachedInstanceID = instanceID,
                    isCacheValid = true
                };
                separators.Add(entry);
            }
            
            _instanceIDCache[instanceID] = entry;
        }

        public void RemoveSeparator(int instanceID)
        {
            EnsureCache();
            
            if (_instanceIDCache.TryGetValue(instanceID, out var entry))
            {
                separators.Remove(entry);
                _instanceIDCache.Remove(instanceID);
            }
        }
        
        // Clean up entries for deleted GameObjects
        public void CleanupInvalidEntries()
        {
            separators.RemoveAll(entry =>
            {
                if (string.IsNullOrEmpty(entry.globalObjectId)) return true;
                
                if (GlobalObjectId.TryParse(entry.globalObjectId, out GlobalObjectId id))
                {
                    Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                    return obj == null;
                }
                
                return true;
            });
            
            RebuildCache();
        }
    }
}