using UnityEngine;

namespace Multiformatris.Core.Pieces
{
    public static class PieceFactory
    {
        public static PieceDefinition[] CreateAllPieces()
        {
            PieceDefinition[] pieces = new PieceDefinition[7];

            pieces[0] = CreateI();
            pieces[1] = CreateJ();
            pieces[2] = CreateL();
            pieces[3] = CreateO();
            pieces[4] = CreateS();
            pieces[5] = CreateT();
            pieces[6] = CreateZ();

            return pieces;
        }

        private static PieceDefinition CreateI()
        {
            PieceDefinition piece = ScriptableObject.CreateInstance<PieceDefinition>();
            piece.PieceName = "I";
            piece.Cells = new Vector3Int[]
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(2, 0, 0),
                new Vector3Int(3, 0, 0)
            };
            piece.BlockColor = Color.cyan;
            return piece;
        }

        private static PieceDefinition CreateJ()
        {
            PieceDefinition piece = ScriptableObject.CreateInstance<PieceDefinition>();
            piece.PieceName = "J";
            piece.Cells = new Vector3Int[]
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(2, 1, 0)
            };
            piece.BlockColor = Color.blue;
            return piece;
        }

        private static PieceDefinition CreateL()
        {
            PieceDefinition piece = ScriptableObject.CreateInstance<PieceDefinition>();
            piece.PieceName = "L";
            piece.Cells = new Vector3Int[]
            {
                new Vector3Int(2, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(2, 1, 0)
            };
            piece.BlockColor = new Color(1f, 0.5f, 0f);
            return piece;
        }

        private static PieceDefinition CreateO()
        {
            PieceDefinition piece = ScriptableObject.CreateInstance<PieceDefinition>();
            piece.PieceName = "O";
            piece.Cells = new Vector3Int[]
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(1, 1, 0)
            };
            piece.BlockColor = Color.yellow;
            return piece;
        }

        private static PieceDefinition CreateS()
        {
            PieceDefinition piece = ScriptableObject.CreateInstance<PieceDefinition>();
            piece.PieceName = "S";
            piece.Cells = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0),
                new Vector3Int(2, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(1, 1, 0)
            };
            piece.BlockColor = Color.green;
            return piece;
        }

        private static PieceDefinition CreateT()
        {
            PieceDefinition piece = ScriptableObject.CreateInstance<PieceDefinition>();
            piece.PieceName = "T";
            piece.Cells = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(2, 1, 0)
            };
            piece.BlockColor = Color.magenta;
            return piece;
        }

        private static PieceDefinition CreateZ()
        {
            PieceDefinition piece = ScriptableObject.CreateInstance<PieceDefinition>();
            piece.PieceName = "Z";
            piece.Cells = new Vector3Int[]
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(1, 1, 0),
                new Vector3Int(2, 1, 0)
            };
            piece.BlockColor = Color.red;
            return piece;
        }
    }
}
