using UnityEngine;
using Multiformatris.Core.Grid;
using Multiformatris.Core.Pieces;

namespace Multiformatris.Presentation
{
    public class PieceView : MonoBehaviour
    {
        [Header("References")]
        public GridView GridView;
        public GridConfig Config;

        [Header("Settings")]
        public float SoftDropSpeed = 20f;

        private PieceDefinition _currentPiece;
        private Vector3Int _currentPosition;
        private Vector3Int[] _rotatedCells;
        private int _rotationX;
        private int _rotationZ;
        private bool _isActive;

        public PieceDefinition CurrentPiece => _currentPiece;
        public Vector3Int CurrentPosition => _currentPosition;
        public Vector3Int[] RotatedCells => _rotatedCells;
        public bool IsActive => _isActive;

        public void SpawnPiece(PieceDefinition piece, Vector3Int spawnPos)
        {
            _currentPiece = piece;
            _currentPosition = spawnPos;
            _rotationX = 0;
            _rotationZ = 0;
            _rotatedCells = (Vector3Int[])piece.Cells.Clone();
            _isActive = true;

            UpdateVisual();
        }

        public bool Move(Vector3Int direction, GridData grid)
        {
            if (!_isActive) return false;

            Vector3Int newPos = _currentPosition + direction;

            if (GridOperations.CanPlacePiece(grid, _rotatedCells, newPos))
            {
                _currentPosition = newPos;
                UpdateVisual();
                return true;
            }

            return false;
        }

        public bool RotateX(GridData grid)
        {
            if (!_isActive) return false;

            Vector3Int[] newCells = RotateCells(_rotatedCells, 90, 'x');

            if (GridOperations.CanPlacePiece(grid, newCells, _currentPosition))
            {
                _rotatedCells = newCells;
                _rotationX = (_rotationX + 90) % 360;
                UpdateVisual();
                return true;
            }

            return false;
        }

        public bool RotateZ(GridData grid)
        {
            if (!_isActive) return false;

            Vector3Int[] newCells = RotateCells(_rotatedCells, 90, 'z');

            if (GridOperations.CanPlacePiece(grid, newCells, _currentPosition))
            {
                _rotatedCells = newCells;
                _rotationZ = (_rotationZ + 90) % 360;
                UpdateVisual();
                return true;
            }

            return false;
        }

        public bool HardDrop(GridData grid)
        {
            if (!_isActive) return false;

            while (GridOperations.CanPlacePiece(grid, _rotatedCells, _currentPosition + Vector3Int.down))
            {
                _currentPosition += Vector3Int.down;
            }

            UpdateVisual();
            return true;
        }

        public Vector3Int GetGhostPosition(GridData grid)
        {
            Vector3Int ghostPos = _currentPosition;

            while (GridOperations.CanPlacePiece(grid, _rotatedCells, ghostPos + Vector3Int.down))
            {
                ghostPos += Vector3Int.down;
            }

            return ghostPos;
        }

        public bool CanMoveDown(GridData grid)
        {
            return GridOperations.CanStep(grid, _rotatedCells, _currentPosition, Vector3Int.down);
        }

        public void Lock(GridData grid, int pieceId, int colorIndex)
        {
            if (!_isActive) return;

            GridOperations.PlacePiece(grid, _rotatedCells, _currentPosition, pieceId, colorIndex);
            _isActive = false;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        private Vector3Int[] RotateCells(Vector3Int[] cells, int degrees, char axis)
        {
            Vector3Int[] rotated = new Vector3Int[cells.Length];

            for (int i = 0; i < cells.Length; i++)
            {
                Vector3 cell = cells[i];

                if (axis == 'x')
                {
                    float rad = degrees * Mathf.Deg2Rad;
                    float y = cell.y * Mathf.Cos(rad) - cell.z * Mathf.Sin(rad);
                    float z = cell.y * Mathf.Sin(rad) + cell.z * Mathf.Cos(rad);
                    rotated[i] = new Vector3Int(cell.x, Mathf.RoundToInt(y), Mathf.RoundToInt(z));
                }
                else if (axis == 'z')
                {
                    float rad = degrees * Mathf.Deg2Rad;
                    float x = cell.x * Mathf.Cos(rad) - cell.y * Mathf.Sin(rad);
                    float y = cell.x * Mathf.Sin(rad) + cell.y * Mathf.Cos(rad);
                    rotated[i] = new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), cell.z);
                }
            }

            return rotated;
        }

        private void UpdateVisual()
        {
            if (GridView != null && _isActive)
            {
                GridView.ClearPieceBlocks();
                GridView.UpdatePieceBlocks(_rotatedCells, _currentPosition, GetColorIndex());
            }
        }

        private int GetColorIndex()
        {
            if (_currentPiece == null) return 0;

            string name = _currentPiece.PieceName;
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
    }
}
