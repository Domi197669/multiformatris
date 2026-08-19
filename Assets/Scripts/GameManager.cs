using UnityEngine;
using Multiformatris.Core.Grid;
using Multiformatris.Core.Pieces;
using Multiformatris.Core.Game;
using Multiformatris.Core.Gravity;
using Multiformatris.Infrastructure.Input;
using Multiformatris.Infrastructure.Audio;
using Multiformatris.Presentation;
using Multiformatris.Presentation.VFX;

namespace Multiformatris
{
    public class GameManager : MonoBehaviour
    {
        [Header("Configs")]
        public GridConfig GridConfig;
        public GravityConfig GravityConfig;
        public PieceBag PieceBag;

        [Header("References")]
        public GridView GridView;
        public PieceView PieceView;
        public CameraController CameraController;
        public InputHandler InputHandler;
        public MobileInputHandler MobileInputHandler;
        public WellRotator WellRotator;

        [Header("VFX")]
        public ClearEffects ClearEffects;
        public ScreenShake ScreenShake;
        public ScorePopup ScorePopup;
        public GhostPiece GhostPiece;

        [Header("Systems")]
        public ComboSystem ComboSystem;

        [Header("Game Settings")]
        public int StartLevel = 1;
        public int LinesPerLevel = 10;

        private GridData _grid;
        private GameStateMachine _stateMachine;
        private int _currentLevel;
        private int _linesCleared;
        private int _score;
        private float _dropTimer;
        private float _lockTimer;
        private bool _holdUsed;
        private PieceDefinition _holdPiece;
        private int _pieceIdCounter;

        private void Awake()
        {
            InitializeGame();
        }

        private void Start()
        {
            SetupInput();
            StartNewGame();
        }

        private void Update()
        {
            if (!_stateMachine.IsPlaying()) return;

            switch (_stateMachine.CurrentState)
            {
                case GameState.Falling:
                    UpdateFalling();
                    break;
                case GameState.Locking:
                    UpdateLocking();
                    break;
            }
        }

        private void InitializeGame()
        {
            _grid = new GridData(GridConfig.Width, GridConfig.Height, GridConfig.Depth);
            _stateMachine = new GameStateMachine(GameState.Menu);

            if (GridView != null)
                GridView.Initialize(_grid, GridConfig);

            if (CameraController != null)
            {
                GameObject cameraTarget = new GameObject("CameraTarget");
                cameraTarget.transform.position = GridConfig.GetCenter();
                CameraController.SetTarget(cameraTarget.transform);
            }

            if (ComboSystem == null)
                ComboSystem = gameObject.AddComponent<ComboSystem>();
        }

        private void SetupInput()
        {
            InputHandler handler = InputHandler != null ? InputHandler :
                (MobileInputHandler != null ? MobileInputHandler : null);

            if (handler == null) return;

            if (handler is InputHandler kbHandler)
            {
                kbHandler.OnMove += HandleMove;
                kbHandler.OnRotateX += HandleRotateX;
                kbHandler.OnRotateZ += HandleRotateZ;
                kbHandler.OnHardDrop += HandleHardDrop;
                kbHandler.OnSoftDrop += HandleSoftDrop;
                kbHandler.OnHold += HandleHold;
                kbHandler.OnPause += HandlePause;
            }
            else if (handler is MobileInputHandler mobileHandler)
            {
                mobileHandler.OnMove += HandleMove;
                mobileHandler.OnRotateX += HandleRotateX;
                mobileHandler.OnRotateZ += HandleRotateZ;
                mobileHandler.OnHardDrop += HandleHardDrop;
                mobileHandler.OnSoftDrop += HandleSoftDrop;
                mobileHandler.OnHold += HandleHold;
                mobileHandler.OnPause += HandlePause;
            }
        }

        public void StartNewGame()
        {
            _grid.Clear();
            _currentLevel = StartLevel;
            _linesCleared = 0;
            _score = 0;
            _holdPiece = null;
            _holdUsed = false;
            _pieceIdCounter = 0;

            PieceBag.Reset();

            if (GridView != null)
                GridView.ClearAll();

            if (ComboSystem != null)
                ComboSystem.ResetCombo();

            if (WellRotator != null)
                WellRotator.UpdateRotationForGravity(GravityConfig.GetGravityForLevel(_currentLevel));

            _stateMachine.TransitionTo(GameState.Spawning);
            SpawnPiece();

            AudioManager.Instance?.PlaySFX(Resources.Load<AudioClip>("SFX/GameStart"));
        }

