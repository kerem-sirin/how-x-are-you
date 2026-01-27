using UnityEngine;
using System.Collections;

namespace HowX.Core
{
    public class AudioController : MonoBehaviour
    {
        private const string PREF_MUSIC_MUTED = "HowX_MusicMuted";
        private const string PREF_SFX_MUTED = "HowX_SFXMuted";

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Clips")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip quizMusic;
        [SerializeField] private AudioClip resultMusic;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip whooshSound;
        [SerializeField] private AudioClip correctSound;
        [SerializeField] private AudioClip wrongSound;

        // Store default values for reset
        private float defaultPitch = 1f;
        private float defaultVolume = 1f;

        // Mute state
        private bool isMusicMuted = false;
        private bool isSFXMuted = false;

        // Public accessors
        public bool IsMusicMuted => isMusicMuted;
        public bool IsSFXMuted => isSFXMuted;

        // Singleton for easy access from SettingsController
        public static AudioController Instance { get; private set; }

        private void Awake()
        {
            // Simple singleton
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            // Cache default values from the AudioSource
            if (sfxSource != null)
            {
                defaultPitch = sfxSource.pitch;
                defaultVolume = sfxSource.volume;
            }

            // Load saved mute preferences
            LoadMutePreferences();
        }

        private void OnEnable()
        {
            // Subscribe to events
            GameEvents.OnSubmit += PlayClick;
            GameEvents.OnHover += PlayHover;
            GameEvents.OnNextQuestion += PlayWhoosh;
            GameEvents.OnAnswerCorrect += PlayCorrect;
            GameEvents.OnAnswerWrong += PlayWrong;

            GameEvents.OnGameStart += PlayQuizMusic;
            GameEvents.OnGameRestart += PlayMenuMusic;
            GameEvents.OnGameEnd += PlayGameEnd;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent errors when scene changes
            GameEvents.OnSubmit -= PlayClick;
            GameEvents.OnHover -= PlayHover;
            GameEvents.OnNextQuestion -= PlayWhoosh;
            GameEvents.OnAnswerCorrect -= PlayCorrect;
            GameEvents.OnAnswerWrong -= PlayWrong;

            GameEvents.OnGameStart -= PlayQuizMusic;
            GameEvents.OnGameRestart -= PlayMenuMusic;
            GameEvents.OnGameEnd -= PlayGameEnd;
        }

        private void Start()
        {
            // Apply mute states to audio sources
            ApplyMuteStates();

            // Play menu music automatically on start
            PlayMenuMusic();
        }

        // --- Mute Controls ---

        /// <summary>
        /// Toggle music mute state.
        /// </summary>
        public void ToggleMusic()
        {
            isMusicMuted = !isMusicMuted;
            ApplyMusicMute();
            SaveMutePreferences();
        }

        /// <summary>
        /// Toggle SFX mute state.
        /// </summary>
        public void ToggleSFX()
        {
            isSFXMuted = !isSFXMuted;
            ApplySFXMute();
            SaveMutePreferences();
        }

        /// <summary>
        /// Set music mute state directly.
        /// </summary>
        public void SetMusicMuted(bool muted)
        {
            if (isMusicMuted == muted) return;
            isMusicMuted = muted;
            ApplyMusicMute();
            SaveMutePreferences();
        }

        /// <summary>
        /// Set SFX mute state directly.
        /// </summary>
        public void SetSFXMuted(bool muted)
        {
            if (isSFXMuted == muted) return;
            isSFXMuted = muted;
            ApplySFXMute();
            SaveMutePreferences();
        }

        private void ApplyMuteStates()
        {
            ApplyMusicMute();
            ApplySFXMute();
        }

        private void ApplyMusicMute()
        {
            if (musicSource != null)
                musicSource.mute = isMusicMuted;
        }

        private void ApplySFXMute()
        {
            if (sfxSource != null)
                sfxSource.mute = isSFXMuted;
        }

        private void LoadMutePreferences()
        {
            isMusicMuted = PlayerPrefs.GetInt(PREF_MUSIC_MUTED, 0) == 1;
            isSFXMuted = PlayerPrefs.GetInt(PREF_SFX_MUTED, 0) == 1;
        }

        private void SaveMutePreferences()
        {
            PlayerPrefs.SetInt(PREF_MUSIC_MUTED, isMusicMuted ? 1 : 0);
            PlayerPrefs.SetInt(PREF_SFX_MUTED, isSFXMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        // --- Event Handlers ---
        private void PlayMenuMusic() => PlayMusic(menuMusic);
        private void PlayQuizMusic() => PlayMusic(quizMusic);
        private void PlayGameEnd() => PlayMusic(resultMusic);

        private void PlayClick() => PlayRandomizedSFX(clickSound);
        private void PlayHover() => PlayRandomizedSFX(hoverSound);
        private void PlayWhoosh() => PlaySFX(whooshSound);
        private void PlayCorrect() => PlaySFX(correctSound);
        private void PlayWrong() => PlaySFX(wrongSound);

        // --- Internal Helpers ---
        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null && !isSFXMuted)
            {
                // Ensure default pitch/volume for non-randomized SFX
                sfxSource.pitch = defaultPitch;
                sfxSource.volume = defaultVolume;
                sfxSource.PlayOneShot(clip);
            }
        }

        private void PlayRandomizedSFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null && !isSFXMuted)
            {
                // Apply random variation
                sfxSource.pitch = Random.Range(0.9f, 1.1f);
                sfxSource.volume = Random.Range(0.9f, 1.0f);
                sfxSource.PlayOneShot(clip);

                // Reset after clip finishes (coroutine approach)
                // Note: PlayOneShot doesn't block, so we schedule reset
                StartCoroutine(ResetAudioSourceAfterDelay(clip.length));
            }
        }

        private IEnumerator ResetAudioSourceAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetSFXSource();
        }

        private void ResetSFXSource()
        {
            if (sfxSource != null)
            {
                sfxSource.pitch = defaultPitch;
                sfxSource.volume = defaultVolume;
            }
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.Stop();
            musicSource.clip = clip;

            if (clip != null)
            {
                musicSource.loop = true;
                musicSource.Play();
            }
        }
    }
}