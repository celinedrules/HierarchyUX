
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BitWaveLabs.HierarchyUX.Editor
{
    public class HierarchyFolderData : ScriptableObject
    {
        [System.Serializable]
        public class FolderEntry
        {
            public string globalObjectId;
            public Color color = Color.white;

            [System.NonSerialized] public int cachedInstanceID;
            [System.NonSerialized] public bool isCacheValid;
        }

        public List<FolderEntry> folders = new();

        private Dictionary<int, FolderEntry> _instanceIDCache = new();
        private bool _cacheInitialized;

        private void OnEnable()
        {
            RebuildCache();
        }

        public void RebuildCache()
        {
            _instanceIDCache.Clear();

            foreach (var entry in folders)
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

        public bool IsFolder(int instanceID)
        {
            EnsureCache();
            return _instanceIDCache.ContainsKey(instanceID);
        }

        public Color GetColor(int instanceID)
        {
            EnsureCache();
            return _instanceIDCache.TryGetValue(instanceID, out var entry) ? entry.color : Color.white;
        }

        public void AddFolder(int instanceID, Color color = default)
        {
            EnsureCache();

            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj == null) return;

            GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            string globalIdString = globalId.ToString();

            // Check if already exists
            if (folders.Exists(f => f.globalObjectId == globalIdString))
                return;

            var entry = new FolderEntry
            {
                globalObjectId = globalIdString,
                color = color == default ? Color.white : color,
                cachedInstanceID = instanceID,
                isCacheValid = true
            };

            folders.Add(entry);
            _instanceIDCache[instanceID] = entry;
        }

        public void SetFolderColor(int instanceID, Color color)
        {
            EnsureCache();

            if (_instanceIDCache.TryGetValue(instanceID, out var entry))
            {
                entry.color = color;
            }
        }

        public void RemoveFolder(int instanceID)
        {
            EnsureCache();

            if (_instanceIDCache.TryGetValue(instanceID, out var entry))
            {
                folders.Remove(entry);
                _instanceIDCache.Remove(instanceID);
            }
        }
    }
}