        private void SpawnPiece()
        {
            PieceDefinition piece = PieceBag.GetNext();
            Vector3Int spawnPos = GetSpawnPosition();

            _dropTimer = 0f;
            _lockTimer = 0f;
            _holdUsed = false;

            if (PieceView != null)
                PieceView.SpawnPiece(piece, spawnPos);

            if (GhostPiece != null)
                GhostPiece.Initialize(PieceView, GridView, _grid);

            _stateMachine.TransitionTo(GameState.Falling);
        }

        private Vector3Int GetSpawnPosition()
        {
            Vector3Int gravityDir = GravityConfig.GetGravityForLevel(_currentLevel);
            return GridOperations.GetSpawnPosition(_grid, new Vector3Int[0], gravityDir);
        }

        private void UpdateFalling()
        {
            float dropInterval = GravityConfig.GetDropInterval(_currentLevel);
            _dropTimer += Time.deltaTime;

            if (_dropTimer >= dropInterval)
            {
                _dropTimer = 0f;

                if (PieceView != null && !PieceView.CanMoveDown(_grid))
                {
                    _stateMachine.TransitionTo(GameState.Locking);
                    _lockTimer = 0f;
                }
                else
                {
                    PieceView?.Move(Vector3Int.down, _grid);
                }
            }

            UpdateGhost();
        }

        private void UpdateLocking()
        {
            _lockTimer += Time.deltaTime;

            if (_lockTimer >= 0.5f)
            {
                LockPiece();
            }
        }

        private void LockPiece()
        {
            if (PieceView == null || PieceView.CurrentPiece == null) return;

            PieceView.Lock(_grid, _pieceIdCounter, GetColorIndex(PieceView.CurrentPiece));
            _pieceIdCounter++;

            AudioManager.Instance?.PlayLock();

            if (ScreenShake != null)
                ScreenShake.ShakeDrop();

            if (GridView != null)
            {
                GridView.ClearPieceBlocks();
                GridView.UpdateBlocks();
            }

            CheckForClearedLayers();
        }

        private void CheckForClearedLayers()
        {
            Vector3Int gravityDir = GravityConfig.GetGravityForLevel(_currentLevel);
            var fullLayers = GridOperations.GetFullLayers(_grid, gravityDir);

            if (fullLayers.Count > 0)
            {
                _stateMachine.TransitionTo(GameState.Clearing);
                ClearLayers(fullLayers, gravityDir);
            }
            else
            {
                CheckGameOver();
            }
        }

        private void ClearLayers(System.Collections.Generic.List<int> layers, Vector3Int gravityDir)
        {
            int points = ComboSystem != null ?
                ComboSystem.CalculatePoints(layers.Count, _currentLevel) :
                CalculatePoints(layers.Count);

            _score += points;
            _linesCleared += layers.Count;

            AudioManager.Instance?.PlayClear();

            if (ScreenShake != null)
                ScreenShake.ShakeClear(layers.Count);

            if (ScorePopup != null && points > 0)
            {
                Vector3 popupPos = GridConfig.GetCenter();
                ScorePopup.Show(popupPos, points);

                if (ComboSystem != null && ComboSystem.CurrentCombo >= 2)
                    ScorePopup.ShowCombo(popupPos + Vector3.up * 0.5f, ComboSystem.CurrentCombo);
            }

            GridOperations.ClearLayers(_grid, layers, gravityDir);

            if (GridView != null)
                GridView.UpdateBlocks();

            CheckLevelUp();
        }

        private int CalculatePoints(int layersCount)
        {
            int[] pointsPerLayer = { 100, 300, 500, 800 };
            int index = Mathf.Min(layersCount - 1, pointsPerLayer.Length - 1);
            return pointsPerLayer[index] * _currentLevel;
        }

        private void CheckLevelUp()
        {
            int newLevel = StartLevel + (_linesCleared / LinesPerLevel);

            if (newLevel > _currentLevel)
            {
                _currentLevel = newLevel;

                AudioManager.Instance?.PlayLevelUp();

                if (ScreenShake != null)
                    ScreenShake.ShakeLevelUp();

                if (WellRotator != null)
                {
                    Vector3Int newGravity = GravityConfig.GetGravityForLevel(_currentLevel);
                    WellRotator.RotateToGravity(newGravity);
                }

                Debug.Log($"Level Up! Now level {_currentLevel}");
            }

            _stateMachine.TransitionTo(GameState.Spawning);
            SpawnPiece();
        }

