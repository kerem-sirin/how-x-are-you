using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HowX.Data
{
    [CreateAssetMenu(fileName = "NewProfile", menuName = "HowX/Country Profile")]
    public class QuizProfile : ScriptableObject
    {
        [Header("Identity")]
        public string countryName;        // "Poland"
        public string countryAdjective;   // "Polish"
        public string nativeLanguageName; // "Polski"

        [Header("Visual Theme")]
        public StyleSheet themeSheet;
        public Sprite backgroundImage;

        [Header("Data")]
        public List<QuestionData> questions;
        public List<TierData> tiers;
    }
}