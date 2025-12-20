using System.Collections.Generic;
using UnityEngine;

namespace HowX.Data
{
    [System.Serializable]
    public class QuestionData
    {
        [Header("Organization")]
        public string id;

        [Header("Content")]
        public LocalizedText questionText;
        public Texture2D questionVisual;

        [Space(10)]
        public LocalizedText answer0; // Correct Answer
        public Texture2D answer0_Image;

        [Space(5)]
        public LocalizedText answer1;
        public Texture2D answer1_Image;

        [Space(5)]
        public LocalizedText answer2;
        public Texture2D answer2_Image;

        [Space(5)]
        public LocalizedText answer3;
        public Texture2D answer3_Image;

        [Header("Settings")]
        public bool useQuestionAnswer;
        public List<int> correctIndices;
        public bool useImageAnswers;
    }
}