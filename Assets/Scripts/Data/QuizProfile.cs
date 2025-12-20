using UnityEngine;
using System.Collections.Generic;

namespace HowX.Data
{
    [CreateAssetMenu(fileName = "NewProfile", menuName = "HowX/Country Profile")]
    public class QuizProfile : ScriptableObject
    {
        [Header("Identity")]
        public string countryName;
        public string countryAdjective;
        public string nativeLanguageName;

        [Tooltip("The folder code, e.g. 'PL' or 'TR'")]
        public string folderName;

        [Header("Visual Theme")]
        public Sprite backgroundImage;

        [Header("Import Settings")]
        public TextAsset quizCsvFile;
        public TextAsset tierCsvFile;

        public string imageLibraryPath => $"Assets/Data/{folderName}/Images/";

        [Header("Game Data")]
        public List<QuestionData> questions = new List<QuestionData>();
        public List<TierData> tiers = new List<TierData>();
    }
}