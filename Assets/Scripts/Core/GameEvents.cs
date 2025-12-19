using System;

namespace HowX.Core
{
    public static class GameEvents
    {
        // UI Interaction Events
        public static Action OnSubmit;       // Button Click
        public static Action OnHover;        // Button Hover

        // Game Flow Events
        public static Action OnGameStart;    // Start Button Clicked
        public static Action OnGameRestart;  // Restart Button Clicked (Back to Menu)
        public static Action OnGameEnd;      // Quiz Finished (Results shown)
    }
}