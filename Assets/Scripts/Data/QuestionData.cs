using UnityEngine;

namespace HowX.Data
{
    [CreateAssetMenu(fileName = "NewQuestion", menuName = "HowX/Question")]
    public class QuestionData : ScriptableObject
    {
        [Header("Question Content")]
        [TextArea(2, 5)] public string text_EN;
        [TextArea(2, 5)] public string text_Native;

        public Sprite visual;
        public bool useImageAnswers;

        [Header("Answers")]
        public Answer[] answers;

        [Header("Post-Question Info")]
        [TextArea(2, 5)] public string explanation_EN;
        [TextArea(2, 5)] public string explanation_Native;
    }

    [System.Serializable]
    public class Answer
    {
        public string text_EN;
        public string text_Native;
        public Sprite image;
        public bool isCorrect;
    }
}