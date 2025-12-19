using UnityEngine;

namespace HowX.Data
{
    [System.Serializable]
    public class LocalizedText
    {
        [TextArea(2, 5)]
        public string en;     // English

        [TextArea(2, 5)]
        public string native; // Target Language (Polish/Turkish)

        // Helper to get the right string cleanly in code
        public string Get(bool isNative)
        {
            return isNative ? native : en;
        }
    }
}