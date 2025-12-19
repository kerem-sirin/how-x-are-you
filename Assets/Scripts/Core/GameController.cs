using System.Collections;
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
        private VisualElement transitionPanel;
        private VisualElement menuContainer, quizContainer, resultContainer;
        private UIDocument uiDocument;

        // Elements
        private Label titleLabel;
        private Button startButton;
        private Button langButton;
        private Label questionLabel;
        private Image questionImage;
        private Button[] answerButtons;
        private Label resultTitle;
        private Label resultDesc;
        private Label resultScore;
        private Button restartButton;

        // State
        private int currentQuestionIndex = 0;
        private int currentScore = 0;
        private bool isNativeLanguage = false;
        private bool isTransitioning = false;

        private void Awake()
        {
            if (!TryGetComponent<UIDocument>(out uiDocument))
                Debug.LogError("UIDocument missing!");
        }

        private void OnEnable()
        {
            if (uiDocument == null) return;
            root = uiDocument.rootVisualElement;

            if (currentProfile != null && currentProfile.themeSheet != null)
            {
                if (!root.styleSheets.Contains(currentProfile.themeSheet))
                    root.styleSheets.Add(currentProfile.themeSheet);
            }

            // Find Containers
            menuContainer = root.Q<VisualElement>("Container_Menu");
            quizContainer = root.Q<VisualElement>("Container_Quiz");
            resultContainer = root.Q<VisualElement>("Container_Result");

            // NEW: Find the Curtain
            transitionPanel = root.Q<VisualElement>("Panel_Transition");

            // Find Elements
            titleLabel = root.Q<Label>("Lbl_Title");
            startButton = root.Q<Button>("Btn_Start");
            langButton = root.Q<Button>("Btn_LangToggle");
            questionLabel = root.Q<Label>("Lbl_QuestionText");
            questionImage = root.Q<Image>("Img_QuestionVisual");
            resultTitle = root.Q<Label>("Lbl_ResultTitle");
            resultDesc = root.Q<Label>("Lbl_ResultDesc");
            resultScore = root.Q<Label>("Lbl_Score");
            restartButton = root.Q<Button>("Btn_Restart");

            // Setup Buttons
            answerButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                answerButtons[i] = root.Q<Button>($"Btn_Ans{i}");
                int x = i;
                answerButtons[i].clicked -= () => OnAnswerClicked(x);
                answerButtons[i].clicked += () => OnAnswerClicked(x);
            }

            // Hook Events
            startButton.clicked -= StartGame;
            startButton.clicked += StartGame;
            langButton.clicked -= ToggleLanguage;
            langButton.clicked += ToggleLanguage;
            restartButton.clicked -= RestartGame;
            restartButton.clicked += RestartGame;

            root.Query<Button>().ForEach(RegisterButtonAudio);

            // Instant Init (No animation on startup)
            InitializeUIState();
        }

        private void InitializeUIState()
        {
            // Reset Transition Panel
            if (transitionPanel != null)
            {
                // Start hidden
                // We ensure the class is definitely there.
                if (!transitionPanel.ClassListContains("curtain-hidden"))
                {
                    transitionPanel.AddToClassList("curtain-hidden");
                }
                transitionPanel.pickingMode = PickingMode.Ignore;
            }

            menuContainer.style.display = DisplayStyle.Flex;
            quizContainer.style.display = DisplayStyle.None;
            resultContainer.style.display = DisplayStyle.None;

            UpdateMenuText();
            GameEvents.OnGameRestart?.Invoke();
        }

        // --- NEW CURTAIN TRANSITION LOGIC ---
        private IEnumerator SwitchPanels(VisualElement outPanel, VisualElement inPanel)
        {
            if (isTransitioning) yield break;
            isTransitioning = true;

            // 1. CURTAIN UP (Fade In)
            if (transitionPanel != null)
            {
                transitionPanel.pickingMode = PickingMode.Position;

                // B. Force Layout Update (Crucial!)
                // Tell Unity: "Calculate that I am at Opacity 0 right now."
                float ignored = transitionPanel.resolvedStyle.opacity;

                // C. Trigger Animation (Target: Visible / Opacity 1)
                // By removing the "Hidden" class, it reverts to the ID's default (Opacity 1).
                if (transitionPanel.ClassListContains("curtain-hidden"))
                {
                    transitionPanel.RemoveFromClassList("curtain-hidden");
                }

                yield return new WaitForSeconds(0.5f);
            }

            // 2. SWAP PANELS
            if (outPanel != null) outPanel.style.display = DisplayStyle.None;
            if (inPanel != null) inPanel.style.display = DisplayStyle.Flex;

            yield return null;

            // 3. CURTAIN DOWN (Fade Out)
            if (transitionPanel != null)
            {
                // Target: Hidden / Opacity 0
                transitionPanel.AddToClassList("curtain-hidden");

                yield return new WaitForSeconds(0.5f);

                transitionPanel.pickingMode = PickingMode.Ignore;
            }

            isTransitioning = false;
        }

        private void RegisterButtonAudio(Button btn)
        {
            // We don't strictly need !isTransitioning here anymore because 
            // the Curtain blocks clicks, but it's good safety.
            btn.clicked -= () => { if (!isTransitioning) GameEvents.OnSubmit?.Invoke(); };
            btn.clicked += () => { if (!isTransitioning) GameEvents.OnSubmit?.Invoke(); };
            btn.RegisterCallback<PointerEnterEvent>(evt => { if (!isTransitioning) GameEvents.OnHover?.Invoke(); });
        }

        // --- GAME FLOW (Same as before) ---

        private void ShowMenu()
        {
            if (isTransitioning) return;
            UpdateMenuText();
            GameEvents.OnGameRestart?.Invoke();

            VisualElement currentActive = resultContainer.style.display == DisplayStyle.Flex ? resultContainer : quizContainer;
            StartCoroutine(SwitchPanels(currentActive, menuContainer));
        }

        private void UpdateMenuText()
        {
            if (currentProfile != null)
                titleLabel.text = $"How {currentProfile.countryAdjective} Are You?";
        }

        private void StartGame()
        {
            if (isTransitioning) return;

            currentQuestionIndex = 0;
            currentScore = 0;

            GameEvents.OnGameStart?.Invoke();
            LoadQuestion();

            StartCoroutine(SwitchPanels(menuContainer, quizContainer));
        }

        private void RestartGame()
        {
            if (isTransitioning) return;
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

            VisualElement gridContainer = root.Q<VisualElement>("Grid_Answers");
            gridContainer.EnableInClassList("layout-grid", q.useImageAnswers);
            gridContainer.EnableInClassList("layout-list", !q.useImageAnswers);

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
            if (isTransitioning) return;

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
            float percentage = (float)currentScore / currentProfile.questions.Count * 100f;
            if (resultScore != null) resultScore.text = $"{percentage:F0}%";

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
                earnedTier = currentProfile.tiers[currentProfile.tiers.Count - 1];

            if (earnedTier != null)
            {
                resultTitle.text = earnedTier.title.Get(isNativeLanguage);
                resultDesc.text = $"<b>{earnedTier.subtitle.Get(isNativeLanguage)}</b>\n\n{earnedTier.description.Get(isNativeLanguage)}";
            }

            GameEvents.OnGameEnd?.Invoke();
            StartCoroutine(SwitchPanels(quizContainer, resultContainer));
        }
    }
}