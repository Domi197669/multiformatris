using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Multiformatris.Core.Grid;
using Multiformatris.Core.Pieces;
using Multiformatris.Core.Gravity;
using Multiformatris.Core.Game;
using Multiformatris.Presentation;
using Multiformatris.Presentation.VFX;
using Multiformatris.Infrastructure.Audio;
using Multiformatris.Infrastructure.Input;
using Multiformatris.UI;

namespace Multiformatris.Infrastructure.Scene
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initialize()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;

            var setupObj = new GameObject("GameSetup");
            setupObj.AddComponent<GameSetup>();
            Object.DontDestroyOnLoad(setupObj);
        }
    }

    public class GameSetup : MonoBehaviour
    {
        private GameManager _gameManager;
        private UIManager _uiManager;

        void Awake()
        {
            SetupCamera();
            SetupAudio();

            var gameRoot = new GameObject("GameRoot");

            SetupGameManager(gameRoot);
            SetupGridView(gameRoot);
            SetupPieceView(gameRoot);
            SetupCameraController(gameRoot);
            SetupInput(gameRoot);
            SetupWellRotator(gameRoot);
            SetupVFX(gameRoot);
            SetupUI(gameRoot);

            _gameManager.GridView.Initialize(_gameManager.Grid, _gameManager.GridConfig);
            _gameManager.GridView.UpdateBlocks();

            var cameraTargetObj = new GameObject("CameraTarget");
            cameraTargetObj.transform.SetParent(gameRoot.transform);
            cameraTargetObj.transform.position = _gameManager.GridConfig.GetCenter();

            _gameManager.CameraController.offset = new Vector3(0, 8, -12);
            _gameManager.CameraController.SetTarget(cameraTargetObj.transform);

            _uiManager.Initialize(_gameManager, _gameManager.StateMachine);
        }

        private void SetupCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                var camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                mainCam = camObj.AddComponent<Camera>();
            }

            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
            mainCam.fieldOfView = 50;
        }

        private void SetupAudio()
        {
            if (Object.FindFirstObjectByType<AudioManager>() != null) return;

            var audioObj = new GameObject("AudioManager");
            var audioSource1 = audioObj.AddComponent<AudioSource>();
            audioSource1.playOnAwake = false;
            var audioSource2 = audioObj.AddComponent<AudioSource>();
            audioSource2.playOnAwake = false;

            var am = audioObj.AddComponent<AudioManager>();
            am.MusicSource = audioSource1;
            am.SFXSource = audioSource2;
        }

        private void SetupGameManager(GameObject gameRoot)
        {
            _gameManager = gameRoot.AddComponent<GameManager>();
            _gameManager.StartLevel = 1;
            _gameManager.LinesPerLevel = 10;
        }

        private void SetupGridView(GameObject gameRoot)
        {
            var gridObj = new GameObject("GridView");
            gridObj.transform.SetParent(gameRoot.transform);
            var gridView = gridObj.AddComponent<GridView>();
            _gameManager.GridView = gridView;
        }

        private void SetupPieceView(GameObject gameRoot)
        {
            var pieceObj = new GameObject("PieceView");
            pieceObj.transform.SetParent(gameRoot.transform);
            var pieceView = pieceObj.AddComponent<PieceView>();
            pieceView.GridView = _gameManager.GridView;
            pieceView.Config = _gameManager.GridConfig;
            _gameManager.PieceView = pieceView;
        }

        private void SetupCameraController(GameObject gameRoot)
        {
            var camControllerObj = new GameObject("CameraController");
            camControllerObj.transform.SetParent(gameRoot.transform);
            var camController = camControllerObj.AddComponent<CameraController>();
            _gameManager.CameraController = camController;
        }

        private void SetupInput(GameObject gameRoot)
        {
            var mobileInputObj = new GameObject("MobileInputHandler");
            mobileInputObj.transform.SetParent(gameRoot.transform);
            var mobileInput = mobileInputObj.AddComponent<MobileInputHandler>();
            mobileInput.UseButtons = true;
            _gameManager.MobileInputHandler = mobileInput;

            var inputObj = new GameObject("InputHandler");
            inputObj.transform.SetParent(gameRoot.transform);
            var inputHandler = inputObj.AddComponent<InputHandler>();
            _gameManager.InputHandler = inputHandler;
        }

        private void SetupWellRotator(GameObject gameRoot)
        {
            var wellObj = new GameObject("WellRotator");
            wellObj.transform.SetParent(gameRoot.transform);
            var wellRotator = wellObj.AddComponent<WellRotator>();
            _gameManager.WellRotator = wellRotator;
        }

        private void SetupVFX(GameObject gameRoot)
        {
            var screenShakeObj = new GameObject("ScreenShake");
            screenShakeObj.transform.SetParent(gameRoot.transform);
            _gameManager.ScreenShake = screenShakeObj.AddComponent<ScreenShake>();

            var clearFxObj = new GameObject("ClearEffects");
            clearFxObj.transform.SetParent(gameRoot.transform);
            _gameManager.ClearEffects = clearFxObj.AddComponent<ClearEffects>();

            var scorePopupObj = new GameObject("ScorePopup");
            scorePopupObj.transform.SetParent(gameRoot.transform);
            _gameManager.ScorePopup = scorePopupObj.AddComponent<ScorePopup>();
        }

        private void SetupUI(GameObject gameRoot)
        {
            var canvasObj = new GameObject("UICanvas");
            canvasObj.transform.SetParent(gameRoot.transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var uiManagerObj = new GameObject("UIManager");
            uiManagerObj.transform.SetParent(canvasObj.transform);
            _uiManager = uiManagerObj.AddComponent<UIManager>();

            SetupMainMenuPanel(_uiManager, canvasObj);
            SetupHUDPanel(_uiManager, canvasObj);
            SetupPausePanel(_uiManager, canvasObj);
            SetupGameOverPanel(_uiManager, canvasObj);
            SetupMobileControls(canvasObj);
        }

        private void SetupMainMenuPanel(UIManager uiManager, GameObject canvas)
        {
            var panel = CreatePanel("MainMenuPanel", canvas.transform);
            uiManager.MainMenuPanel = panel;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

            CreateText("TitleText", panel.transform, "MULTIFORMATRIS",
                new Vector2(0, 200), 72, Color.white, TextAlignmentOptions.Center);
            CreateText("SubtitleText", panel.transform, "3D Tetris",
                new Vector2(0, 120), 36, new Color(0.7f, 0.7f, 0.8f), TextAlignmentOptions.Center);

            var playBtn = CreateButton("PlayButton", panel.transform, "PLAY",
                new Vector2(0, 0), new Color(0.2f, 0.7f, 0.3f));
            uiManager.PlayButton = playBtn;

            var optionsBtn = CreateButton("OptionsButton", panel.transform, "OPTIONS",
                new Vector2(0, -80), new Color(0.3f, 0.5f, 0.8f));
            uiManager.OptionsButton = optionsBtn;

            var quitBtn = CreateButton("QuitButton", panel.transform, "QUIT",
                new Vector2(0, -160), new Color(0.7f, 0.2f, 0.2f));
            uiManager.QuitButton = quitBtn;
        }

        private void SetupHUDPanel(UIManager uiManager, GameObject canvas)
        {
            var panel = CreatePanel("HUDPanel", canvas.transform);
            uiManager.HUDPanel = panel;

            var scoreText = CreateText("ScoreText", panel.transform, "Score: 0",
                new Vector2(0, -60), 36, Color.white, TextAlignmentOptions.Center);
            uiManager.ScoreText = scoreText;

            var levelText = CreateText("LevelText", panel.transform, "Level: 1",
                new Vector2(-300, 60), 28, new Color(0.8f, 0.8f, 1f), TextAlignmentOptions.Center);
            uiManager.LevelText = levelText;

            var linesText = CreateText("LinesText", panel.transform, "Lines: 0",
                new Vector2(300, 60), 28, new Color(0.8f, 0.8f, 1f), TextAlignmentOptions.Center);
            uiManager.LinesText = linesText;

            var pauseBtn = CreateButton("PauseButton", panel.transform, "||",
                new Vector2(420, 820), new Color(0.5f, 0.5f, 0.5f, 0.8f));
            pauseBtn.GetComponentInChildren<TMP_Text>().fontSize = 28;
            uiManager.PauseButton = pauseBtn;

            panel.SetActive(false);
        }

        private void SetupPausePanel(UIManager uiManager, GameObject canvas)
        {
            var panel = CreatePanel("PausePanel", canvas.transform);
            uiManager.PausePanel = panel;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            CreateText("PauseTitle", panel.transform, "PAUSED",
                new Vector2(0, 100), 60, Color.white, TextAlignmentOptions.Center);

            var resumeBtn = CreateButton("ResumeButton", panel.transform, "RESUME",
                new Vector2(0, 0), new Color(0.2f, 0.7f, 0.3f));
            uiManager.ResumeButton = resumeBtn;

            var menuBtn = CreateButton("MenuButton", panel.transform, "MENU",
                new Vector2(0, -80), new Color(0.7f, 0.5f, 0.2f));
            uiManager.PauseMenuButton = menuBtn;

            panel.SetActive(false);
        }

        private void SetupGameOverPanel(UIManager uiManager, GameObject canvas)
        {
            var panel = CreatePanel("GameOverPanel", canvas.transform);
            uiManager.GameOverPanel = panel;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0, 0, 0.85f);

            CreateText("GameOverTitle", panel.transform, "GAME OVER",
                new Vector2(0, 200), 64, new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center);

            var finalScore = CreateText("FinalScoreText", panel.transform, "Score: 0",
                new Vector2(0, 100), 42, Color.white, TextAlignmentOptions.Center);
            uiManager.FinalScoreText = finalScore;

            var highScore = CreateText("HighScoreText", panel.transform, "Best: 0",
                new Vector2(0, 40), 32, new Color(1f, 0.8f, 0.2f), TextAlignmentOptions.Center);
            uiManager.HighScoreText = highScore;

            var retryBtn = CreateButton("RetryButton", panel.transform, "RETRY",
                new Vector2(0, -40), new Color(0.2f, 0.7f, 0.3f));
            uiManager.RetryButton = retryBtn;

            var menuBtn = CreateButton("MenuButton", panel.transform, "MENU",
                new Vector2(0, -120), new Color(0.7f, 0.5f, 0.2f));
            uiManager.GameOverMenuButton = menuBtn;

            panel.SetActive(false);
        }

        private void SetupMobileControls(GameObject canvas)
        {
            var controlsObj = new GameObject("MobileControls");
            controlsObj.transform.SetParent(canvas.transform, false);

            var controlsRt = controlsObj.AddComponent<RectTransform>();
            controlsRt.anchorMin = Vector2.zero;
            controlsRt.anchorMax = Vector2.one;
            controlsRt.offsetMin = Vector2.zero;
            controlsRt.offsetMax = Vector2.zero;

            var mobileUIObj = new GameObject("MobileUIController");
            mobileUIObj.transform.SetParent(controlsObj.transform, false);
            var mobileUI = mobileUIObj.AddComponent<MobileUIController>();
            mobileUI.CanvasScaler = canvas.GetComponent<CanvasScaler>();

            float btnSize = 120f;

            mobileUI.LeftButton = CreateMobileButton(controlsObj.transform, "LeftBtn", "<",
                new Vector2(btnSize * 0.5f, btnSize * 1.5f), new Color(0.25f, 0.25f, 0.25f, 0.7f), btnSize);
            mobileUI.RightButton = CreateMobileButton(controlsObj.transform, "RightBtn", ">",
                new Vector2(btnSize * 2.5f, btnSize * 1.5f), new Color(0.25f, 0.25f, 0.25f, 0.7f), btnSize);
            mobileUI.ForwardButton = CreateMobileButton(controlsObj.transform, "ForwardBtn", "^",
                new Vector2(btnSize * 1.5f, btnSize * 2.5f), new Color(0.25f, 0.25f, 0.25f, 0.7f), btnSize);
            mobileUI.BackButton = CreateMobileButton(controlsObj.transform, "BackBtn", "v",
                new Vector2(btnSize * 1.5f, btnSize * 0.5f), new Color(0.25f, 0.25f, 0.25f, 0.7f), btnSize);

            mobileUI.RotateXButton = CreateMobileButton(controlsObj.transform, "RotXBtn", "RX",
                new Vector2(-btnSize * 1.5f, btnSize * 2f), new Color(0.35f, 0.15f, 0.55f, 0.7f), btnSize);
            mobileUI.RotateZButton = CreateMobileButton(controlsObj.transform, "RotZBtn", "RZ",
                new Vector2(-btnSize * 0.5f, btnSize * 2f), new Color(0.35f, 0.15f, 0.55f, 0.7f), btnSize);

            mobileUI.HardDropButton = CreateMobileButton(controlsObj.transform, "HardDropBtn", "DROP",
                new Vector2(-btnSize * 1f, btnSize * 0.5f), new Color(0.7f, 0.15f, 0.15f, 0.7f), btnSize * 1.2f);
            mobileUI.SoftDropButton = CreateMobileButton(controlsObj.transform, "SoftDropBtn", "vv",
                new Vector2(-btnSize * 1f, btnSize * 1.5f), new Color(0.15f, 0.5f, 0.15f, 0.7f), btnSize);

            mobileUI.HoldButton = CreateMobileButton(controlsObj.transform, "HoldBtn", "H",
                new Vector2(-btnSize * 0.5f, btnSize * 3.5f), new Color(0.15f, 0.4f, 0.7f, 0.7f), btnSize * 0.8f);
        }

        private Button CreateMobileButton(Transform parent, string name, string label,
            Vector2 position, Color color, float size = 100f)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(size, size);

            var img = btnObj.AddComponent<Image>();
            img.color = color;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.highlightedColor = new Color(
                Mathf.Min(color.r + 0.15f, 1f),
                Mathf.Min(color.g + 0.15f, 1f),
                Mathf.Min(color.b + 0.15f, 1f));
            colors.pressedColor = new Color(
                color.r * 0.8f, color.g * 0.8f, color.b * 0.8f);
            btn.colors = colors;

            var txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            var txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = size * 0.3f;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;

            return btn;
        }

        private GameObject CreatePanel(string name, Transform parent)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return panel;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string text,
            Vector2 position, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            var txtObj = new GameObject(name);
            txtObj.transform.SetParent(parent, false);

            var rt = txtObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(600, 80);

            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;

            return tmp;
        }

        private Button CreateButton(string name, Transform parent, string label,
            Vector2 position, Color color)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(300, 70);

            var img = btnObj.AddComponent<Image>();
            img.color = color;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.highlightedColor = new Color(
                Mathf.Min(color.r + 0.15f, 1f),
                Mathf.Min(color.g + 0.15f, 1f),
                Mathf.Min(color.b + 0.15f, 1f));
            colors.pressedColor = new Color(
                color.r * 0.8f, color.g * 0.8f, color.b * 0.8f);
            btn.colors = colors;

            var txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            var txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 32;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;

            return btn;
        }
    }
}
