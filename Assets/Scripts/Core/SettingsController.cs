using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace HowX.Core
{
    /// <summary>
    /// Controls the Settings Panel UI.
    /// Handles mute toggles, language toggle requests, return to menu, and confirmation dialog.
    /// Talks to other systems only through GameEvents: it raises requests and reacts to
    /// state events. Localized text (language button, confirm dialog) is set by GameController.
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

        private UIDocument uiDocument;
        private VisualElement root;

        // Settings Panel
        private Button btnSettings;
        private VisualElement panelSettings;
        private VisualElement settingsOverlay;
        private VisualElement boxSettings;
        private Button btnToggleMusic;
        private Button btnToggleSFX;
        private Button btnToggleLanguage;
        private Button btnCloseSettings;
        private Button btnReturnToMenu;

        // Confirmation Dialog
        private VisualElement panelConfirm;
        private VisualElement confirmOverlay;
        private Button btnConfirmYes;
        private Button btnConfirmNo;

        #endregion

        #region State

        // Which screen the game is on, tracked from GameEvents
        private enum GameScreen { Menu, Quiz, Results }
        private GameScreen currentScreen = GameScreen.Menu;

        // Last mute state reported by AudioController
        private bool isMusicMuted = false;
        private bool isSFXMuted = false;

        private bool isSettingsOpen = false;
        private bool isConfirmOpen = false;

        #endregion

        #region Callback Storage

        private Action onSettingsClick;
        private Action onMusicToggleClick;
        private Action onSFXToggleClick;
        private Action onLanguageToggleClick;
        private Action onCloseClick;
        private Action onReturnToMenuClick;
        private Action onConfirmYesClick;
        private Action onConfirmNoClick;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
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
            btnToggleLanguage = root.Q<Button>("Btn_ToggleLanguage");
            btnCloseSettings = root.Q<Button>("Btn_CloseSettings");
            btnReturnToMenu = root.Q<Button>("Btn_ReturnToMenu");

            // Confirmation panel
            panelConfirm = root.Q<VisualElement>("Panel_Confirm");
            confirmOverlay = root.Q<VisualElement>("Confirm_Overlay");
            btnConfirmYes = root.Q<Button>("Btn_ConfirmYes");
            btnConfirmNo = root.Q<Button>("Btn_ConfirmNo");
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

            // Language toggle
            if (btnToggleLanguage != null)
            {
                onLanguageToggleClick = OnLanguageToggle;
                btnToggleLanguage.clicked += onLanguageToggleClick;
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

            // Game state
            GameEvents.OnGameStart += OnQuizShown;
            GameEvents.OnGameEnd += OnResultsShown;
            GameEvents.OnGameRestart += OnMenuShown;
            GameEvents.OnMuteStateChanged += ApplyMuteState;
        }

        private void CleanupCallbacks()
        {
            if (btnSettings != null && onSettingsClick != null)
                btnSettings.clicked -= onSettingsClick;

            if (btnToggleMusic != null && onMusicToggleClick != null)
                btnToggleMusic.clicked -= onMusicToggleClick;

            if (btnToggleSFX != null && onSFXToggleClick != null)
                btnToggleSFX.clicked -= onSFXToggleClick;

            if (btnToggleLanguage != null && onLanguageToggleClick != null)
                btnToggleLanguage.clicked -= onLanguageToggleClick;

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

            GameEvents.OnGameStart -= OnQuizShown;
            GameEvents.OnGameEnd -= OnResultsShown;
            GameEvents.OnGameRestart -= OnMenuShown;
            GameEvents.OnMuteStateChanged -= ApplyMuteState;
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

        #region Game State

        private void OnMenuShown() => currentScreen = GameScreen.Menu;
        private void OnQuizShown() => currentScreen = GameScreen.Quiz;
        private void OnResultsShown() => currentScreen = GameScreen.Results;

        #endregion

        #region Settings Panel

        private void OpenSettings()
        {
            if (isSettingsOpen) return;

            isSettingsOpen = true;

            UpdateReturnToMenuVisibility();
            UpdateLanguageButtonVisibility();
            UpdateMuteIcons();

            if (panelSettings != null)
            {
                panelSettings.RemoveFromClassList("panel-settings-hidden");
                panelSettings.AddToClassList("panel-settings-visible");
            }

            GameEvents.OnSettingsOpened?.Invoke();
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

            GameEvents.OnSettingsClosed?.Invoke();
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

            // Only offered mid-quiz; menu and results have their own way back
            btnReturnToMenu.EnableInClassList("btn-return-menu-hidden", currentScreen != GameScreen.Quiz);
        }

        #endregion

        #region Audio Toggles

        private void OnMusicToggle()
        {
            GameEvents.OnMusicToggleRequested?.Invoke();
        }

        private void OnSFXToggle()
        {
            GameEvents.OnSFXToggleRequested?.Invoke();
        }

        private void ApplyMuteState(bool musicMuted, bool sfxMuted)
        {
            isMusicMuted = musicMuted;
            isSFXMuted = sfxMuted;
            UpdateMuteIcons();
        }

        private void UpdateMuteIcons()
        {
            // Music icon
            if (btnToggleMusic != null)
            {
                btnToggleMusic.style.backgroundImage = isMusicMuted ? iconMusicOff : iconMusicOn;

                if (isMusicMuted)
                    btnToggleMusic.AddToClassList("muted");
                else
                    btnToggleMusic.RemoveFromClassList("muted");
            }

            // SFX icon
            if (btnToggleSFX != null)
            {
                btnToggleSFX.style.backgroundImage = isSFXMuted ? iconSFXOff : iconSFXOn;

                if (isSFXMuted)
                    btnToggleSFX.AddToClassList("muted");
                else
                    btnToggleSFX.RemoveFromClassList("muted");
            }
        }

        #endregion

        #region Language Toggle

        private void UpdateLanguageButtonVisibility()
        {
            if (btnToggleLanguage == null) return;

            // The menu already has its own language button (Btn_LangToggle)
            btnToggleLanguage.EnableInClassList("btn-language-hidden", currentScreen == GameScreen.Menu);
        }

        private void OnLanguageToggle()
        {
            GameEvents.OnLanguageToggleRequested?.Invoke();
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

        #endregion
    }
}
