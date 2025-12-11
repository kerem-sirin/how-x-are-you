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
        private VisualElement gridAnswers; // Parent of buttons (optional, good for hiding)
        private Button[] answerButtons; // Array to hold our 4 buttons

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

            // 1. Find Containers
            menuContainer = root.Q<VisualElement>("Container_Menu");
            quizContainer = root.Q<VisualElement>("Container_Quiz");
            resultContainer = root.Q<VisualElement>("Container_Result");

            // 2. Find Menu Elements
            titleLabel = root.Q<Label>("Lbl_Title");
            startButton = root.Q<Button>("Btn_Start");
            langButton = root.Q<Button>("Btn_LangToggle"); // Make sure this ID exists in UXML!

            // 3. Find Quiz Elements
            questionLabel = root.Q<Label>("Lbl_QuestionText");
            questionImage = root.Q<Image>("Img_QuestionVisual");

            // 4. Find Answer Buttons (Btn_Ans0 to Btn_Ans3)
            answerButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                answerButtons[i] = root.Q<Button>($"Btn_Ans{i}");

                // CRITICAL: Assign click event ONCE. 
                // We capture the index 'x' to avoid closure issues.
                int x = i;
                answerButtons[i].clicked -= () => OnAnswerClicked(x); // Safety clean
                answerButtons[i].clicked += () => OnAnswerClicked(x);
            }

            // 5. Hook up General Buttons
            startButton.clicked -= StartGame;
            startButton.clicked += StartGame;

            langButton.clicked -= ToggleLanguage;
            langButton.clicked += ToggleLanguage;

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

        private void LoadQuestion()
        {
            // Debug Check 1: Is the Profile or Question list valid?
            if (currentProfile == null) { Debug.LogError("Profile is NULL"); return; }
            if (currentProfile.questions == null) { Debug.LogError("Questions List is NULL"); return; }

            if (currentQuestionIndex >= currentProfile.questions.Count)
            {
                EndGame();
                return;
            }

            QuestionData q = currentProfile.questions[currentQuestionIndex];

            // Debug Check 2: Is the specific Question Asset missing?
            if (q == null)
            {
                Debug.LogError($"Question at index {currentQuestionIndex} is NULL! Check the Profile list.");
                return;
            }

            // Debug Check 3: Did we fail to find the Label?
            if (questionLabel == null)
            {
                Debug.LogError("UI Error: Could not find 'Lbl_QuestionText'. Check UXML Names.");
                return;
            }

            questionLabel.text = isNativeLanguage ? q.text_Native : q.text_EN;

            // Debug Check 4: Did we fail to find the Image?
            if (questionImage == null)
            {
                // Warning only, we might not crash if we don't use it, but good to know
                Debug.LogWarning("UI Warning: Could not find 'Img_QuestionVisual'.");
            }
            else
            {
                // Only try to use it if it exists
                if (q.visual != null)
                {
                    questionImage.style.display = DisplayStyle.Flex;
                    questionImage.sprite = q.visual;
                }
                else
                {
                    questionImage.style.display = DisplayStyle.None;
                }
            }

            // Debug Check 5: Buttons
            for (int i = 0; i < 4; i++)
            {
                Button btn = answerButtons[i];

                // Safety: Does the button exist?
                if (btn == null) continue;

                // 1. Should we show this answer?
                if (i < q.answers.Length)
                {
                    btn.style.display = DisplayStyle.Flex;

                    // Find the children inside THIS specific button
                    Label txtChild = btn.Q<Label>("Lbl_Ans");
                    Image imgChild = btn.Q<Image>("Img_Ans");

                    // 2. Image Mode vs Text Mode
                    if (q.useImageAnswers)
                    {
                        // IMAGE MODE
                        if (txtChild != null) txtChild.style.display = DisplayStyle.None;

                        if (imgChild != null)
                        {
                            imgChild.style.display = DisplayStyle.Flex;
                            imgChild.sprite = q.answers[i].image;
                        }
                    }
                    else
                    {
                        // TEXT MODE
                        if (imgChild != null) imgChild.style.display = DisplayStyle.None;

                        if (txtChild != null)
                        {
                            txtChild.style.display = DisplayStyle.Flex;
                            // Localization check
                            txtChild.text = isNativeLanguage ? q.answers[i].text_Native : q.answers[i].text_EN;
                        }
                    }
                }
                else
                {
                    // No answer for this slot, hide the whole button
                    btn.style.display = DisplayStyle.None;
                }
            }
        }

        private void OnAnswerClicked(int index)
        {
            // Safety Check
            if (currentQuestionIndex >= currentProfile.questions.Count) return;

            // Check correctness
            bool correct = currentProfile.questions[currentQuestionIndex].answers[index].isCorrect;

            if (correct)
            {
                currentScore++;
                Debug.Log("Correct!");
            }
            else
            {
                Debug.Log("Wrong!");
            }

            // Move to next
            currentQuestionIndex++;
            LoadQuestion();
        }

        private void ToggleLanguage()
        {
            isNativeLanguage = !isNativeLanguage;

            // Update button text
            langButton.text = isNativeLanguage ? "Native" : "English";

            // If we are mid-game, refresh the current question immediately
            if (quizContainer.style.display == DisplayStyle.Flex)
            {
                LoadQuestion();
            }
        }

        private void EndGame()
        {
            Debug.Log($"Game Over! Score: {currentScore}");
            quizContainer.style.display = DisplayStyle.None;
            resultContainer.style.display = DisplayStyle.Flex;
        }
    }
}