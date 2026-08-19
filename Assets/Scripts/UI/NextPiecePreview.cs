using UnityEngine;
using Multiformatris.Core.Pieces;

namespace Multiformatris.UI
{
    public class NextPiecePreview : MonoBehaviour
    {
        [Header("References")]
        public PieceView PieceView;
        public GameManager GameManager;

        [Header("Preview Settings")]
        public Transform PreviewContainer;
        public float PreviewScale = 0.5f;
        public Color PreviewColor = Color.white;

        private GameObject[] _previewBlocks;
        private PieceDefinition _lastPiece;

        private void Start()
        {
            _previewBlocks = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                _previewBlocks[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _previewBlocks[i].transform.SetParent(PreviewContainer);
                _previewBlocks[i].transform.localScale = Vector3.one * PreviewScale;
                _previewBlocks[i].SetActive(false);
            }
        }

        private void Update()
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            PieceDefinition nextPiece = GameManager?.GetNextPiece();
            if (nextPiece == null || nextPiece == _lastPiece) return;

            _lastPiece = nextPiece;
            ShowPiece(nextPiece);
        }

        private void ShowPiece(PieceDefinition piece)
        {
            Vector3 center = piece.GetCenter();

            for (int i = 0; i < 4; i++)
            {
                if (i < piece.Cells.Length)
                {
                    Vector3 pos = (piece.Cells[i] - center) * PreviewScale;
                    _previewBlocks[i].transform.localPosition = pos;
                    _previewBlocks[i].SetActive(true);

                    Renderer renderer = _previewBlocks[i].GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = piece.BlockColor;
                }
                else
                {
                    _previewBlocks[i].SetActive(false);
                }
            }
        }

        public void ClearPreview()
        {
            _lastPiece = null;
            for (int i = 0; i < _previewBlocks.Length; i++)
            {
                if (_previewBlocks[i] != null)
                    _previewBlocks[i].SetActive(false);
            }
        }
    }
}
