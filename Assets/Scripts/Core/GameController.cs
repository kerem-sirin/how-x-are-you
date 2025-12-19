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
        private Button[] answerButtons;

        // Result UI
        private Label resultTitle;
        private Label resultDesc;
        private Label resultScore;
        private Button restartButton;

        // --- Game State ---
        private int currentQuestionIndex = 0;
        private int currentScore = 0;
        private bool isNativeLanguage = false;

        private void Awake()
        {
            if (!TryGetComponent<UIDocument>(out uiDocument))
            {
                Debug.LogError("UIDocument missing!");
            }
        }

        private void OnEnable()
        {
            if (uiDocument == null) return;
            root = uiDocument.rootVisualElement;

            // 1. Load Theme
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

            // 3. Find Elements
            titleLabel = root.Q<Label>("Lbl_Title");
            startButton = root.Q<Button>("Btn_Start");
            langButton = root.Q<Button>("Btn_LangToggle");
            questionLabel = root.Q<Label>("Lbl_QuestionText");
            questionImage = root.Q<Image>("Img_QuestionVisual");
            resultTitle = root.Q<Label>("Lbl_ResultTitle");
            resultDesc = root.Q<Label>("Lbl_ResultDesc");
            resultScore = root.Q<Label>("Lbl_Score");
            restartButton = root.Q<Button>("Btn_Restart");

            // 4. Setup Answer Buttons
            answerButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                answerButtons[i] = root.Q<Button>($"Btn_Ans{i}");
                int x = i;
                // Only game logic subscription here
                answerButtons[i].clicked -= () => OnAnswerClicked(x);
                answerButtons[i].clicked += () => OnAnswerClicked(x);
            }

            // 5. Hook up General Logic
            startButton.clicked -= StartGame;
            startButton.clicked += StartGame;
            langButton.clicked -= ToggleLanguage;
            langButton.clicked += ToggleLanguage;
            restartButton.clicked -= RestartGame;
            restartButton.clicked += RestartGame;

            // 6. --- GLOBAL AUDIO BINDING ---
            // Find EVERY button in the hierarchy (Menu, Quiz, Results)
            root.Query<Button>().ForEach(RegisterButtonAudio);

            ShowMenu();
        }

        // Helper to bind audio events to any button found
        private void RegisterButtonAudio(Button btn)
        {
            // Fire Click Event
            btn.clicked -= () => GameEvents.OnSubmit?.Invoke();
            btn.clicked += () => GameEvents.OnSubmit?.Invoke();

            // Fire Hover Event
            btn.RegisterCallback<PointerEnterEvent>(evt => GameEvents.OnHover?.Invoke());
        }

        // --- LOGIC ---

        private void ShowMenu()
        {
            menuContainer.style.display = DisplayStyle.Flex;
            quizContainer.style.display = DisplayStyle.None;
            resultContainer.style.display = DisplayStyle.None;

            if (currentProfile != null)
                titleLabel.text = $"How {currentProfile.countryAdjective} Are You?";

            // Notify System: Back to Menu
            GameEvents.OnGameRestart?.Invoke();
        }

        private void StartGame()
        {
            currentQuestionIndex = 0;
            currentScore = 0;

            menuContainer.style.display = DisplayStyle.None;
            quizContainer.style.display = DisplayStyle.Flex;

            // Notify System: Game Started
            GameEvents.OnGameStart?.Invoke();

            LoadQuestion();
        }

        private void RestartGame()
        {
            currentQuestionIndex = 0;
            currentScore = 0;

            resultContainer.style.display = DisplayStyle.None;
            ShowMenu();
            // Note: ShowMenu() invokes OnGameRestart, so we don't need to do it here
        }

        private void LoadQuestion()
        {
            if (currentQuestionIndex >= currentProfile.questions.Count)
            {
                EndGame();
                return;
            }

            QuestionData q = currentProfile.questions[currentQuestionIndex];

            // Set Text/Visual
            questionLabel.text = q.text.Get(isNativeLanguage);

            if (q.visual != null)
            {
                questionImage.style.display = DisplayStyle.Flex;
                questionImage.sprite = q.visual;
            }
            else
            {
                questionImage.style.display = DisplayStyle.None;
            }

            // Layout Switching
            VisualElement gridContainer = root.Q<VisualElement>("Grid_Answers");
            gridContainer.EnableInClassList("layout-grid", q.useImageAnswers);
            gridContainer.EnableInClassList("layout-list", !q.useImageAnswers);

            // Button Setup
            for (int i = 0; i < 4; i++)
            {
                Button btn = answerButtons[i];
                if (btn == null) continue;

                if (i < q.answers.Length)
                {
                    btn.style.display = DisplayStyle.Flex;
                    Label txtChild = btn.Q<Label>("Lbl_Ans");
                    Image imgChild = btn.Q<Image>("Img_Ans");

                    if (q.useImageAnswers)
                    {
                        if (txtChild != null) txtChild.style.display = DisplayStyle.None;
                        if (imgChild != null)
                        {
                            imgChild.style.display = DisplayStyle.Flex;
                            imgChild.sprite = q.answers[i].image;
                            imgChild.scaleMode = ScaleMode.ScaleToFit;
                        }
                    }
                    else
                    {
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
            // Notify System: Game Over
            GameEvents.OnGameEnd?.Invoke();

            quizContainer.style.display = DisplayStyle.None;
            resultContainer.style.display = DisplayStyle.Flex;

            float percentage = (float)currentScore / currentProfile.questions.Count * 100f;

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