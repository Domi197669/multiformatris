using UnityEngine;
using System.Collections.Generic;

namespace Multiformatris.Core.Grid
{
    public static class GridOperations
    {
        public static bool CanPlacePiece(GridData grid, Vector3Int[] pieceCells, Vector3Int offset)
        {
            for (int i = 0; i < pieceCells.Length; i++)
            {
                Vector3Int worldPos = pieceCells[i] + offset;

                if (!grid.InBounds(worldPos))
                    return false;

                if (grid.IsOccupied(worldPos.x, worldPos.y, worldPos.z))
                    return false;
            }
            return true;
        }

        public static void PlacePiece(GridData grid, Vector3Int[] pieceCells, Vector3Int offset, int pieceId, int colorIndex)
        {
            for (int i = 0; i < pieceCells.Length; i++)
            {
                Vector3Int worldPos = pieceCells[i] + offset;
                grid.OccupyCell(worldPos.x, worldPos.y, worldPos.z, pieceId, colorIndex);
            }
        }

        public static Vector3Int GetSpawnPosition(GridData grid, Vector3Int[] pieceCells, Vector3Int gravityDir)
        {
            Vector3Int center = new Vector3Int(grid.Width / 2, grid.Height / 2, grid.Depth / 2);

            if (gravityDir == Vector3Int.down)
                return new Vector3Int(center.x, grid.Height - 2, center.z);
            else if (gravityDir == Vector3Int.up)
                return new Vector3Int(center.x, 1, center.z);
            else if (gravityDir == Vector3Int.right)
                return new Vector3Int(1, center.y, center.z);
            else if (gravityDir == Vector3Int.left)
                return new Vector3Int(grid.Width - 2, center.y, center.z);

            return center;
        }

        public static Vector3Int GetStepPosition(Vector3Int current, Vector3Int gravityDir)
        {
            return current + gravityDir;
        }

        public static bool CanStep(GridData grid, Vector3Int[] pieceCells, Vector3Int currentOffset, Vector3Int gravityDir)
        {
            Vector3Int nextOffset = currentOffset + gravityDir;
            return CanPlacePiece(grid, pieceCells, nextOffset);
        }

        public static List<int> GetFullLayers(GridData grid, Vector3Int gravityDir)
        {
            List<int> fullLayers = new List<int>();

            if (gravityDir == Vector3Int.down || gravityDir == Vector3Int.up)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    bool full = true;
                    for (int x = 0; x < grid.Width && full; x++)
                        for (int z = 0; z < grid.Depth && full; z++)
                            if (grid.IsEmpty(x, y, z)) full = false;

                    if (full) fullLayers.Add(y);
                }
            }
            else if (gravityDir == Vector3Int.right || gravityDir == Vector3Int.left)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    bool full = true;
                    for (int y = 0; y < grid.Height && full; y++)
                        for (int z = 0; z < grid.Depth && full; z++)
                            if (grid.IsEmpty(x, y, z)) full = false;

                    if (full) fullLayers.Add(x);
                }
            }

            return fullLayers;
        }

        public static void ClearLayers(GridData grid, List<int> layers, Vector3Int gravityDir)
        {
            layers.Sort();

            if (gravityDir == Vector3Int.down || gravityDir == Vector3Int.up)
            {
                foreach (int y in layers)
                {
                    for (int x = 0; x < grid.Width; x++)
                        for (int z = 0; z < grid.Depth; z++)
                            grid.ClearCell(x, y, z);

                    int shiftDir = gravityDir == Vector3Int.down ? 1 : -1;

                    if (gravityDir == Vector3Int.down)
                    {
                        for (int aboveY = y + 1; aboveY < grid.Height; aboveY++)
                        {
                            for (int x = 0; x < grid.Width; x++)
                                for (int z = 0; z < grid.Depth; z++)
                                {
                                    Cell cell = grid.GetCell(x, aboveY, z);
                                    grid.SetCell(x, aboveY - 1, z, cell);
                                    grid.ClearCell(x, aboveY, z);
                                }
                        }
                    }
                    else
                    {
                        for (int belowY = y - 1; belowY >= 0; belowY--)
                        {
                            for (int x = 0; x < grid.Width; x++)
                                for (int z = 0; z < grid.Depth; z++)
                                {
                                    Cell cell = grid.GetCell(x, belowY, z);
                                    grid.SetCell(x, belowY + 1, z, cell);
                                    grid.ClearCell(x, belowY, z);
                                }
                        }
                    }
                }
            }
            else if (gravityDir == Vector3Int.right || gravityDir == Vector3Int.left)
            {
                foreach (int x in layers)
                {
                    for (int y = 0; y < grid.Height; y++)
                        for (int z = 0; z < grid.Depth; z++)
                            grid.ClearCell(x, y, z);

                    if (gravityDir == Vector3Int.right)
                    {
                        for (int leftX = x - 1; leftX >= 0; leftX--)
                        {
                            for (int y = 0; y < grid.Height; y++)
                                for (int z = 0; z < grid.Depth; z++)
                                {
                                    Cell cell = grid.GetCell(leftX, y, z);
                                    grid.SetCell(leftX + 1, y, z, cell);
                                    grid.ClearCell(leftX, y, z);
                                }
                        }
                    }
                    else
                    {
                        for (int rightX = x + 1; rightX < grid.Width; rightX++)
                        {
                            for (int y = 0; y < grid.Height; y++)
                                for (int z = 0; z < grid.Depth; z++)
                                {
                                    Cell cell = grid.GetCell(rightX, y, z);
                                    grid.SetCell(rightX - 1, y, z, cell);
                                    grid.ClearCell(rightX, y, z);
                                }
                        }
                    }
                }
            }
        }

        public static bool IsGameOver(GridData grid, Vector3Int[] pieceCells, Vector3Int spawnOffset)
        {
            return !CanPlacePiece(grid, pieceCells, spawnOffset);
        }
    }
}
