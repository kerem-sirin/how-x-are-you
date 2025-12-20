using UnityEngine;
using System.Collections.Generic;

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
        public Sprite backgroundImage;

        [Header("Data")]
        public List<QuestionData> questions;
        public List<TierData> tiers;
    }
}