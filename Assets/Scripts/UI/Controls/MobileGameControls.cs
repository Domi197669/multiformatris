using UnityEngine;
using UnityEngine.UI;
using System;

namespace Multiformatris.UI.Controls
{
    public class MobileGameControls : MonoBehaviour
    {
        [Header("Joystick (Movement)")]
        public VirtualJoystick MovementJoystick;

        [Header("Rotation Wheel")]
        public RotationWheel RotationWheel;

        [Header("Action Buttons")]
        public Button HardDropButton;
        public Button SoftDropButton;
        public Button HoldButton;
        public Button PauseButton;

        [Header("Layout")]
        public float JoystickSize = 200f;
        public float ButtonSize = 80f;
        public float ScreenPadding = 50f;

        public event Action<Vector3Int> OnMove;
        public event Action OnRotateX;
        public event Action OnRotateZ;
        public event Action OnHardDrop;
        public event Action OnSoftDrop;
        public event Action OnHold;
        public event Action OnPause;

        private void Start()
        {
            SetupJoystick();
            SetupRotationWheel();
            SetupButtons();
        }

        private void SetupJoystick()
        {
            if (MovementJoystick == null) return;

            MovementJoystick.OnDirectionChanged += (dir) =>
            {
                if (dir.y > 0) OnMove?.Invoke(Vector3Int.forward);
                else if (dir.y < 0) OnMove?.Invoke(Vector3Int.back);
                else if (dir.x > 0) OnMove?.Invoke(Vector3Int.right);
                else if (dir.x < 0) OnMove?.Invoke(Vector3Int.left);
            };
        }

        private void SetupRotationWheel()
        {
            if (RotationWheel == null) return;

            RotationWheel.OnRotateLeft += () => OnRotateX?.Invoke();
            RotationWheel.OnRotateRight += () => OnRotateZ?.Invoke();
            RotationWheel.OnRotateUp += () => OnRotateX?.Invoke();
            RotationWheel.OnRotateDown += () => OnRotateZ?.Invoke();
        }

        private void SetupButtons()
        {
            if (HardDropButton != null)
                HardDropButton.onClick.AddListener(() => OnHardDrop?.Invoke());

            if (SoftDropButton != null)
            {
                SoftDropButton.onClick.AddListener(() => OnSoftDrop?.Invoke());
            }

            if (HoldButton != null)
                HoldButton.onClick.AddListener(() => OnHold?.Invoke());

            if (PauseButton != null)
                PauseButton.onClick.AddListener(() => OnPause?.Invoke());
        }

        public void SetControlsActive(bool active)
        {
            if (MovementJoystick != null)
                MovementJoystick.SetActive(active);

            if (RotationWheel != null)
                RotationWheel.SetActive(active);

            if (HardDropButton != null)
                HardDropButton.interactable = active;

            if (SoftDropButton != null)
                SoftDropButton.interactable = active;

            if (HoldButton != null)
                HoldButton.interactable = active;
        }
    }
}
