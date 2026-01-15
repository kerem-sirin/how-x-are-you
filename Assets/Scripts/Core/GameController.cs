using HowX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HowX.Core
{
    public class GameController : MonoBehaviour
    {
        private const float QUESTION_SLIDE_DURATION = 0.225f;
        private const string MISSING_TEXT = "TEXT MISSING";

        [Header("Reference")]
        [SerializeField] private QuizProfile currentProfile;

        [Header("Settings")]
        [SerializeField] private int questionsPerGame = 20;

        // --- UI Elements ---
        private VisualElement root;
        private VisualElement transitionPanel;
        private VisualElement menuContainer, quizContainer, resultContainer;
        private UIDocument uiDocument;
        private VisualElement progressBarFill;
        private VisualElement flagIcon;

        // Elements
        private Label titleLabel;
        private Button startButton;
        private Button langButton;
        private Label questionLabel;
        private VisualElement questionImage;
        private Button[] answerButtons;
        private Label resultTitle;
        private Label resultDesc;
        private Label resultScore;
        private Button restartButton;

        // State
        private List<QuestionData> activeQuestions;
        private int currentQuestionIndex = 0;
        private int currentScore = 0;
        private bool isNativeLanguage = false;
        private bool isTransitioning = false;

        // Shuffle Mapping
        private int[] currentShuffleIndices;

        // --- Callback Storage (for proper unsubscription) ---
        private Action[] answerClickActions;
        private Action submitAction;
        private EventCallback<PointerEnterEvent> hoverCallback;
        private List<Button> registeredAudioButtons = new List<Button>();

        private void Awake()
        {
            if (!TryGetComponent<UIDocument>(out uiDocument))
                Debug.LogError("UIDocument missing!");
        }

        private void OnEnable()
        {
            if (uiDocument == null) return;
            root = uiDocument.rootVisualElement;

            // Find Containers
            menuContainer = root.Q<VisualElement>("Container_Menu");
            quizContainer = root.Q<VisualElement>("Container_Quiz");
            resultContainer = root.Q<VisualElement>("Container_Result");
            transitionPanel = root.Q<VisualElement>("Panel_Transition");
            progressBarFill = root.Q<VisualElement>("Fill_Progress");

            // Find Elements
            titleLabel = root.Q<Label>("Lbl_Title");
            startButton = root.Q<Button>("Btn_Start");
            langButton = root.Q<Button>("Btn_LangToggle");
            questionLabel = root.Q<Label>("Lbl_QuestionText");
            questionImage = root.Q<VisualElement>("Img_QuestionVisual");
            resultTitle = root.Q<Label>("Lbl_ResultTitle");
            resultDesc = root.Q<Label>("Lbl_ResultDesc");
            resultScore = root.Q<Label>("Lbl_Score");
            restartButton = root.Q<Button>("Btn_Restart");
            flagIcon = root.Q<VisualElement>("Img_Flag");

            // Setup Answer Buttons with stored delegates
            SetupAnswerButtons();

            // Hook Menu/Result Events
            if (startButton != null)
                startButton.clicked += StartGame;

            if (langButton != null)
                langButton.clicked += ToggleLanguage;

            if (restartButton != null)
                restartButton.clicked += RestartGame;

            // Register audio for all buttons
            RegisterAllButtonAudio();

            InitializeUIState();
        }

        private void OnDisable()
        {
            // Cleanup answer button callbacks
            CleanupAnswerButtons();

            // Cleanup menu/result button callbacks
            if (startButton != null)
                startButton.clicked -= StartGame;

            if (langButton != null)
                langButton.clicked -= ToggleLanguage;

            if (restartButton != null)
                restartButton.clicked -= RestartGame;

            // Cleanup audio callbacks
            UnregisterAllButtonAudio();
        }

        private void SetupAnswerButtons()
        {
            answerButtons = new Button[4];
            answerClickActions = new Action[4];

            for (int i = 0; i < 4; i++)
            {
                answerButtons[i] = root.Q<Button>($"Btn_Ans{i}");

                // Fallback: try to find by index in grid
                if (answerButtons[i] == null)
                {
                    var grid = root.Q("Grid_Answers");
                    if (grid != null && i < grid.childCount)
                        answerButtons[i] = grid.ElementAt(i) as Button;
                }

                if (answerButtons[i] != null)
                {
                    // Create and store the delegate with captured index
                    int capturedIndex = i;
                    answerClickActions[i] = () => OnAnswerClicked(capturedIndex);
                    answerButtons[i].clicked += answerClickActions[i];
                }
            }
        }

        private void CleanupAnswerButtons()
        {
            if (answerButtons == null || answerClickActions == null) return;

            for (int i = 0; i < 4; i++)
            {
                if (answerButtons[i] != null && answerClickActions[i] != null)
                {
                    answerButtons[i].clicked -= answerClickActions[i];
                }
            }
        }

        private void RegisterAllButtonAudio()
        {
            // Create reusable callbacks
            submitAction = OnSubmitAudio;
            hoverCallback = OnHoverAudio;

            // Find and register all buttons
            registeredAudioButtons.Clear();
            root.Query<Button>().ForEach(btn =>
            {
                btn.clicked += submitAction;
                btn.RegisterCallback(hoverCallback);
                registeredAudioButtons.Add(btn);
            });
        }

        private void UnregisterAllButtonAudio()
        {
            if (submitAction == null || hoverCallback == null) return;

            foreach (var btn in registeredAudioButtons)
            {
                if (btn != null)
                {
                    btn.clicked -= submitAction;
                    btn.UnregisterCallback(hoverCallback);
                }
            }
            registeredAudioButtons.Clear();
        }

        private void OnSubmitAudio()
        {
            if (!isTransitioning)
                GameEvents.OnSubmit?.Invoke();
        }

        private void OnHoverAudio(PointerEnterEvent evt)
        {
            if (!isTransitioning)
                GameEvents.OnHover?.Invoke();
        }

        private void InitializeUIState()
        {
            if (transitionPanel != null)
            {
                transitionPanel.AddToClassList("curtain-hidden");
                transitionPanel.pickingMode = PickingMode.Ignore;
            }
            if (quizContainer != null)
            {
                quizContainer.RemoveFromClassList("quiz-left");
                quizContainer.RemoveFromClassList("quiz-right");
                quizContainer.AddToClassList("quiz-center");
                quizContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(QUESTION_SLIDE_DURATION, TimeUnit.Second) };
                quizContainer.style.display = DisplayStyle.None;
            }
            if (progressBarFill != null) progressBarFill.style.width = Length.Percent(0);
            if (resultContainer != null) resultContainer.style.display = DisplayStyle.None;
            if (menuContainer != null) menuContainer.style.display = DisplayStyle.Flex;

            UpdateUIText();
            GameEvents.OnGameRestart?.Invoke();
        }

        // LOCALIZATION LOGIC ---
        private void UpdateUIText()
        {
            // Safety Check
            if (currentProfile == null || currentProfile.uiData == null) return;

            // 1. Dynamic Title
            if (titleLabel != null)
            {
                string text = currentProfile.uiData.titleTemplate?.Get(isNativeLanguage);

                if (string.IsNullOrEmpty(text))
                {
                    titleLabel.text = MISSING_TEXT;
                }
                else
                {
                    try
                    {
                        titleLabel.text = text;
                    }
                    catch
                    {
                        titleLabel.text = MISSING_TEXT;
                    }
                }
            }

            // 2. Start Button
            if (startButton != null)
            {
                string text = currentProfile.uiData.btnStart?.Get(isNativeLanguage);
                startButton.text = string.IsNullOrEmpty(text) ? MISSING_TEXT : text;
            }

            // 3. Restart Button
            if (restartButton != null)
            {
                string text = currentProfile.uiData.btnRestart?.Get(isNativeLanguage);
                restartButton.text = string.IsNullOrEmpty(text) ? MISSING_TEXT : text;
            }

            // 4. Language Button
            if (langButton != null)
            {
                string text = currentProfile.uiData.btnLang?.Get(isNativeLanguage);
                langButton.text = string.IsNullOrEmpty(text) ? MISSING_TEXT : text;
            }

            // 5. Flag (Menu)
            if (flagIcon != null && currentProfile.uiData.flagIcon != null)
            {
                flagIcon.style.backgroundImage = currentProfile.uiData.flagIcon;
                flagIcon.style.display = DisplayStyle.Flex;
            }
        }

        // --- GAME FLOW ---
        private void StartGame()
        {
            if (isTransitioning) return;

            // Validate profile
            if (currentProfile == null)
            {
                Debug.LogError("No Quiz Profile assigned!");
                return;
            }

            if (currentProfile.questions == null || currentProfile.questions.Count == 0)
            {
                Debug.LogError($"Profile '{currentProfile.name}' has no questions!");
                return;
            }

            // Shuffle questions (Fisher-Yates)
            activeQuestions = new List<QuestionData>(currentProfile.questions);
            for (int i = 0; i < activeQuestions.Count; i++)
            {
                QuestionData temp = activeQuestions[i];
                int randomIndex = UnityEngine.Random.Range(i, activeQuestions.Count);
                activeQuestions[i] = activeQuestions[randomIndex];
                activeQuestions[randomIndex] = temp;
            }

            // Limit to questionsPerGame
            if (questionsPerGame > 0 && activeQuestions.Count > questionsPerGame)
            {
                activeQuestions = activeQuestions.GetRange(0, questionsPerGame);
            }

            currentQuestionIndex = 0;
            currentScore = 0;

            if (progressBarFill != null) progressBarFill.style.width = Length.Percent(0);

            GameEvents.OnGameStart?.Invoke();
            LoadQuestion();

            StartCoroutine(SwitchPanels(menuContainer, quizContainer));
        }

        private void LoadQuestion()
        {
            if (activeQuestions == null || currentQuestionIndex >= activeQuestions.Count) return;

            QuestionData q = activeQuestions[currentQuestionIndex];

            // Shuffle answer indices (Fisher-Yates)
            currentShuffleIndices = new int[] { 0, 1, 2, 3 };
            for (int i = 0; i < currentShuffleIndices.Length; i++)
            {
                int rnd = UnityEngine.Random.Range(i, currentShuffleIndices.Length);
                int temp = currentShuffleIndices[i];
                currentShuffleIndices[i] = currentShuffleIndices[rnd];
                currentShuffleIndices[rnd] = temp;
            }

            // Set question text
            if (questionLabel != null)
                questionLabel.text = q.questionText?.Get(isNativeLanguage) ?? MISSING_TEXT;

            // Set question image
            if (questionImage != null)
            {
                if (q.useQuestionAnswer && q.questionVisual != null)
                {
                    questionImage.style.display = DisplayStyle.Flex;
                    questionImage.style.backgroundImage = q.questionVisual;
                }
                else
                {
                    questionImage.style.display = DisplayStyle.None;
                }
            }

            // Toggle grid/list layout
            VisualElement gridContainer = root.Q<VisualElement>("Grid_Answers");
            if (gridContainer != null)
            {
                gridContainer.EnableInClassList("layout-grid", q.useImageAnswers);
                gridContainer.EnableInClassList("layout-list", !q.useImageAnswers);
            }

            // Populate answer buttons
            for (int i = 0; i < 4; i++)
            {
                Button btn = answerButtons[i];
                if (btn == null) continue;

                int realDataIndex = currentShuffleIndices[i];
                GetAnswerData(q, realDataIndex, out string ansText, out Texture2D ansImg);

                if (!string.IsNullOrEmpty(ansText) || ansImg != null)
                {
                    btn.style.display = DisplayStyle.Flex;
                    btn.style.backgroundColor = StyleKeyword.Null;

                    Label txtChild = btn.Q<Label>("Lbl_Ans");
                    VisualElement imgChild = btn.Q<VisualElement>("Img_Ans");

                    if (q.useImageAnswers)
                    {
                        if (txtChild != null) txtChild.style.display = DisplayStyle.None;
                        if (imgChild != null)
                        {
                            imgChild.style.display = DisplayStyle.Flex;
                            if (ansImg != null) imgChild.style.backgroundImage = ansImg;
                        }
                    }
                    else
                    {
                        if (imgChild != null) imgChild.style.display = DisplayStyle.None;
                        if (txtChild != null)
                        {
                            txtChild.style.display = DisplayStyle.Flex;
                            txtChild.text = ansText;
                            txtChild.style.whiteSpace = WhiteSpace.Normal;
                        }
                    }
                }
                else
                {
                    btn.style.display = DisplayStyle.None;
                }
            }
        }

        private void OnAnswerClicked(int btnIndex)
        {
            if (isTransitioning) return;
            if (activeQuestions == null || currentQuestionIndex >= activeQuestions.Count) return;

            QuestionData q = activeQuestions[currentQuestionIndex];
            int realDataIndex = currentShuffleIndices[btnIndex];

            bool correct = q.correctIndices != null && q.correctIndices.Contains(realDataIndex);
            if (correct) currentScore++;

            currentQuestionIndex++;

            if (currentQuestionIndex >= activeQuestions.Count)
            {
                EndGame();
                return;
            }

            StartCoroutine(NextQuestionSequence());
        }

        private void EndGame()
        {
            if (progressBarFill != null) progressBarFill.style.width = Length.Percent(100);

            float totalQs = activeQuestions != null ? activeQuestions.Count : 1;
            float percentage = (float)currentScore / totalQs * 100f;

            if (resultScore != null) resultScore.text = $"{percentage:F0}%";

            // Find earned tier
            TierData earnedTier = null;
            if (currentProfile?.tiers != null)
            {
                foreach (var tier in currentProfile.tiers)
                {
                    if (percentage <= tier.maxPercentage)
                    {
                        earnedTier = tier;
                        break;
                    }
                }
                // Fallback to highest tier
                if (earnedTier == null && currentProfile.tiers.Count > 0)
                    earnedTier = currentProfile.tiers[currentProfile.tiers.Count - 1];
            }

            if (earnedTier != null)
            {
                if (resultTitle != null)
                    resultTitle.text = earnedTier.title?.Get(isNativeLanguage) ?? "";

                if (resultDesc != null)
                {
                    string subtitle = earnedTier.subtitle?.Get(isNativeLanguage) ?? "";
                    string desc = earnedTier.description?.Get(isNativeLanguage) ?? "";
                    resultDesc.text = $"<b>{subtitle}</b>\n\n{desc}";
                }
            }

            GameEvents.OnGameEnd?.Invoke();
            StartCoroutine(SwitchPanels(quizContainer, resultContainer));
        }

        private void UpdateProgressBar()
        {
            if (progressBarFill == null || activeQuestions == null || activeQuestions.Count == 0) return;

            float progress = (float)currentQuestionIndex / activeQuestions.Count * 100f;
            if (progress > 100f) progress = 100f;
            progressBarFill.style.width = Length.Percent(progress);
        }

        // --- HELPERS ---

        private void GetAnswerData(QuestionData q, int index, out string text, out Texture2D image)
        {
            text = "";
            image = null;

            if (q == null) return;

            switch (index)
            {
                case 0:
                    text = q.answer0?.Get(isNativeLanguage) ?? "";
                    image = q.answer0_Image;
                    break;
                case 1:
                    text = q.answer1?.Get(isNativeLanguage) ?? "";
                    image = q.answer1_Image;
                    break;
                case 2:
                    text = q.answer2?.Get(isNativeLanguage) ?? "";
                    image = q.answer2_Image;
                    break;
                case 3:
                    text = q.answer3?.Get(isNativeLanguage) ?? "";
                    image = q.answer3_Image;
                    break;
            }
        }

        private void ShowMenu()
        {
            if (isTransitioning) return;
            UpdateUIText();
            GameEvents.OnGameRestart?.Invoke();
            VisualElement currentActive = resultContainer?.style.display == DisplayStyle.Flex ? resultContainer : quizContainer;
            StartCoroutine(SwitchPanels(currentActive, menuContainer));
        }

        private void RestartGame()
        {
            if (isTransitioning) return;
            ShowMenu();
        }

        private void ToggleLanguage()
        {
            isNativeLanguage = !isNativeLanguage;
            UpdateUIText();

            if (quizContainer?.style.display == DisplayStyle.Flex)
                LoadQuestion();

            if (resultContainer?.style.display == DisplayStyle.Flex)
                EndGame(); // Refreshes result text
        }

        private IEnumerator SwitchPanels(VisualElement outPanel, VisualElement inPanel)
        {
            if (isTransitioning) yield break;
            isTransitioning = true;

            if (transitionPanel != null)
            {
                transitionPanel.pickingMode = PickingMode.Position;
                float ignored = transitionPanel.resolvedStyle.opacity;
                if (transitionPanel.ClassListContains("curtain-hidden"))
                    transitionPanel.RemoveFromClassList("curtain-hidden");
                yield return new WaitForSeconds(0.5f);
            }

            if (outPanel != null) outPanel.style.display = DisplayStyle.None;
            if (inPanel != null) inPanel.style.display = DisplayStyle.Flex;

            yield return null;

            if (transitionPanel != null)
            {
                transitionPanel.AddToClassList("curtain-hidden");
                yield return new WaitForSeconds(0.5f);
                transitionPanel.pickingMode = PickingMode.Ignore;
            }

            isTransitioning = false;
        }

        private IEnumerator NextQuestionSequence()
        {
            isTransitioning = true;
            if (quizContainer != null) quizContainer.pickingMode = PickingMode.Ignore;
            GameEvents.OnNextQuestion?.Invoke();

            if (quizContainer != null)
            {
                quizContainer.RemoveFromClassList("quiz-center");
                quizContainer.AddToClassList("quiz-right");
                yield return new WaitForSeconds(QUESTION_SLIDE_DURATION);
            }

            if (currentQuestionIndex < activeQuestions.Count)
            {
                LoadQuestion();
                UpdateProgressBar();
            }

            if (quizContainer != null)
            {
                quizContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(0, TimeUnit.Second) };
                quizContainer.RemoveFromClassList("quiz-right");
                yield return null;
                quizContainer.AddToClassList("quiz-left");
                yield return null;
                quizContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(QUESTION_SLIDE_DURATION, TimeUnit.Second) };
            }

            if (quizContainer != null)
            {
                quizContainer.RemoveFromClassList("quiz-left");
                quizContainer.AddToClassList("quiz-center");
                yield return new WaitForSeconds(QUESTION_SLIDE_DURATION);
                quizContainer.pickingMode = PickingMode.Position;
            }

            isTransitioning = false;
        }
    }
}