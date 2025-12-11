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
        private VisualElement menuContainer;
        private VisualElement quizContainer;
        private VisualElement resultContainer;

        private UIDocument uiDocument;
        private Label titleLabel;
        private Button startButton;
        private Button restartButton;

        private void Awake()
        {
            // TryGetComponent assigns 'uiDocument' automatically if found
            if (!TryGetComponent<UIDocument>(out uiDocument))
            {
                Debug.LogError("UIDocument component is missing on GameController GameObject.");
            }
        }

        private void OnEnable()
        {
            if (uiDocument == null) // Safety check
            {
                Debug.LogError("UIDocument is not assigned. Cannot initialize UI.");
                return;
            }

            // 1. Grab the Root
            root = uiDocument.rootVisualElement;

            // 2. Find Containers
            menuContainer = root.Q<VisualElement>("Container_Menu");
            quizContainer = root.Q<VisualElement>("Container_Quiz");
            resultContainer = root.Q<VisualElement>("Container_Result");

            // 3. Find Buttons & Labels
            startButton = root.Q<Button>("Btn_Start");
            restartButton = root.Q<Button>("Btn_Restart");
            titleLabel = root.Q<Label>("Lbl_Title");

            // 4. Hook up Buttons
            // Unregister first to prevent double-clicking issues if OnEnable runs twice
            startButton.clicked -= StartGame;
            startButton.clicked += StartGame;

            restartButton.clicked -= RestartGame;
            restartButton.clicked += RestartGame;

            ShowMenu();
        }

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
            Debug.Log("Game Started!");
            menuContainer.style.display = DisplayStyle.None;
            quizContainer.style.display = DisplayStyle.Flex;
        }

        private void RestartGame()
        {
            Debug.Log("Restarting...");
            resultContainer.style.display = DisplayStyle.None;
            ShowMenu();
        }
    }
}