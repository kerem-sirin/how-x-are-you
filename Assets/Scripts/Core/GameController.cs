using UnityEngine;
using UnityEngine.UIElements;
using HowX.Data;

namespace HowX.Core
{
    public class GameController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private QuizProfile currentProfile;

        // --- UI Elements ---
        private VisualElement root;
        private VisualElement menuContainer, quizContainer, resultContainer;
        private UIDocument uiDocument;

        // Menu UI
        private Label titleLabel;
        private Button startButton;
        private Button langButton;

        // Quiz UI
        private Label questionLabel;
        private Image questionImage;
        private Button[] answerButtons; // Array to hold our 4 buttons

        // Result UI
        private Label resultTitle;
        private Label resultDesc;
        private Label resultScore;
        private Button restartButton;

        // --- Game State ---
        private int currentQuestionIndex = 0;
        private int currentScore = 0;
        private bool isNativeLanguage = false; // false = English, true = Native

        private void Awake()
        {
            if (!TryGetComponent<UIDocument>(out uiDocument))
            {
                Debug.LogError("UIDocument missing!");
            }
        }

        private void OnEnable()
        {
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument is null!");
                return;
            }
            root = uiDocument.rootVisualElement;

            // 1. Load the "Profile" specific theme (e.g., Poland.uss)
            if (currentProfile != null && currentProfile.themeSheet != null)
            {
                if (!root.styleSheets.Contains(currentProfile.themeSheet))
                {
                    root.styleSheets.Add(currentProfile.themeSheet);
                }
            }

            // 2. Find Containers
            menuContainer = root.Q<VisualElement>("Container_Menu");
            quizContainer = root.Q<VisualElement>("Container_Quiz");
            resultContainer = root.Q<VisualElement>("Container_Result");

            // 3. Find Menu Elements
            titleLabel = root.Q<Label>("Lbl_Title");
            startButton = root.Q<Button>("Btn_Start");
            langButton = root.Q<Button>("Btn_LangToggle");

            // 4. Find Quiz Elements
            questionLabel = root.Q<Label>("Lbl_QuestionText");
            questionImage = root.Q<Image>("Img_QuestionVisual");

            // 5. Find Answer Buttons (Btn_Ans0 to Btn_Ans3)
            answerButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                answerButtons[i] = root.Q<Button>($"Btn_Ans{i}");

                // CRITICAL: Assign click event ONCE. 
                int x = i;
                answerButtons[i].clicked -= () => OnAnswerClicked(x);
                answerButtons[i].clicked += () => OnAnswerClicked(x);
            }

            // 6. Find Result Elements
            resultTitle = root.Q<Label>("Lbl_ResultTitle");
            resultDesc = root.Q<Label>("Lbl_ResultDesc");
            resultScore = root.Q<Label>("Lbl_Score");
            restartButton = root.Q<Button>("Btn_Restart");

            // 7. Hook up General Buttons
            startButton.clicked -= StartGame;
            startButton.clicked += StartGame;

            langButton.clicked -= ToggleLanguage;
            langButton.clicked += ToggleLanguage;

            restartButton.clicked -= RestartGame; // <--- Hooking up restart
            restartButton.clicked += RestartGame;

            ShowMenu();
        }

        // --- LOGIC ---

        private void ShowMenu()
        {
            menuContainer.style.display = DisplayStyle.Flex;
            quizContainer.style.display = DisplayStyle.None;
            resultContainer.style.display = DisplayStyle.None;

            if (currentProfile != null)
                titleLabel.text = $"How {currentProfile.countryAdjective} Are You?";
        }

        private void StartGame()
        {
            currentQuestionIndex = 0;
            currentScore = 0;

            menuContainer.style.display = DisplayStyle.None;
            quizContainer.style.display = DisplayStyle.Flex;

            LoadQuestion();
        }

        private void RestartGame()
        {
            // Reset state and go back to menu
            currentQuestionIndex = 0;
            currentScore = 0;

            resultContainer.style.display = DisplayStyle.None;
            ShowMenu();
        }

        private void LoadQuestion()
        {
            if (currentQuestionIndex >= currentProfile.questions.Count)
            {
                EndGame();
                return;
            }

            QuestionData q = currentProfile.questions[currentQuestionIndex];

            // Set Main Question Text
            questionLabel.text = q.text.Get(isNativeLanguage);

            // Set Main Question Image
            if (q.visual != null)
            {
                questionImage.style.display = DisplayStyle.Flex;
                questionImage.sprite = q.visual;
            }
            else
            {
                questionImage.style.display = DisplayStyle.None;
            }

            // Set Answer Buttons
            for (int i = 0; i < 4; i++)
            {
                Button btn = answerButtons[i];
                if (btn == null) continue;

                if (i < q.answers.Length)
                {
                    btn.style.display = DisplayStyle.Flex;

                    // Look for the children inside the button
                    Label txtChild = btn.Q<Label>("Lbl_Ans");
                    Image imgChild = btn.Q<Image>("Img_Ans");

                    if (q.useImageAnswers)
                    {
                        // 1. Hide Text
                        if (txtChild != null) txtChild.style.display = DisplayStyle.None;

                        // 2. Show Image
                        if (imgChild != null)
                        {
                            imgChild.style.display = DisplayStyle.Flex;
                            imgChild.sprite = q.answers[i].image;
                        }
                    }
                    else
                    {
                        // Text Mode
                        if (imgChild != null) imgChild.style.display = DisplayStyle.None;
                        if (txtChild != null)
                        {
                            txtChild.style.display = DisplayStyle.Flex;
                            txtChild.text = q.answers[i].text.Get(isNativeLanguage);
                        }
                    }
                }
                else
                {
                    btn.style.display = DisplayStyle.None;
                }
            }
        }

        private void OnAnswerClicked(int index)
        {
            if (currentQuestionIndex >= currentProfile.questions.Count) return;

            bool correct = currentProfile.questions[currentQuestionIndex].answers[index].isCorrect;
            if (correct) currentScore++;

            currentQuestionIndex++;
            LoadQuestion();
        }

        private void ToggleLanguage()
        {
            isNativeLanguage = !isNativeLanguage;
            langButton.text = isNativeLanguage ? "Native" : "English";

            if (quizContainer.style.display == DisplayStyle.Flex)
            {
                LoadQuestion();
            }
        }

        private void EndGame()
        {
            quizContainer.style.display = DisplayStyle.None;
            resultContainer.style.display = DisplayStyle.Flex;

            float percentage = (float)currentScore / currentProfile.questions.Count * 100f;

            // 1. UPDATE THE SCORE LABEL
            if (resultScore != null)
            {
                resultScore.text = $"{percentage:F0}%";
            }

            TierData earnedTier = null;
            foreach (var tier in currentProfile.tiers)
            {
                if (percentage <= tier.maxPercentage)
                {
                    earnedTier = tier;
                    break;
                }
            }

            if (earnedTier == null && currentProfile.tiers.Count > 0)
            {
                earnedTier = currentProfile.tiers[currentProfile.tiers.Count - 1];
            }

            if (earnedTier != null)
            {
                resultTitle.text = earnedTier.title.Get(isNativeLanguage);
                resultDesc.text = $"<b>{earnedTier.subtitle.Get(isNativeLanguage)}</b>\n\n{earnedTier.description.Get(isNativeLanguage)}";
            }
        }
    }
}