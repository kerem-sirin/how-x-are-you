using UnityEngine;
using UnityEngine.UIElements;

namespace HowX.Core
{
    /// <summary>
    /// Monitors screen resolution and fires events when aspect ratio changes.
    /// Works in both Editor and Play mode for live preview.
    /// Attach to the same GameObject as your UIDocument.
    /// </summary>
    [ExecuteAlways]
    public class MediaQuery : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private UIDocument document;

        [Header("Settings")]
        [Tooltip("Minimum aspect ratio (width/height) to be considered Landscape. Default 1.2 handles near-square screens.")]
        [SerializeField] private float landscapeThreshold = 1.2f;

        // Current state
        private Vector2 currentResolution;
        private MediaAspectRatio currentAspectRatio = MediaAspectRatio.Undefined;

        // Public accessors
        public Vector2 CurrentResolution => currentResolution;
        public MediaAspectRatio CurrentAspectRatio => currentAspectRatio;

        private void OnEnable()
        {
            // Try to get UIDocument if not assigned
            if (document == null)
                TryGetComponent(out document);

            if (document == null)
            {
                Debug.LogWarning("[MediaQuery] UIDocument not assigned.");
                return;
            }

            // Register for geometry changes (fires when UI resizes)
            VisualElement root = document.rootVisualElement;
            if (root != null)
                root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            // Initial query
            QueryResolution();
        }

        private void OnDisable()
        {
            if (document == null) return;

            VisualElement root = document.rootVisualElement;
            if (root != null)
                root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void Start()
        {
            QueryResolution();
        }

        /// <summary>
        /// Called when the UI root element's geometry changes.
        /// </summary>
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            QueryResolution();
        }

        /// <summary>
        /// Check current resolution and fire events if changed.
        /// </summary>
        public void QueryResolution()
        {
            Vector2 newResolution = new Vector2(Screen.width, Screen.height);

            // Fire resolution event if dimensions changed
            if (newResolution != currentResolution)
            {
                currentResolution = newResolution;
                MediaQueryEvents.ResolutionUpdated?.Invoke(newResolution);
            }

            // Calculate and fire aspect ratio event if category changed
            MediaAspectRatio newAspectRatio = CalculateAspectRatio(newResolution);

            if (newAspectRatio != currentAspectRatio)
            {
                currentAspectRatio = newAspectRatio;
                MediaQueryEvents.AspectRatioUpdated?.Invoke(newAspectRatio);

                #if UNITY_EDITOR
                Debug.Log($"[MediaQuery] Aspect ratio changed: {newAspectRatio} ({newResolution.x}x{newResolution.y})");
                #endif
            }
        }

        /// <summary>
        /// Force update resolution and aspect ratio (useful after scene loads).
        /// </summary>
        public void ForceUpdate()
        {
            Vector2 newResolution = new Vector2(Screen.width, Screen.height);
            currentResolution = newResolution;
            currentAspectRatio = CalculateAspectRatio(newResolution);

            MediaQueryEvents.ResolutionUpdated?.Invoke(currentResolution);
            MediaQueryEvents.AspectRatioUpdated?.Invoke(currentAspectRatio);
        }

        /// <summary>
        /// Determine if resolution is Landscape or Portrait based on threshold.
        /// </summary>
        private MediaAspectRatio CalculateAspectRatio(Vector2 resolution)
        {
            // Guard against division by zero
            if (resolution.y < float.Epsilon)
            {
                Debug.LogWarning("[MediaQuery] Height is zero, cannot calculate aspect ratio.");
                return MediaAspectRatio.Undefined;
            }

            float aspectRatio = resolution.x / resolution.y;

            // Landscape if width/height >= threshold (default 1.2)
            if (aspectRatio >= landscapeThreshold)
                return MediaAspectRatio.Landscape;
            else
                return MediaAspectRatio.Portrait;
        }
    }
}
