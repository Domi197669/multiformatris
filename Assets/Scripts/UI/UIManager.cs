using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Multiformatris.Core.Game;
using Multiformatris.Infrastructure.Scene;

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
        public CanvasScaler CanvasScaler;

        private GameManager _gameManager;
        private GameStateMachine _stateMachine;
        private int _lastTouchFrame = -1;
        private ScreenFitMode _lastLayoutMode;

        public void Initialize(GameManager gameManager, GameStateMachine stateMachine)
        {
            _gameManager = gameManager;
            _stateMachine = stateMachine;

            SetupButtons();
            SetupStateListener();
            ApplyAdaptiveLayout();
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

            if (ScreenManager.CurrentMode != _lastLayoutMode)
            {
                _lastLayoutMode = ScreenManager.CurrentMode;
                ApplyAdaptiveLayout();
            }

            UpdateHUD();
            HandleDirectTouch();
        }

        private void HandleDirectTouch()
        {
            if (_stateMachine == null) return;
            if (Time.frameCount == _lastTouchFrame) return;

            GameState state = _stateMachine.CurrentState;
            if (state != GameState.Menu && state != GameState.Paused && state != GameState.GameOver)
                return;

            Vector2 tapPos;
            bool gotTap = false;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    tapPos = touch.position;
                    gotTap = true;
                }
                else
                {
                    return;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                tapPos = Input.mousePosition;
                gotTap = true;
            }
            else
            {
                return;
            }

            if (!gotTap) return;
            _lastTouchFrame = Time.frameCount;

            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData ped = new PointerEventData(EventSystem.current);
            ped.position = tapPos;
            EventSystem.current.RaycastAll(ped, results);

            bool hitPlay = false, hitRetry = false, hitResume = false;
            bool hitMenu = false, hitOptions = false, hitQuit = false;
            bool hitPauseMenu = false;

            foreach (RaycastResult r in results)
            {
                GameObject go = r.gameObject;
                if (go == null) continue;

                if (PlayButton != null && go == PlayButton.gameObject) hitPlay = true;
                if (RetryButton != null && go == RetryButton.gameObject) hitRetry = true;
                if (ResumeButton != null && go == ResumeButton.gameObject) hitResume = true;
                if (PauseMenuButton != null && go == PauseMenuButton.gameObject) hitPauseMenu = true;
                if (GameOverMenuButton != null && go == GameOverMenuButton.gameObject) hitMenu = true;
                if (OptionsButton != null && go == OptionsButton.gameObject) hitOptions = true;
                if (QuitButton != null && go == QuitButton.gameObject) hitQuit = true;
            }

            if (state == GameState.Menu)
            {
                if (hitPlay) OnPlayClicked();
                else if (hitOptions) OnOptionsClicked();
                else if (hitQuit) OnQuitClicked();
            }
            else if (state == GameState.Paused)
            {
                if (hitResume) OnResumeClicked();
                else if (hitPauseMenu) OnMenuClicked();
            }
            else if (state == GameState.GameOver)
            {
                if (hitRetry) OnRetryClicked();
                else if (hitMenu) OnMenuClicked();
            }
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

        public void ApplyAdaptiveLayout()
        {
            if (CanvasScaler != null)
            {
                bool portrait = ScreenManager.IsPortrait;
                CanvasScaler.referenceResolution = portrait
                    ? new Vector2(1080f, 1920f)
                    : new Vector2(1920f, 1080f);
            }

            if (_gameManager != null && _gameManager.CameraController != null && _gameManager.GridConfig != null)
            {
                bool portrait = ScreenManager.IsPortrait;
                Camera.main.fieldOfView = portrait ? 50f : 55f;

                var grid = _gameManager.GridConfig;
                Vector3 defaultOffset = new Vector3(0, 8, -12);
                _gameManager.CameraController.FitToGrid(
                    grid.CellSize, grid.Width, grid.Height, grid.Depth, defaultOffset);
            }

            var mobileController = FindFirstObjectByType<MobileUIController>();
            if (mobileController != null) mobileController.ApplyAdaptiveLayout();
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
            try
            {
                _gameManager?.StartNewGame();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] StartNewGame failed: {e}");
                ShowError(e.Message);
            }
        }

        private void OnRetryClicked()
        {
            if (_stateMachine != null && _stateMachine.CurrentState != GameState.GameOver) return;
            try
            {
                _gameManager?.StartNewGame();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] StartNewGame (retry) failed: {e}");
                ShowError(e.Message);
            }
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

        private void ShowError(string message)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var errObj = new GameObject("ErrorOverlay");
            errObj.transform.SetParent(canvas.transform, false);
            var errRt = errObj.AddComponent<RectTransform>();
            errRt.anchorMin = Vector2.zero;
            errRt.anchorMax = Vector2.one;
            errRt.offsetMin = Vector2.zero;
            errRt.offsetMax = Vector2.zero;
            var errBg = errObj.AddComponent<Image>();
            errBg.color = new Color(0, 0, 0, 0.9f);

            var txtObj = new GameObject("ErrorText");
            txtObj.transform.SetParent(errObj.transform, false);
            var txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0.1f, 0.3f);
            txtRt.anchorMax = new Vector2(0.9f, 0.7f);
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            var txt = txtObj.AddComponent<Text>();
            txt.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
            txt.fontSize = 24;
            txt.color = Color.red;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.text = "ERROR:\n" + message;
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.OnStateChanged -= OnStateChanged;
        }
    }
}
