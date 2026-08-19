using UnityEngine;

namespace Multiformatris.Core.Grid
{
    [CreateAssetMenu(fileName = "GridConfig", menuName = "Multiformatris/Grid Config")]
    public class GridConfig : ScriptableObject
    {
        [Header("Dimensions")]
        public int Width = 5;
        public int Height = 10;
        public int Depth = 5;

        [Header("Cell")]
        public float CellSize = 1f;
        public Vector3 GridOrigin = Vector3.zero;

        public Vector3Int Dimensions => new Vector3Int(Width, Height, Depth);

        public Vector3 GridToWorld(Vector3Int gridPos)
        {
            return GridOrigin + new Vector3(gridPos.x, gridPos.y, gridPos.z) * CellSize;
        }

        public Vector3Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.RoundToInt((worldPos.x - GridOrigin.x) / CellSize),
                Mathf.RoundToInt((worldPos.y - GridOrigin.y) / CellSize),
                Mathf.RoundToInt((worldPos.z - GridOrigin.z) / CellSize));
        }

        public Vector3 GetCenter()
        {
            return GridOrigin + new Vector3(Width, Height, Depth) * CellSize * 0.5f;
        }
    }
}
