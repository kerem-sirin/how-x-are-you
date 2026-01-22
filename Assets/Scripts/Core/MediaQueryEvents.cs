using System;
using UnityEngine;

namespace HowX.Core
{
    /// <summary>
    /// Aspect ratio categories for responsive UI layouts.
    /// </summary>
    public enum MediaAspectRatio
    {
        Undefined,
        Landscape,
        Portrait
    }

    /// <summary>
    /// Static events for screen resolution and aspect ratio changes.
    /// Subscribe to these from any UI component that needs to respond to orientation changes.
    /// </summary>
    public static class MediaQueryEvents
    {
        /// <summary>
        /// Fired when screen dimensions change. Provides new resolution as Vector2(width, height).
        /// </summary>
        public static Action<Vector2> ResolutionUpdated;

        /// <summary>
        /// Fired when aspect ratio category changes (Portrait <-> Landscape).
        /// This is the primary event for responsive layout switching.
        /// </summary>
        public static Action<MediaAspectRatio> AspectRatioUpdated;
    }
}
