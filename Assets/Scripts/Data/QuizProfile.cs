using UnityEngine;
using System.Collections.Generic;

namespace HowX.Data
{
    [CreateAssetMenu(fileName = "NewProfile", menuName = "HowX/Country Profile")]
    public class QuizProfile : ScriptableObject
    {
        [Tooltip("The folder code, e.g. 'PL'")]
        public string folderName;

        [Header("Visual Theme")]
        public Sprite backgroundImage;

        [Header("Import from CSV (Local Files)")]
        public TextAsset quizCsvFile;
        public TextAsset tierCsvFile;
        public TextAsset uiCsvFile;

        [Header("Import from Google Sheets (URLs)")]
        [Tooltip("Full URL to the Quiz tab, e.g. https://docs.google.com/spreadsheets/d/.../edit?gid=0")]
        public string quizSheetUrl;

        [Tooltip("Full URL to the Tier tab")]
        public string tierSheetUrl;

        [Tooltip("Full URL to the UI tab")]
        public string uiSheetUrl;

        public string imageLibraryPath => $"Assets/Data/{folderName}/Images/";

        [Header("Game Data")]
        public UIData uiData = new UIData();
        public List<QuestionData> questions = new List<QuestionData>();
        public List<TierData> tiers = new List<TierData>();
    }
}