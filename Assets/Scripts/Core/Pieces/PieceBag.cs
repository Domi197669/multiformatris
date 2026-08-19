using UnityEngine;
using System.Collections.Generic;

namespace Multiformatris.Core.Pieces
{
    [CreateAssetMenu(fileName = "PieceBag", menuName = "Multiformatris/Piece Bag")]
    public class PieceBag : ScriptableObject
    {
        public PieceDefinition[] AllPieces;

        private List<PieceDefinition> _bag = new List<PieceDefinition>();

        public PieceDefinition GetNext()
        {
            if (_bag.Count == 0)
                RefillBag();

            PieceDefinition piece = _bag[0];
            _bag.RemoveAt(0);
            return piece;
        }

        public PieceDefinition PeekNext()
        {
            if (_bag.Count == 0)
                RefillBag();

            return _bag[0];
        }

        private void RefillBag()
        {
            _bag.AddRange(AllPieces);
            Shuffle(_bag);
        }

        private void Shuffle(List<PieceDefinition> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public void Reset()
        {
            _bag.Clear();
        }
    }
}
