using UnityEngine;

namespace Multiformatris.Core.Pieces
{
    [CreateAssetMenu(fileName = "PieceDefinition", menuName = "Multiformatris/Piece Definition")]
    public class PieceDefinition : ScriptableObject
    {
        public string PieceName = "I";
        public Vector3Int[] Cells = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(2, 0, 0),
            new Vector3Int(3, 0, 0)
        };

        public Color BlockColor = Color.white;
        public Material BlockMaterial;

        public Vector3Int GetCenter()
        {
            if (Cells == null || Cells.Length == 0) return Vector3Int.zero;

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < Cells.Length; i++)
                sum += (Vector3)Cells[i];

            Vector3 center = sum / Cells.Length;
            return new Vector3Int(
                Mathf.RoundToInt(center.x),
                Mathf.RoundToInt(center.y),
                Mathf.RoundToInt(center.z));
        }
    }
}
