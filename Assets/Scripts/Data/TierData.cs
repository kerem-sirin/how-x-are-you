using UnityEngine;

namespace HowX.Data
{
    [System.Serializable]
    public class TierData
    {
        public string title;          // "Russian Spy"
        [Range(0, 100)]
        public float maxPercentage;   // 12
        public string subtitle;       // "O kurwa..."

        [TextArea(3, 10)]
        public string description;    // "You put ketchup on pierogi..."
    }
}