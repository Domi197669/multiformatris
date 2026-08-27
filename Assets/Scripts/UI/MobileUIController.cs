using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Multiformatris.Infrastructure.Scene;

namespace Multiformatris.UI
{
    public class MobileUIController : MonoBehaviour
    {
        [Header("Button References")]
        public Button LeftButton;
        public Button RightButton;
        public Button ForwardButton;
        public Button BackButton;
        public Button RotateXButton;
        public Button RotateZButton;
        public Button HardDropButton;
        public Button SoftDropButton;
        public Button HoldButton;
        public Button PauseButton;

        [Header("Layout")]
        public CanvasScaler CanvasScaler;
        public float ReferenceWidth = 1080f;
        public float ReferenceHeight = 1920f;

        private Infrastructure.Input.MobileInputHandler _inputHandler;

        private void Start()
        {
            _inputHandler = FindFirstObjectByType<Infrastructure.Input.MobileInputHandler>();
            SetupButtons();
            SetupCanvasScaler();
            ApplyAdaptiveLayout();
        }

        private void SetupButtons()
        {
            if (_inputHandler == null) return;

            if (LeftButton != null) LeftButton.onClick.AddListener(_inputHandler.OnMoveLeft);
            if (RightButton != null) RightButton.onClick.AddListener(_inputHandler.OnMoveRight);
            if (ForwardButton != null) ForwardButton.onClick.AddListener(_inputHandler.OnMoveForward);
            if (BackButton != null) BackButton.onClick.AddListener(_inputHandler.OnMoveBack);
            if (RotateXButton != null) RotateXButton.onClick.AddListener(_inputHandler.OnRotateXButton);
            if (RotateZButton != null) RotateZButton.onClick.AddListener(_inputHandler.OnRotateZButton);
            if (HardDropButton != null) HardDropButton.onClick.AddListener(_inputHandler.OnHardDropButton);
            if (SoftDropButton != null)
            {
                SoftDropButton.onClick.AddListener(_inputHandler.OnSoftDropButtonDown);
            }
            if (HoldButton != null) HoldButton.onClick.AddListener(_inputHandler.OnHoldButton);
            if (PauseButton != null) PauseButton.onClick.AddListener(_inputHandler.OnPauseButton);
        }

        private void SetupCanvasScaler()
        {
            if (CanvasScaler == null) return;

            CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            CanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            CanvasScaler.matchWidthOrHeight = 0.5f;
            ApplyCanvasScalerResolution();
        }

        private void ApplyCanvasScalerResolution()
        {
            if (CanvasScaler == null) return;
            bool portrait = ScreenManager.IsPortrait;
            CanvasScaler.referenceResolution = portrait
                ? new Vector2(ReferenceWidth, ReferenceHeight)
                : new Vector2(ReferenceHeight, ReferenceWidth);
        }

        public void ApplyAdaptiveLayout()
        {
            ApplyCanvasScalerResolution();

            bool portrait = ScreenManager.IsPortrait;
            float s = portrait ? 120f : 90f;
            float edge = portrait ? s * 0.5f : s * 0.35f;

            SetBtn(LeftButton, Anchor.Left, new Vector2(edge, edge), s, portrait);
            SetBtn(RightButton, Anchor.Left, new Vector2(edge + s * 1.0f, edge), s, portrait);
            SetBtn(ForwardButton, Anchor.Left, new Vector2(edge + s * 0.5f, edge + s), s, portrait);
            SetBtn(BackButton, Anchor.Left, new Vector2(edge + s * 0.5f, edge - s), s, portrait);

            float rEdge = portrait ? s * 0.5f : s * 0.35f;
            SetBtn(RotateXButton, Anchor.Right, new Vector2(-rEdge, rEdge + s * 2.0f), s, portrait);
            SetBtn(RotateZButton, Anchor.Right, new Vector2(-rEdge - s * 1.0f, rEdge + s * 2.0f), s, portrait);
            SetBtn(SoftDropButton, Anchor.Right, new Vector2(-rEdge - s * 1.0f, rEdge + s * 1.0f), s, portrait);
            SetBtn(HardDropButton, Anchor.Right, new Vector2(-rEdge, rEdge), s * 1.2f, portrait);

            SetBtn(HoldButton, Anchor.Right, new Vector2(-rEdge, rEdge + s * 3.5f), s * 0.8f, portrait);
        }

        private enum Anchor { Left, Right }

        private void SetBtn(Button btn, Anchor anchor, Vector2 pos, float size, bool portrait)
        {
            if (btn == null) return;
            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt == null) return;

            Vector2 pivot = anchor == Anchor.Left ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rt.anchorMin = pivot;
            rt.anchorMax = pivot;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(size, size);
        }

        private void Update()
        {
            HandleSoftDropHold();
        }

        private void HandleSoftDropHold()
        {
            if (SoftDropButton == null || _inputHandler == null) return;
            if (!SoftDropButton.gameObject.activeInHierarchy) return;

            if (UnityEngine.Input.GetMouseButton(0))
            {
                bool isOverButton = false;
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Input.mousePosition;

                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                foreach (var result in results)
                {
                    if (result.gameObject == SoftDropButton.gameObject)
                    {
                        isOverButton = true;
                        break;
                    }
                }

                if (isOverButton)
                    _inputHandler.OnSoftDropButtonDown();
            }
        }
    }
}