        private void CheckGameOver()
        {
            Vector3Int spawnPos = GetSpawnPosition();
            PieceDefinition nextPiece = PieceBag.PeekNext();

            if (nextPiece != null && GridOperations.IsGameOver(_grid, nextPiece.Cells, spawnPos))
            {
                _stateMachine.TransitionTo(GameState.GameOver);
                AudioManager.Instance?.PlayGameOver();

                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                if (_score > highScore)
                {
                    PlayerPrefs.SetInt("HighScore", _score);
                    PlayerPrefs.Save();
                }

                Debug.Log($"Game Over! Score: {_score}");
            }
            else
            {
                _stateMachine.TransitionTo(GameState.Spawning);
                SpawnPiece();
            }
        }

        private int GetColorIndex(PieceDefinition piece)
        {
            string name = piece.PieceName;
            switch (name)
            {
                case "I": return 0;
                case "J": return 1;
                case "L": return 2;
                case "O": return 3;
                case "S": return 4;
                case "T": return 5;
                case "Z": return 6;
                default: return 0;
            }
        }

        private void UpdateGhost()
        {
            if (GhostPiece != null && GhostPiece.EnableGhost)
            {
                GhostPiece.UpdateGhost();
                GhostPiece.RenderGhost();
            }
            else if (PieceView != null && PieceView.CurrentPiece != null)
            {
                Vector3Int ghostPos = PieceView.GetGhostPosition(_grid);
                GridView?.ClearPieceBlocks();
                GridView?.UpdatePieceBlocks(PieceView.RotatedCells, ghostPos, GetColorIndex(PieceView.CurrentPiece), true);
            }
        }

        #region Input Handlers

        private void HandleMove(Vector3Int direction)
        {
            if (_stateMachine.CurrentState != GameState.Falling) return;

            bool moved = PieceView?.Move(direction, _grid) ?? false;
            if (moved)
            {
                AudioManager.Instance?.PlayMove();
                UpdateGhost();
            }
        }

        private void HandleRotateX()
        {
            if (_stateMachine.CurrentState != GameState.Falling) return;

            bool rotated = PieceView?.RotateX(_grid) ?? false;
            if (rotated)
            {
                AudioManager.Instance?.PlayRotate();
                UpdateGhost();
            }
        }

        private void HandleRotateZ()
        {
            if (_stateMachine.CurrentState != GameState.Falling) return;

            bool rotated = PieceView?.RotateZ(_grid) ?? false;
            if (rotated)
            {
                AudioManager.Instance?.PlayRotate();
                UpdateGhost();
            }
        }

        private void HandleHardDrop()
        {
            if (_stateMachine.CurrentState != GameState.Falling) return;

            PieceView?.HardDrop(_grid);
            AudioManager.Instance?.PlayDrop();
            LockPiece();
        }

        private void HandleSoftDrop()
        {
            if (_stateMachine.CurrentState != GameState.Falling) return;

            if (PieceView != null && PieceView.CanMoveDown(_grid))
            {
                PieceView.Move(Vector3Int.down, _grid);
                _score += 1;
            }
        }

        private void HandleHold()
        {
            if (_stateMachine.CurrentState != GameState.Falling || _holdUsed) return;

            PieceDefinition currentPiece = PieceView?.CurrentPiece;
            if (currentPiece == null) return;

            AudioManager.Instance?.PlaySFX(Resources.Load<AudioClip>("SFX/Hold"));

            if (_holdPiece != null)
            {
                PieceView?.SpawnPiece(_holdPiece, GetSpawnPosition());
            }
            else
            {
                PieceView?.Deactivate();
                _stateMachine.TransitionTo(GameState.Spawning);
                SpawnPiece();
            }

            _holdPiece = currentPiece;
            _holdUsed = true;
        }

        private void HandlePause()
        {
            if (_stateMachine.CurrentState == GameState.Paused)
                _stateMachine.TransitionTo(GameState.Falling);
            else if (_stateMachine.IsPlaying())
                _stateMachine.TransitionTo(GameState.Paused);
        }

        #endregion

        public int GetScore() => _score;
        public int GetLevel() => _currentLevel;
        public int GetLines() => _linesCleared;
        public PieceDefinition GetHoldPiece() => _holdPiece;
        public PieceDefinition GetNextPiece() => PieceBag?.PeekNext();
        public GameStateMachine GetStateMachine() => _stateMachine;
    }
}
