using UnityEngine;
using UnityEngine.UIElements;

namespace HowX.Core
{
    /// <summary>
    /// Swaps PanelSettings and StyleSheets based on screen orientation.
    /// Works in both Editor and Play mode for live preview.
    /// 
    /// Based on Unity's ThemeManager pattern from UI Toolkit Demo.
    /// </summary>
    [ExecuteAlways]
    public class ResponsiveUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private UIDocument document;

        [Header("Portrait Settings (Height > Width)")]
        [Tooltip("PanelSettings with portrait reference resolution (e.g., 1080x1920)")]
        [SerializeField] private PanelSettings portraitPanelSettings;

        [Header("Landscape Settings (Width > Height)")]
        [Tooltip("PanelSettings with landscape reference resolution (e.g., 1920x1080)")]
        [SerializeField] private PanelSettings landscapePanelSettings;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Track current state
        private MediaAspectRatio currentAspectRatio = MediaAspectRatio.Undefined;

        private void OnEnable()
        {
            // Try to get UIDocument if not assigned
            if (document == null)
                TryGetComponent(out document);

            if (document == null)
            {
                Debug.LogWarning("[ResponsiveUI] UIDocument not assigned.");
                return;
            }

            // Subscribe to aspect ratio changes
            MediaQueryEvents.AspectRatioUpdated += OnAspectRatioUpdated;

            // Apply initial settings based on current screen size
            ApplyInitialSettings();
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            MediaQueryEvents.AspectRatioUpdated -= OnAspectRatioUpdated;
        }

        /// <summary>
        /// Apply settings based on current screen dimensions.
        /// </summary>
        private void ApplyInitialSettings()
        {
            if (document == null) return;

            // Calculate current aspect ratio
            Vector2 resolution = new Vector2(Screen.width, Screen.height);
            MediaAspectRatio initialRatio = MediaAspectRatio.Portrait;

            if (resolution.y > float.Epsilon)
            {
                float aspect = resolution.x / resolution.y;
                initialRatio = aspect >= 1.2f ? MediaAspectRatio.Landscape : MediaAspectRatio.Portrait;
            }

            // Apply the appropriate settings
            ApplySettings(initialRatio);
        }

        /// <summary>
        /// Event handler for aspect ratio changes from MediaQuery.
        /// </summary>
        private void OnAspectRatioUpdated(MediaAspectRatio newAspectRatio)
        {
            if (newAspectRatio == MediaAspectRatio.Undefined) return;
            if (newAspectRatio == currentAspectRatio) return;

            ApplySettings(newAspectRatio);
        }

        /// <summary>
        /// Apply PanelSettings and StyleSheet for the given aspect ratio.
        /// </summary>
        private void ApplySettings(MediaAspectRatio aspectRatio)
        {
            if (document == null) return;

            // 1. Swap PanelSettings (includes reference resolution)
            ApplyPanelSettings(aspectRatio);

            currentAspectRatio = aspectRatio;

            if (debugMode)
            {
                Debug.Log($"[ResponsiveUI] Applied {aspectRatio} settings");
            }
        }

        /// <summary>
        /// Swap the PanelSettings on the UIDocument.
        /// </summary>
        private void ApplyPanelSettings(MediaAspectRatio aspectRatio)
        {
            PanelSettings targetSettings = null;

            if (aspectRatio == MediaAspectRatio.Landscape && landscapePanelSettings != null)
            {
                targetSettings = landscapePanelSettings;
            }
            else if (aspectRatio == MediaAspectRatio.Portrait && portraitPanelSettings != null)
            {
                targetSettings = portraitPanelSettings;
            }

            if (targetSettings != null && document.panelSettings != targetSettings)
            {
                document.panelSettings = targetSettings;

                if (debugMode)
                {
                    Debug.Log($"[ResponsiveUI] Applied PanelSettings: {targetSettings.name}");
                }
            }
        }

        /// <summary>
        /// Force refresh settings (useful for testing in Editor).
        /// </summary>
        [ContextMenu("Force Refresh")]
        public void ForceRefresh()
        {
            currentAspectRatio = MediaAspectRatio.Undefined;
            ApplyInitialSettings();
        }

        /// <summary>
        /// Get current aspect ratio state.
        /// </summary>
        public MediaAspectRatio CurrentAspectRatio => currentAspectRatio;
    }
}