using UnityEngine;
using System;

namespace Multiformatris.Core.Grid
{
    public enum CellState { Empty, Occupied }

    public struct Cell
    {
        public CellState State;
        public int PieceId;
        public int ColorIndex;

        public static Cell Empty => new Cell { State = CellState.Empty, PieceId = -1, ColorIndex = -1 };
    }

    public class GridData
    {
        private readonly Cell[,,] _cells;
        private readonly int _width;
        private readonly int _height;
        private readonly int _depth;

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;

        public GridData(int width, int height, int depth)
        {
            _width = width;
            _height = height;
            _depth = depth;
            _cells = new Cell[width, height, depth];
        }

        public Cell GetCell(int x, int y, int z)
        {
            if (!InBounds(x, y, z)) return Cell.Empty;
            return _cells[x, y, z];
        }

        public void SetCell(int x, int y, int z, Cell cell)
        {
            if (InBounds(x, y, z))
                _cells[x, y, z] = cell;
        }

        public bool IsEmpty(int x, int y, int z)
        {
            if (!InBounds(x, y, z)) return false;
            return _cells[x, y, z].State == CellState.Empty;
        }

        public bool IsOccupied(int x, int y, int z)
        {
            if (!InBounds(x, y, z)) return true;
            return _cells[x, y, z].State == CellState.Occupied;
        }

        public bool InBounds(int x, int y, int z)
        {
            return x >= 0 && x < _width &&
                   y >= 0 && y < _height &&
                   z >= 0 && z < _depth;
        }

        public bool InBounds(Vector3Int pos)
        {
            return InBounds(pos.x, pos.y, pos.z);
        }

        public void OccupyCell(int x, int y, int z, int pieceId, int colorIndex)
        {
            if (InBounds(x, y, z))
            {
                _cells[x, y, z] = new Cell
                {
                    State = CellState.Occupied,
                    PieceId = pieceId,
                    ColorIndex = colorIndex
                };
            }
        }

        public void ClearCell(int x, int y, int z)
        {
            if (InBounds(x, y, z))
                _cells[x, y, z] = Cell.Empty;
        }

        public void Clear()
        {
            Array.Clear(_cells, 0, _cells.Length);
        }
    }
}
