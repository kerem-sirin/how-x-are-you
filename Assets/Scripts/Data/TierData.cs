using UnityEngine;

namespace HowX.Data
{
    [System.Serializable]
    public class TierData
    {
        public LocalizedText title;
        [Range(0, 100)]
        public float maxPercentage;

        public LocalizedText subtitle;
        public LocalizedText description;
    }
}