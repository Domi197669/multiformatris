using UnityEngine;
using UnityEngine.UI;
using Multiformatris.Core.Game;

namespace Multiformatris.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        public Text ScoreText;
        public Text LevelText;
        public Text LinesText;
        public Button PauseButton;

        [Header("Panels")]
        public GameObject HUDPanel;
        public GameObject PausePanel;
        public GameObject GameOverPanel;
        public GameObject MainMenuPanel;

        [Header("Game Over")]
        public Text FinalScoreText;
        public Text HighScoreText;
        public Button RetryButton;
        public Button GameOverMenuButton;

        [Header("Pause")]
        public Button ResumeButton;
        public Button PauseMenuButton;

        [Header("Main Menu")]
        public Button PlayButton;
        public Button OptionsButton;
        public Button QuitButton;

        [Header("Mobile")]
        public CanvasGroup MobileControlsGroup;

        private GameManager _gameManager;
        private GameStateMachine _stateMachine;
        private bool _touchConsumed;

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
            if (PlayButton != null) PlayButton.onClick.AddListener(OnPlayClicked);
            if (RetryButton != null) RetryButton.onClick.AddListener(OnRetryClicked);
            if (GameOverMenuButton != null) GameOverMenuButton.onClick.AddListener(OnMenuClicked);
            if (ResumeButton != null) ResumeButton.onClick.AddListener(OnResumeClicked);
            if (PauseMenuButton != null) PauseMenuButton.onClick.AddListener(OnMenuClicked);
            if (PauseButton != null) PauseButton.onClick.AddListener(OnPauseClicked);
            if (OptionsButton != null) OptionsButton.onClick.AddListener(OnOptionsClicked);
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
                case GameState.Clearing:
                    ShowHUD();
                    SetMobileControlsVisible(true);
                    break;
                case GameState.Paused:
                    ShowPause();
                    SetMobileControlsVisible(false);
                    break;
                case GameState.GameOver:
                    ShowGameOver();
                    SetMobileControlsVisible(false);
                    break;
                case GameState.Menu:
                    ShowMainMenu();
                    SetMobileControlsVisible(false);
                    break;
            }
        }

        private void SetMobileControlsVisible(bool visible)
        {
            if (MobileControlsGroup != null)
            {
                MobileControlsGroup.alpha = visible ? 1f : 0f;
                MobileControlsGroup.blocksRaycasts = visible;
                MobileControlsGroup.interactable = visible;
            }
        }

        private void Update()
        {
            if (_gameManager == null) return;

            UpdateHUD();
            HandleDirectTouch();
        }

        private void HandleDirectTouch()
        {
            if (_stateMachine == null) return;
            if (_stateMachine.CurrentState != GameState.Menu &&
                _stateMachine.CurrentState != GameState.Paused &&
                _stateMachine.CurrentState != GameState.GameOver)
                return;

            bool tapDown = false;
            Vector2 tapPos = Vector2.zero;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    tapDown = true;
                    tapPos = touch.position;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                tapDown = true;
                tapPos = Input.mousePosition;
            }

            if (!tapDown) return;

            _touchConsumed = false;

            switch (_stateMachine.CurrentState)
            {
                case GameState.Menu:
                    if (HitButton(PlayButton, tapPos)) OnPlayClicked();
                    else if (HitButton(OptionsButton, tapPos)) OnOptionsClicked();
                    else if (HitButton(QuitButton, tapPos)) OnQuitClicked();
                    break;
                case GameState.Paused:
                    if (HitButton(ResumeButton, tapPos)) OnResumeClicked();
                    else if (HitButton(PauseMenuButton, tapPos)) OnMenuClicked();
                    break;
                case GameState.GameOver:
                    if (HitButton(RetryButton, tapPos)) OnRetryClicked();
                    else if (HitButton(GameOverMenuButton, tapPos)) OnMenuClicked();
                    break;
            }
        }

        private bool HitButton(Button button, Vector2 screenPos)
        {
            if (button == null || !button.gameObject.activeInHierarchy) return false;
            RectTransform rt = button.GetComponent<RectTransform>();
            if (rt == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null);
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
            if (_stateMachine != null && _stateMachine.CurrentState != GameState.Menu) return;
            _gameManager?.StartNewGame();
        }

        private void OnRetryClicked()
        {
            if (_stateMachine != null && _stateMachine.CurrentState != GameState.GameOver) return;
            _gameManager?.StartNewGame();
        }

        private void OnResumeClicked()
        {
            if (_stateMachine != null && _stateMachine.CurrentState != GameState.Paused) return;
            _stateMachine?.TransitionTo(GameState.Falling);
        }

        private void OnPauseClicked()
        {
            if (_stateMachine?.CurrentState == GameState.Falling)
                _stateMachine.TransitionTo(GameState.Paused);
        }

        private void OnMenuClicked()
        {
            _stateMachine?.TransitionTo(GameState.Menu);
            ShowMainMenu();
        }

        private void OnOptionsClicked()
        {
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
