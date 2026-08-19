using UnityEngine;
using UnityEngine.UI;

namespace Multiformatris.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class ResponsiveCanvas : MonoBehaviour
    {
        [Header("Reference Resolution")]
        public float ReferenceWidth = 1080f;
        public float ReferenceHeight = 1920f;

        [Header("Scaling")]
        public CanvasScaler.ScreenMatchMode MatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [Range(0f, 1f)]
        public float MatchWidthOrHeight = 0.5f;

        private CanvasScaler _canvasScaler;

        private void Awake()
        {
            _canvasScaler = GetComponent<CanvasScaler>();
            SetupCanvasScaler();
        }

        private void SetupCanvasScaler()
        {
            if (_canvasScaler == null) return;

            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            _canvasScaler.screenMatchMode = MatchMode;
            _canvasScaler.matchWidthOrHeight = MatchWidthOrHeight;

            #if UNITY_ANDROID || UNITY_IOS
            _canvasScaler.referenceResolution = new Vector2(1080, 1920);
            _canvasScaler.matchWidthOrHeight = 0.5f;
            #endif
        }

        private void Update()
        {
            HandleOrientationChange();
        }

        private void HandleOrientationChange()
        {
            float aspectRatio = (float)Screen.width / Screen.height;

            if (aspectRatio > 1f)
            {
                _canvasScaler.matchWidthOrHeight = 0f;
            }
            else
            {
                _canvasScaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
}
