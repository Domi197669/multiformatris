using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Multiformatris.UI
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject SettingsPanel;

        [Header("Audio")]
        public Slider MusicSlider;
        public Slider SFXSlider;
        public TextMeshProUGUI MusicValueText;
        public TextMeshProUGUI SFXValueText;

        [Header("Gameplay")]
        public Toggle GhostPieceToggle;
        public Toggle VibrationToggle;

        [Header("Buttons")]
        public Button BackButton;
        public Button ResetHighScoreButton;

        private Infrastructure.Audio.AudioManager _audioManager;

        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        private const string GHOST_PIECE_KEY = "GhostPiece";
        private const string VIBRATION_KEY = "Vibration";

        private void Start()
        {
            _audioManager = Infrastructure.Audio.AudioManager.Instance;
            LoadSettings();
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (BackButton != null)
                BackButton.onClick.AddListener(OnBackClicked);

            if (ResetHighScoreButton != null)
                ResetHighScoreButton.onClick.AddListener(OnResetHighScore);

            if (MusicSlider != null)
                MusicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (SFXSlider != null)
                SFXSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (GhostPieceToggle != null)
                GhostPieceToggle.onValueChanged.AddListener(OnGhostPieceToggled);

            if (VibrationToggle != null)
                VibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
        }

        private void LoadSettings()
        {
            float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
            float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.7f);
            bool ghostPiece = PlayerPrefs.GetInt(GHOST_PIECE_KEY, 1) == 1;
            bool vibration = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;

            if (MusicSlider != null)
            {
                MusicSlider.value = musicVolume;
                UpdateMusicText(musicVolume);
            }

            if (SFXSlider != null)
            {
                SFXSlider.value = sfxVolume;
                UpdateSFXText(sfxVolume);
            }

            if (GhostPieceToggle != null)
                GhostPieceToggle.isOn = ghostPiece;

            if (VibrationToggle != null)
                VibrationToggle.isOn = vibration;

            ApplySettings();
        }

        private void ApplySettings()
        {
            if (_audioManager != null)
            {
                _audioManager.SetMusicVolume(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f));
                _audioManager.SetSFXVolume(PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.7f));
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
            UpdateMusicText(value);
            ApplySettings();
        }

        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
            UpdateSFXText(value);
            ApplySettings();
        }

        private void OnGhostPieceToggled(bool value)
        {
            PlayerPrefs.SetInt(GHOST_PIECE_KEY, value ? 1 : 0);
        }

        private void OnVibrationToggled(bool value)
        {
            PlayerPrefs.SetInt(VIBRATION_KEY, value ? 1 : 0);
        }

        private void UpdateMusicText(float value)
        {
            if (MusicValueText != null)
                MusicValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void UpdateSFXText(float value)
        {
            if (SFXValueText != null)
                SFXValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void OnBackClicked()
        {
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }

        private void OnResetHighScore()
        {
            PlayerPrefs.DeleteKey("HighScore");
            PlayerPrefs.Save();
            Debug.Log("High score reset!");
        }

        public void Show()
        {
            gameObject.SetActive(true);
            LoadSettings();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
