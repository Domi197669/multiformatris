using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Multiformatris.Core.Game;

namespace Multiformatris.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI LevelText;
        public TextMeshProUGUI LinesText;

        [Header("Panels")]
        public GameObject HUDPanel;
        public GameObject PausePanel;
        public GameObject GameOverPanel;
        public GameObject MainMenuPanel;

        [Header("Game Over")]
        public TextMeshProUGUI FinalScoreText;
        public TextMeshProUGUI HighScoreText;
        public Button RetryButton;
        public Button MenuButton;

        [Header("Pause")]
        public Button ResumeButton;
        public Button PauseMenuButton;

        [Header("Main Menu")]
        public Button PlayButton;
        public Button OptionsButton;
        public Button QuitButton;

        private GameManager _gameManager;
        private GameStateMachine _stateMachine;

        public void Initialize(GameManager gameManager, GameStateMachine stateMachine)
        {
            _gameManager = gameManager;
            _stateMachine = stateMachine;

            SetupButtons();
            SetupStateListener();
            ShowMainMenu();
        }

        private void SetupButtons()
        {
            if (RetryButton != null) RetryButton.onClick.AddListener(OnRetryClicked);
            if (MenuButton != null) MenuButton.onClick.AddListener(OnMenuClicked);
            if (ResumeButton != null) ResumeButton.onClick.AddListener(OnResumeClicked);
            if (PauseMenuButton != null) PauseMenuButton.onClick.AddListener(OnMenuClicked);
            if (PlayButton != null) PlayButton.onClick.AddListener(OnPlayClicked);
            if (QuitButton != null) QuitButton.onClick.AddListener(OnQuitClicked);
        }

        private void SetupStateListener()
        {
            if (_stateMachine != null)
                _stateMachine.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Falling:
                case GameState.Spawning:
                    ShowHUD();
                    break;
                case GameState.Paused:
                    ShowPause();
                    break;
                case GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }

        private void Update()
        {
            if (_gameManager == null) return;

            UpdateHUD();
        }

        private void UpdateHUD()
        {
            if (ScoreText != null)
                ScoreText.text = $"Score: {_gameManager.GetScore():N0}";

            if (LevelText != null)
                LevelText.text = $"Level: {_gameManager.GetLevel()}";

            if (LinesText != null)
                LinesText.text = $"Lines: {_gameManager.GetLines()}";
        }

        public void ShowMainMenu()
        {
            SetPanelActive(MainMenuPanel);
        }

        public void ShowHUD()
        {
            SetPanelActive(HUDPanel);
        }

        public void ShowPause()
        {
            SetPanelActive(PausePanel);
        }

        public void ShowGameOver()
        {
            SetPanelActive(GameOverPanel);

            if (FinalScoreText != null)
                FinalScoreText.text = $"Score: {_gameManager.GetScore():N0}";

            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (_gameManager.GetScore() > highScore)
            {
                highScore = _gameManager.GetScore();
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }

            if (HighScoreText != null)
                HighScoreText.text = $"Best: {highScore:N0}";
        }

        private void SetPanelActive(GameObject panel)
        {
            if (HUDPanel != null) HUDPanel.SetActive(panel == HUDPanel);
            if (PausePanel != null) PausePanel.SetActive(panel == PausePanel);
            if (GameOverPanel != null) GameOverPanel.SetActive(panel == GameOverPanel);
            if (MainMenuPanel != null) MainMenuPanel.SetActive(panel == MainMenuPanel);
        }

        private void OnPlayClicked()
        {
            _gameManager?.StartNewGame();
        }

        private void OnRetryClicked()
        {
            _gameManager?.StartNewGame();
        }

        private void OnResumeClicked()
        {
            _stateMachine?.TransitionTo(GameState.Falling);
        }

        private void OnMenuClicked()
        {
            ShowMainMenu();
        }

        private void OnQuitClicked()
        {
            Application.Quit();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.OnStateChanged -= OnStateChanged;
        }
    }
}
