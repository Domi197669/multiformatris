using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            _inputHandler = FindObjectOfType<Infrastructure.Input.MobileInputHandler>();
            SetupButtons();
            SetupCanvasScaler();
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
            CanvasScaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            CanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            CanvasScaler.matchWidthOrHeight = 0.5f;
        }

        private void Update()
        {
            HandleSoftDropHold();
        }

        private void HandleSoftDropHold()
        {
            if (SoftDropButton == null || _inputHandler == null) return;

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
