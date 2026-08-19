using UnityEngine;
using System.Collections.Generic;
using Multiformatris.Core.Grid;

namespace Multiformatris.Presentation
{
    public class GridView : MonoBehaviour
    {
        [Header("References")]
        public GridData Grid;
        public GridConfig Config;

        [Header("Visual")]
        public Material GridMaterial;
        public Material BlockMaterial;
        public float GridLineWidth = 0.02f;

        private GameObject _gridLines;
        private Dictionary<Vector3Int, GameObject> _blockObjects = new Dictionary<Vector3Int, GameObject>();
        private List<GameObject> _pool = new List<GameObject>();

        public void Initialize(GridData grid, GridConfig config)
        {
            Grid = grid;
            Config = config;
            CreateGridLines();
        }

        private void CreateGridLines()
        {
            if (_gridLines != null)
                Destroy(_gridLines);

            _gridLines = new GameObject("GridLines");
            _gridLines.transform.SetParent(transform);

            CreateLineGrid();
        }

        private void CreateLineGrid()
        {
            Color lineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            for (int x = 0; x <= Config.Width; x++)
                CreateLine(
                    Config.GridToWorld(new Vector3Int(x, 0, 0)),
                    Config.GridToWorld(new Vector3Int(x, Config.Height, 0)),
                    Config.GridToWorld(new Vector3Int(x, 0, Config.Depth)),
                    lineColor);

            for (int y = 0; y <= Config.Height; y++)
                CreateLine(
                    Config.GridToWorld(new Vector3Int(0, y, 0)),
                    Config.GridToWorld(new Vector3Int(Config.Width, y, 0)),
                    Config.GridToWorld(new Vector3Int(0, y, Config.Depth)),
                    lineColor);

            for (int z = 0; z <= Config.Depth; z++)
                CreateLine(
                    Config.GridToWorld(new Vector3Int(0, 0, z)),
                    Config.GridToWorld(new Vector3Int(Config.Width, 0, z)),
                    Config.GridToWorld(new Vector3Int(0, Config.Height, z)),
                    lineColor);
        }

        private void CreateLine(Vector3 start, Vector3 endX, Vector3 endZ, Color color)
        {
            GameObject lineObj = new GameObject("GridLine");
            lineObj.transform.SetParent(_gridLines.transform);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = GridMaterial != null ? GridMaterial : new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = GridLineWidth;
            lr.endWidth = GridLineWidth;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, endX);

            GameObject lineObj2 = new GameObject("GridLine2");
            lineObj2.transform.SetParent(_gridLines.transform);

            LineRenderer lr2 = lineObj2.AddComponent<LineRenderer>();
            lr2.material = lr.material;
            lr2.startColor = color;
            lr2.endColor = color;
            lr2.startWidth = GridLineWidth;
            lr2.endWidth = GridLineWidth;
            lr2.positionCount = 2;
            lr2.SetPosition(0, start);
            lr2.SetPosition(1, endZ);
        }

        public void UpdateBlocks()
        {
            HashSet<Vector3Int> currentPositions = new HashSet<Vector3Int>();

            for (int x = 0; x < Grid.Width; x++)
                for (int y = 0; y < Grid.Height; y++)
                    for (int z = 0; z < Grid.Depth; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        Cell cell = Grid.GetCell(x, y, z);

                        if (cell.State == CellState.Occupied)
                        {
                            currentPositions.Add(pos);

                            if (!_blockObjects.ContainsKey(pos))
                            {
                                GameObject block = CreateBlock(pos, cell.ColorIndex);
                                _blockObjects[pos] = block;
                            }
                        }
                    }

            List<Vector3Int> toRemove = new List<Vector3Int>();
            foreach (var kvp in _blockObjects)
            {
                if (!currentPositions.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }

            foreach (var pos in toRemove)
            {
                ReturnBlock(_blockObjects[pos]);
                _blockObjects.Remove(pos);
            }
        }

        public void UpdatePieceBlocks(Vector3Int[] pieceCells, Vector3Int offset, int colorIndex, bool isGhost = false)
        {
            ClearPieceBlocks();

            for (int i = 0; i < pieceCells.Length; i++)
            {
                Vector3Int worldPos = pieceCells[i] + offset;
                if (Grid.InBounds(worldPos))
                {
                    GameObject block = CreateBlock(worldPos, colorIndex, isGhost);
                    block.name = "PieceBlock";
                }
            }
        }

        public void ClearPieceBlocks()
        {
            foreach (var block in GameObject.FindGameObjectsWithTag("Untagged"))
            {
                if (block.name == "PieceBlock")
                    ReturnBlock(block);
            }
        }

        private GameObject CreateBlock(Vector3Int gridPos, int colorIndex, bool ghost = false)
        {
            GameObject block = GetBlockFromPool();
            block.transform.position = Config.GridToWorld(gridPos);
            block.transform.localScale = Vector3.one * Config.CellSize * 0.95f;
            block.SetActive(true);

            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (BlockMaterial != null)
                    renderer.material = BlockMaterial;

                Color[] colors = GetBlockColors();
                Color blockColor = colors[colorIndex % colors.Length];

                if (ghost)
                {
                    blockColor.a = 0.3f;
                    renderer.material.color = blockColor;
                }
                else
                {
                    renderer.material.color = blockColor;
                }
            }

            return block;
        }

        private GameObject GetBlockFromPool()
        {
            foreach (var block in _pool)
            {
                if (!block.activeSelf)
                    return block;
            }

            GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newBlock.transform.SetParent(transform);
            newBlock.tag = "Untagged";
            _pool.Add(newBlock);
            return newBlock;
        }

        private void ReturnBlock(GameObject block)
        {
            if (block != null)
                block.SetActive(false);
        }

        private Color[] GetBlockColors()
        {
            return new Color[]
            {
                Color.cyan,     // I
                Color.blue,     // J
                new Color(1f, 0.5f, 0f), // L
                Color.yellow,   // O
                Color.green,    // S
                Color.magenta,  // T
                Color.red       // Z
            };
        }

        public void ClearAll()
        {
            foreach (var kvp in _blockObjects)
                ReturnBlock(kvp.Value);
            _blockObjects.Clear();
        }
    }
}
