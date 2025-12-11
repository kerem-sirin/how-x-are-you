using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements; // For StyleSheet

namespace HowX.Data
{
    [CreateAssetMenu(fileName = "NewProfile", menuName = "HowX/Country Profile")]
    public class QuizProfile : ScriptableObject
    {
        [Header("Identity")]
        public string countryName;        // "Poland"
        public string nativeLanguageName; // "Polski"

        [Header("Visual Theme")]
        public StyleSheet themeSheet;     // The .uss file
        public Sprite backgroundImage;

        [Header("Data")]
        public List<QuestionData> questions;
        public List<TierData> tiers;
    }
}