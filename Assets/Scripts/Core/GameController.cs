using HowX.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HowX.Core
{
    public class GameController : MonoBehaviour
    {
        private const float QUESTION_SLIDE_DURATION = 0.225f;

        [Header("Reference")]
        [SerializeField] private QuizProfile currentProfile;

        // --- UI Elements ---
        private VisualElement root;
        private VisualElement transitionPanel;
        private VisualElement menuContainer, quizContainer, resultContainer;
        private UIDocument uiDocument;
        private VisualElement progressBarFill;

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

            // Find the Curtain
            transitionPanel = root.Q<VisualElement>("Panel_Transition");

            // Find Progress Bar Fill
            progressBarFill = root.Q<VisualElement>("Fill_Progress");

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
                if (answerButtons[i] != null)
                {
                    answerButtons[i].clicked -= () => OnAnswerClicked(x);
                    answerButtons[i].clicked += () => OnAnswerClicked(x);
                }
            }

            // Hook Events
            if (startButton != null)
            {
                startButton.clicked -= StartGame;
                startButton.clicked += StartGame;
            }
            if (langButton != null)
            {
                langButton.clicked -= ToggleLanguage;
                langButton.clicked += ToggleLanguage;
            }
            if (restartButton != null)
            {
                restartButton.clicked -= RestartGame;
                restartButton.clicked += RestartGame;
            }

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
                transitionPanel.AddToClassList("curtain-hidden");
                transitionPanel.pickingMode = PickingMode.Ignore;
            }

            // Ensure Quiz Container starts in the correct state
            if (quizContainer != null)
            {
                quizContainer.RemoveFromClassList("quiz-left");
                quizContainer.RemoveFromClassList("quiz-right");
                quizContainer.AddToClassList("quiz-center");

                // OVERRIDE the USS value with your C# constant
                quizContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(QUESTION_SLIDE_DURATION, TimeUnit.Second) };

                quizContainer.style.display = DisplayStyle.None;
            }

            // Reset Progress Bar
            if (progressBarFill != null)
            {
                progressBarFill.style.width = Length.Percent(0);
            }

            // Hide Results
            if (resultContainer != null) resultContainer.style.display = DisplayStyle.None;

            // Show Menu
            if (menuContainer != null) menuContainer.style.display = DisplayStyle.Flex;

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
                float ignored = transitionPanel.resolvedStyle.opacity;

                // C. Trigger Animation (Target: Visible / Opacity 1)
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
            btn.clicked -= () => { if (!isTransitioning) GameEvents.OnSubmit?.Invoke(); };
            btn.clicked += () => { if (!isTransitioning) GameEvents.OnSubmit?.Invoke(); };
            btn.RegisterCallback<PointerEnterEvent>(evt => { if (!isTransitioning) GameEvents.OnHover?.Invoke(); });
        }

        // --- GAME FLOW ---

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

            // Reset Progress Bar at Start
            if (progressBarFill != null) progressBarFill.style.width = Length.Percent(0);

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
            if (currentQuestionIndex >= currentProfile.questions.Count) return;

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
            if (gridContainer != null)
            {
                gridContainer.EnableInClassList("layout-grid", q.useImageAnswers);
                gridContainer.EnableInClassList("layout-list", !q.useImageAnswers);
            }

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

        // --- NEW: PROGRESS BAR HELPER ---
        private void UpdateProgressBar()
        {
            if (progressBarFill == null || currentProfile == null || currentProfile.questions.Count == 0) return;

            // Calculate progress (e.g., 1 answered / 10 total = 10%)
            // We use currentQuestionIndex because it has already been incremented 
            // by OnAnswerClicked before this method runs.
            float progress = (float)currentQuestionIndex / currentProfile.questions.Count * 100f;

            if (progress > 100f) progress = 100f;

            progressBarFill.style.width = Length.Percent(progress);
        }

        // --- LOGIC ---

        private void OnAnswerClicked(int index)
        {
            // 1. Block interactions
            if (isTransitioning) return;

            // 2. Check Logic (Score update happens BEFORE animation starts)
            if (currentQuestionIndex >= currentProfile.questions.Count) return;

            bool correct = currentProfile.questions[currentQuestionIndex].answers[index].isCorrect;
            if (correct) currentScore++;

            // Increment index NOW so the next question is loaded during the transition
            currentQuestionIndex++;

            // 4. If last question, end game
            if (currentQuestionIndex >= currentProfile.questions.Count)
            {
                EndGame();
                return;
            }

            // 3. Start the Visual Sequence
            StartCoroutine(NextQuestionSequence());
        }

        private IEnumerator NextQuestionSequence()
        {
            isTransitioning = true;

            // DISABLE INTERACTION: No clicks, no hover effects
            if (quizContainer != null) quizContainer.pickingMode = PickingMode.Ignore;

            // === PHASE 1: EXIT ===
            GameEvents.OnNextQuestion?.Invoke();

            if (quizContainer != null)
            {
                quizContainer.RemoveFromClassList("quiz-center");
                quizContainer.AddToClassList("quiz-right");
                yield return new WaitForSeconds(QUESTION_SLIDE_DURATION);
            }

            // === PHASE 2: UPDATE DATA ===
            // This is the "Invisible Window"
            if (currentQuestionIndex < currentProfile.questions.Count)
            {
                LoadQuestion();

                //Update Progress Bar
                UpdateProgressBar();
            }

            // === PHASE 3: TELEPORT ===
            if (quizContainer != null)
            {
                quizContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };

                quizContainer.RemoveFromClassList("quiz-right");
                yield return null;
                quizContainer.AddToClassList("quiz-left");
                yield return null;

                quizContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(QUESTION_SLIDE_DURATION, TimeUnit.Second) };
            }

            // === PHASE 4: ENTER ===
            if (quizContainer != null)
            {
                quizContainer.RemoveFromClassList("quiz-left");
                quizContainer.AddToClassList("quiz-center");
                yield return new WaitForSeconds(QUESTION_SLIDE_DURATION);

                // RE-ENABLE INTERACTION: Clicks and hovers allowed again
                quizContainer.pickingMode = PickingMode.Position;
            }

            isTransitioning = false;
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
            // Fill progress bar to 100% on game end
            if (progressBarFill != null) progressBarFill.style.width = Length.Percent(100);

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