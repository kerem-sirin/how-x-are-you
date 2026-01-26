using UnityEngine;

namespace HowX.Data
{
    [System.Serializable]
    public class UIData
    {
        // Title Template: "How (name) Are You?"
        public LocalizedText titleTemplate;

        // Button Texts
        public LocalizedText btnStart;
        public LocalizedText btnRestart;
        public LocalizedText btnLang;

        public LocalizedText confirmTitle;
        public LocalizedText btnYes;
        public LocalizedText btnNo;

        // Dynamic Flag
        public Texture2D flagIcon;
    }
}