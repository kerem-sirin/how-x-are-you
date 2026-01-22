using UnityEngine;

namespace HowX.Data
{
    [System.Serializable]
    public class UIData
    {
        // Button Texts
        public LocalizedText btnStart;
        public LocalizedText btnRestart;
        public LocalizedText btnLang;

        // Title Template: "How (name) Are You?"
        public LocalizedText titleTemplate;

        // Dynamic Flag
        public Texture2D flagIcon;
    }
}