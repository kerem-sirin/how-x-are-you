using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace HowX.Core
{
    /// <summary>
    /// Controls the Settings Panel UI.
    /// Handles mute toggles, return to menu, and confirmation dialog.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Icons")]
        [SerializeField] private Texture2D iconMusicOn;
        [SerializeField] private Texture2D iconMusicOff;
        [SerializeField] private Texture2D iconSFXOn;
        [SerializeField] private Texture2D iconSFXOff;

        #endregion

        #region UI References

        private GameController gameController;
        private Label lblConfirmText;
        private UIDocument uiDocument;
        private VisualElement root;

        // Settings Panel
        private Button btnSettings;
        private VisualElement panelSettings;
        private VisualElement settingsOverlay;
        private VisualElement boxSettings;
        private Button btnToggleMusic;
        private Button btnToggleSFX;
        private Button btnCloseSettings;
        private Button btnReturnToMenu;

        // Confirmation Dialog
        private VisualElement panelConfirm;
        private VisualElement confirmOverlay;
        private Button btnConfirmYes;
        private Button btnConfirmNo;

        // Game Containers
        private VisualElement quizContainer;
        private VisualElement menuContainer;
        private VisualElement resultContainer;

        #endregion

        #region State

        private bool isSettingsOpen = false;
        private bool isConfirmOpen = false;

        #endregion

        #region Callback Storage

        private Action onSettingsClick;
        private Action onMusicToggleClick;
        private Action onSFXToggleClick;
        private Action onCloseClick;
        private Action onReturnToMenuClick;
        private Action onConfirmYesClick;
        private Action onConfirmNoClick;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            gameController = GetComponent<GameController>();
            uiDocument = GetComponentInChildren<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument missing in children!");
            }
        }

        private void OnEnable()
        {
            if (uiDocument == null) return;

            root = uiDocument.rootVisualElement;
            if (root == null) return;

            FindElements();
            UpdateMuteIcons();
            SetupCallbacks();
            InitializeState();
        }

        private void OnDisable()
        {
            CleanupCallbacks();
        }

        #endregion

        #region UI Setup

        private void FindElements()
        {
            // Settings button
            btnSettings = root.Q<Button>("Btn_Settings");

            // Settings panel
            panelSettings = root.Q<VisualElement>("Panel_Settings");
            settingsOverlay = root.Q<VisualElement>("Settings_Overlay");
            boxSettings = root.Q<VisualElement>("Box_Settings");
            btnToggleMusic = root.Q<Button>("Btn_ToggleMusic");
            btnToggleSFX = root.Q<Button>("Btn_ToggleSFX");
            btnCloseSettings = root.Q<Button>("Btn_CloseSettings");
            btnReturnToMenu = root.Q<Button>("Btn_ReturnToMenu");

            // Confirmation panel
            panelConfirm = root.Q<VisualElement>("Panel_Confirm");
            confirmOverlay = root.Q<VisualElement>("Confirm_Overlay");
            btnConfirmYes = root.Q<Button>("Btn_ConfirmYes");
            btnConfirmNo = root.Q<Button>("Btn_ConfirmNo");
            lblConfirmText = root.Q<Label>("Lbl_ConfirmText");

            // Game containers
            quizContainer = root.Q<VisualElement>("Container_Quiz");
            menuContainer = root.Q<VisualElement>("Container_Menu");
            resultContainer = root.Q<VisualElement>("Container_Result");
        }

        private void SetupCallbacks()
        {
            // Settings button
            if (btnSettings != null)
            {
                onSettingsClick = OpenSettings;
                btnSettings.clicked += onSettingsClick;
            }

            // Music toggle
            if (btnToggleMusic != null)
            {
                onMusicToggleClick = OnMusicToggle;
                btnToggleMusic.clicked += onMusicToggleClick;
            }

            // SFX toggle
            if (btnToggleSFX != null)
            {
                onSFXToggleClick = OnSFXToggle;
                btnToggleSFX.clicked += onSFXToggleClick;
            }

            // Close button
            if (btnCloseSettings != null)
            {
                onCloseClick = CloseSettings;
                btnCloseSettings.clicked += onCloseClick;
            }

            // Overlay click to close
            if (settingsOverlay != null)
            {
                settingsOverlay.RegisterCallback<ClickEvent>(OnOverlayClicked);
            }

            // Return to menu
            if (btnReturnToMenu != null)
            {
                onReturnToMenuClick = OnReturnToMenuClicked;
                btnReturnToMenu.clicked += onReturnToMenuClick;
            }

            // Confirmation buttons
            if (btnConfirmYes != null)
            {
                onConfirmYesClick = OnConfirmYes;
                btnConfirmYes.clicked += onConfirmYesClick;
            }

            if (btnConfirmNo != null)
            {
                onConfirmNoClick = OnConfirmNo;
                btnConfirmNo.clicked += onConfirmNoClick;
            }

            if (confirmOverlay != null)
            {
                confirmOverlay.RegisterCallback<ClickEvent>(OnConfirmOverlayClicked);
            }
        }

        private void CleanupCallbacks()
        {
            if (btnSettings != null && onSettingsClick != null)
                btnSettings.clicked -= onSettingsClick;

            if (btnToggleMusic != null && onMusicToggleClick != null)
                btnToggleMusic.clicked -= onMusicToggleClick;

            if (btnToggleSFX != null && onSFXToggleClick != null)
                btnToggleSFX.clicked -= onSFXToggleClick;

            if (btnCloseSettings != null && onCloseClick != null)
                btnCloseSettings.clicked -= onCloseClick;

            if (settingsOverlay != null)
                settingsOverlay.UnregisterCallback<ClickEvent>(OnOverlayClicked);

            if (btnReturnToMenu != null && onReturnToMenuClick != null)
                btnReturnToMenu.clicked -= onReturnToMenuClick;

            if (btnConfirmYes != null && onConfirmYesClick != null)
                btnConfirmYes.clicked -= onConfirmYesClick;

            if (btnConfirmNo != null && onConfirmNoClick != null)
                btnConfirmNo.clicked -= onConfirmNoClick;

            if (confirmOverlay != null)
                confirmOverlay.UnregisterCallback<ClickEvent>(OnConfirmOverlayClicked);
        }

        private void InitializeState()
        {
            // Hide settings panel
            if (panelSettings != null)
            {
                panelSettings.RemoveFromClassList("panel-settings-visible");
                panelSettings.AddToClassList("panel-settings-hidden");
            }

            // Hide confirmation panel
            if (panelConfirm != null)
            {
                panelConfirm.RemoveFromClassList("panel-confirm-visible");
                panelConfirm.AddToClassList("panel-confirm-hidden");
            }

            isSettingsOpen = false;
            isConfirmOpen = false;
        }

        #endregion

        #region Settings Panel

        private void OpenSettings()
        {
            if (isSettingsOpen) return;

            isSettingsOpen = true;

            UpdateReturnToMenuVisibility();
            UpdateMuteIcons();

            if (panelSettings != null)
            {
                panelSettings.RemoveFromClassList("panel-settings-hidden");
                panelSettings.AddToClassList("panel-settings-visible");
            }

            // Disable quiz interaction
            if (quizContainer != null && quizContainer.style.display == DisplayStyle.Flex)
            {
                quizContainer.AddToClassList("quiz-disabled");
            }
        }

        private void CloseSettings()
        {
            if (!isSettingsOpen) return;

            isSettingsOpen = false;

            if (panelSettings != null)
            {
                panelSettings.RemoveFromClassList("panel-settings-visible");
                panelSettings.AddToClassList("panel-settings-hidden");
            }

            // Re-enable quiz interaction
            if (quizContainer != null)
            {
                quizContainer.RemoveFromClassList("quiz-disabled");
            }
        }

        private void OnOverlayClicked(ClickEvent evt)
        {
            if (evt.target == settingsOverlay)
            {
                CloseSettings();
            }
        }

        private void UpdateReturnToMenuVisibility()
        {
            if (btnReturnToMenu == null) return;

            bool inQuiz = quizContainer != null && quizContainer.style.display == DisplayStyle.Flex;

            if (inQuiz)
            {
                btnReturnToMenu.RemoveFromClassList("btn-return-menu-hidden");
                btnReturnToMenu.style.display = DisplayStyle.Flex;
            }
            else
            {
                btnReturnToMenu.AddToClassList("btn-return-menu-hidden");
                btnReturnToMenu.style.display = DisplayStyle.None;
            }
        }

        #endregion

        #region Audio Toggles

        private void OnMusicToggle()
        {
            if (AudioController.Instance != null)
            {
                AudioController.Instance.ToggleMusic();
                UpdateMuteIcons();
            }
        }

        private void OnSFXToggle()
        {
            if (AudioController.Instance != null)
            {
                AudioController.Instance.ToggleSFX();
                UpdateMuteIcons();
            }
        }

        private void UpdateMuteIcons()
        {
            if (AudioController.Instance == null) return;

            // Music icon
            if (btnToggleMusic != null)
            {
                bool musicMuted = AudioController.Instance.IsMusicMuted;
                btnToggleMusic.style.backgroundImage = musicMuted ? iconMusicOff : iconMusicOn;

                if (musicMuted)
                    btnToggleMusic.AddToClassList("muted");
                else
                    btnToggleMusic.RemoveFromClassList("muted");
            }

            // SFX icon
            if (btnToggleSFX != null)
            {
                bool sfxMuted = AudioController.Instance.IsSFXMuted;
                btnToggleSFX.style.backgroundImage = sfxMuted ? iconSFXOff : iconSFXOn;

                if (sfxMuted)
                    btnToggleSFX.AddToClassList("muted");
                else
                    btnToggleSFX.RemoveFromClassList("muted");
            }
        }

        #endregion

        #region Confirmation Dialog

        private void OnReturnToMenuClicked()
        {
            ShowConfirmDialog();
        }

        private void ShowConfirmDialog()
        {
            if (isConfirmOpen) return;
            UpdateConfirmText();

            isConfirmOpen = true;

            if (panelConfirm != null)
            {
                panelConfirm.RemoveFromClassList("panel-confirm-hidden");
                panelConfirm.AddToClassList("panel-confirm-visible");
            }
        }

        private void HideConfirmDialog()
        {
            if (!isConfirmOpen) return;

            isConfirmOpen = false;

            if (panelConfirm != null)
            {
                panelConfirm.RemoveFromClassList("panel-confirm-visible");
                panelConfirm.AddToClassList("panel-confirm-hidden");
            }
        }

        private void OnConfirmYes()
        {
            HideConfirmDialog();
            CloseSettings();
            GameEvents.OnGameRestart?.Invoke();
        }

        private void OnConfirmNo()
        {
            HideConfirmDialog();
        }

        private void OnConfirmOverlayClicked(ClickEvent evt)
        {
            if (evt.target == confirmOverlay)
            {
                HideConfirmDialog();
            }
        }

        private void UpdateConfirmText()
        {
            if (gameController?.CurrentProfile?.uiData == null) return;

            var ui = gameController.CurrentProfile.uiData;
            bool native = gameController.IsNativeLanguage;

            if (lblConfirmText != null)
                lblConfirmText.text = ui.confirmTitle?.Get(native);

            if (btnConfirmYes != null)
                btnConfirmYes.text = ui.btnYes?.Get(native);

            if (btnConfirmNo != null)
                btnConfirmNo.text = ui.btnNo?.Get(native);
        }

        #endregion
    }
}
