#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EpochBreaker.Editor.QA
{
    [Serializable]
    public class QAItem
    {
        public string Section;
        public string Title;
        [TextArea(2, 4)]
        public string Description;
        public bool Required = true;
        public bool Passed;
        [TextArea(2, 4)]
        public string Notes;
    }

    public class EditorQASettings : ScriptableObject
    {
        public string TesterName;
        public string BuildOrScene;
        public string DeviceOrResolution;
        public string Notes;
        public string LastUpdatedUtc;
        public List<QAItem> Items = new List<QAItem>();

        public void TouchTimestamp()
        {
            LastUpdatedUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        }

        public void ResetAll()
        {
            foreach (var item in Items)
            {
                item.Passed = false;
                item.Notes = string.Empty;
            }
            Notes = string.Empty;
            TouchTimestamp();
        }

        public void MarkAllPassed()
        {
            foreach (var item in Items)
            {
                item.Passed = true;
            }
            TouchTimestamp();
        }
    }
}
#endif
