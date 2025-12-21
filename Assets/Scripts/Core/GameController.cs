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

        [Header("Settings")]
        [SerializeField] private int questionsPerGame = 20;

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

        // --- NEW: Shuffle Mapping ---
        // This array holds the real data index for each button position.
        // e.g., currentShuffleIndices[0] = 2 means "Button 0 holds Answer 2"
        private int[] currentShuffleIndices;

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

            // Setup Buttons
            answerButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                answerButtons[i] = root.Q<Button>($"Btn_Ans{i}");
                if (answerButtons[i] == null)
                {
                    var grid = root.Q("Grid_Answers");
                    if (grid != null && i < grid.childCount)
                        answerButtons[i] = grid.ElementAt(i) as Button;
                }

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

            InitializeUIState();
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

            UpdateMenuText();
            GameEvents.OnGameRestart?.Invoke();
        }

        // --- GAME FLOW ---

        private void StartGame()
        {
            if (isTransitioning) return;
            if (currentProfile == null || currentProfile.questions.Count == 0)
            {
                Debug.LogError("No Profile assigned!");
                return;
            }

            // 1. Prepare & Shuffle Questions
            activeQuestions = new List<QuestionData>(currentProfile.questions);
            for (int i = 0; i < activeQuestions.Count; i++)
            {
                QuestionData temp = activeQuestions[i];
                int randomIndex = Random.Range(i, activeQuestions.Count);
                activeQuestions[i] = activeQuestions[randomIndex];
                activeQuestions[randomIndex] = temp;
            }

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

            // Create & Shuffle Answer Mapping ---
            currentShuffleIndices = new int[] { 0, 1, 2, 3 };

            // Fisher-Yates Shuffle for the answers
            for (int i = 0; i < currentShuffleIndices.Length; i++)
            {
                int rnd = Random.Range(i, currentShuffleIndices.Length);
                int temp = currentShuffleIndices[i];
                currentShuffleIndices[i] = currentShuffleIndices[rnd];
                currentShuffleIndices[rnd] = temp;
            }

            // 2. Set Question UI
            questionLabel.text = q.questionText.Get(isNativeLanguage);

            if (q.useQuestionAnswer && q.questionVisual != null)
            {
                questionImage.style.display = DisplayStyle.Flex;
                questionImage.style.backgroundImage = q.questionVisual;
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

            // 3. Populate Buttons using the SHUFFLED indices
            for (int i = 0; i < 4; i++)
            {
                Button btn = answerButtons[i];
                if (btn == null) continue;

                // Ask the shuffle map which data to show
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

            // --- KEY CHANGE: Unwrap the shuffle to find the real data index ---
            int realDataIndex = currentShuffleIndices[btnIndex];

            // Logic: Does the list contain the REAL data index?
            bool correct = false;
            if (q.correctIndices != null && q.correctIndices.Contains(realDataIndex))
            {
                correct = true;
            }

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
            switch (index)
            {
                case 0: text = q.answer0.Get(isNativeLanguage); image = q.answer0_Image; break;
                case 1: text = q.answer1.Get(isNativeLanguage); image = q.answer1_Image; break;
                case 2: text = q.answer2.Get(isNativeLanguage); image = q.answer2_Image; break;
                case 3: text = q.answer3.Get(isNativeLanguage); image = q.answer3_Image; break;
            }
        }

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

        private void RestartGame()
        {
            if (isTransitioning) return;
            ShowMenu();
        }

        private void ToggleLanguage()
        {
            isNativeLanguage = !isNativeLanguage;
            if (langButton != null) langButton.text = isNativeLanguage ? "Native" : "English";
            if (quizContainer.style.display == DisplayStyle.Flex) LoadQuestion();
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

        private void RegisterButtonAudio(Button btn)
        {
            btn.clicked -= () => { if (!isTransitioning) GameEvents.OnSubmit?.Invoke(); };
            btn.clicked += () => { if (!isTransitioning) GameEvents.OnSubmit?.Invoke(); };
            btn.RegisterCallback<PointerEnterEvent>(evt => { if (!isTransitioning) GameEvents.OnHover?.Invoke(); });
        }
    }
}