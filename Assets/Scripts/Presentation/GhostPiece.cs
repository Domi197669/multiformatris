using UnityEngine;
using Multiformatris.Core.Grid;
using Multiformatris.Core.Pieces;

namespace Multiformatris.Presentation
{
    public class GhostPiece : MonoBehaviour
    {
        [Header("Settings")]
        public float GhostAlpha = 0.3f;
        public bool EnableGhost = true;

        private PieceView _pieceView;
        private GridView _gridView;
        private GridData _grid;

        private Vector3Int[] _ghostCells;
        private Vector3Int _ghostPosition;
        private bool _isActive;

        public Vector3Int[] GhostCells => _ghostCells;
        public Vector3Int GhostPosition => _ghostPosition;
        public bool IsActive => _isActive && EnableGhost;

        public void Initialize(PieceView pieceView, GridView gridView, GridData grid)
        {
            _pieceView = pieceView;
            _gridView = gridView;
            _grid = grid;
        }

        public void UpdateGhost()
        {
            if (!EnableGhost || _pieceView == null || !_pieceView.IsActive)
            {
                ClearGhost();
                return;
            }

            _ghostPosition = CalculateGhostPosition();
            _ghostCells = _pieceView.RotatedCells;
            _isActive = true;
        }

        private Vector3Int CalculateGhostPosition()
        {
            Vector3Int currentPos = _pieceView.CurrentPosition;
            Vector3Int[] cells = _pieceView.RotatedCells;

            Vector3Int testPos = currentPos;
            Vector3Int lastValidPos = currentPos;

            while (GridOperations.CanPlacePiece(_grid, cells, testPos + Vector3Int.down))
            {
                testPos += Vector3Int.down;
                lastValidPos = testPos;
            }

            return lastValidPos;
        }

        public void RenderGhost()
        {
            if (!IsActive || _gridView == null) return;

            _gridView.ClearPieceBlocks();
            _gridView.UpdatePieceBlocks(_ghostCells, _ghostPosition, GetGhostColorIndex(), true);
        }

        public void ClearGhost()
        {
            _isActive = false;
            _ghostCells = null;
        }

        private int GetGhostColorIndex()
        {
            if (_pieceView == null || _pieceView.CurrentPiece == null) return 0;

            string name = _pieceView.CurrentPiece.PieceName;
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

        public void SetEnabled(bool enabled)
        {
            EnableGhost = enabled;
            if (!enabled) ClearGhost();
        }
    }
}
