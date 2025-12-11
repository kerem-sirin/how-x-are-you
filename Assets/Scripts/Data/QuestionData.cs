using UnityEngine;

namespace HowX.Data
{
    [CreateAssetMenu(fileName = "NewQuestion", menuName = "HowX/Question")]
    public class QuestionData : ScriptableObject
    {
        [Header("Question Content")]
        public LocalizedText text;

        public Sprite visual;
        public bool useImageAnswers;

        [Header("Answers")]
        public Answer[] answers;

        [Header("Post-Question Info")]
        public LocalizedText explanation;
    }

    [System.Serializable]
    public class Answer
    {
        public LocalizedText text;
        public Sprite image;
        public bool isCorrect;
    }
}