using UnityEngine;

namespace HowX.Core
{
    public class AudioController : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Clips")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip quizMusic;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip resultSound;
        [SerializeField] private AudioClip whooshSound;

        private void OnEnable()
        {
            // Subscribe to events
            GameEvents.OnSubmit += PlayClick;
            GameEvents.OnHover += PlayHover;
            GameEvents.OnNextQuestion += PlayWhoosh;

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

            GameEvents.OnGameStart -= PlayQuizMusic;
            GameEvents.OnGameRestart -= PlayMenuMusic;
            GameEvents.OnGameEnd -= PlayGameEnd;
        }

        private void Start()
        {
            // Play menu music automatically on start
            PlayMenuMusic();
        }

        // --- Event Handlers ---
        private void PlayMenuMusic() => PlayMusic(menuMusic);
        private void PlayQuizMusic() => PlayMusic(quizMusic);
        private void PlayGameEnd()
        {
            StopMusic();
            PlaySFX(resultSound);
        }
        private void PlayClick() => PlayRandomizedSFX(clickSound);
        private void PlayHover() => PlayRandomizedSFX(hoverSound);
        private void PlayWhoosh() => PlaySFX(whooshSound);

        // --- Internal Helpers ---
        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
        }

        private void PlayRandomizedSFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.pitch = Random.Range(0.9f, 1.1f);
                sfxSource.volume = Random.Range(0.9f, 1.0f);
                sfxSource.PlayOneShot(clip);
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

        private void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }
    }
}