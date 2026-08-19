using UnityEngine;

namespace Multiformatris.Infrastructure.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource MusicSource;
        public AudioSource SFXSource;

        [Header("Clips")]
        public AudioClip MoveSFX;
        public AudioClip RotateSFX;
        public AudioClip DropSFX;
        public AudioClip LockSFX;
        public AudioClip ClearSFX;
        public AudioClip LevelUpSFX;
        public AudioClip GameOverSFX;

        [Header("Settings")]
        public float MusicVolume = 0.5f;
        public float SFXVolume = 0.7f;

        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (MusicSource == null || clip == null) return;

            MusicSource.clip = clip;
            MusicSource.volume = MusicVolume;
            MusicSource.loop = true;
            MusicSource.Play();
        }

        public void StopMusic()
        {
            if (MusicSource != null)
                MusicSource.Stop();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (SFXSource == null || clip == null) return;

            SFXSource.PlayOneShot(clip, SFXVolume);
        }

        public void PlayMove() => PlaySFX(MoveSFX);
        public void PlayRotate() => PlaySFX(RotateSFX);
        public void PlayDrop() => PlaySFX(DropSFX);
        public void PlayLock() => PlaySFX(LockSFX);
        public void PlayClear() => PlaySFX(ClearSFX);
        public void PlayLevelUp() => PlaySFX(LevelUpSFX);
        public void PlayGameOver() => PlaySFX(GameOverSFX);

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            if (MusicSource != null)
                MusicSource.volume = MusicVolume;
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
        }
    }
}
