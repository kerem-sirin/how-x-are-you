using UnityEngine;

namespace HowX.Data
{
    [System.Serializable]
    public class LocalizedText
    {
        [TextArea(2, 5)]
        public string en;      // English

        [TextArea(2, 5)]
        public string native; // Target Language (Polish/Turkish)

        // Helper: Returns the correct string based on the boolean
        public string Get(bool isNative)
        {
            return isNative ? native : en;
        }
    }
}