using UnityEngine;
using System;

namespace Multiformatris.Infrastructure.Input
{
    public class MobileInputHandler : MonoBehaviour
    {
        public event Action<Vector3Int> OnMove;
        public event Action OnRotateX;
        public event Action OnRotateZ;
        public event Action OnHardDrop;
        public event Action OnSoftDrop;
        public event Action OnHold;
        public event Action OnPause;

        [Header("Swipe Settings")]
        public float SwipeThreshold = 50f;
        public float SwipeCooldown = 0.1f;

        private Vector2 _touchStartPos;
        private float _touchStartTime;
        private bool _isSwiping;
        private float _swipeCooldownTimer;
        private bool _softDropActive;

        private void Update()
        {
            _swipeCooldownTimer -= Time.deltaTime;

            if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _touchStartPos = touch.position;
                        _touchStartTime = Time.time;
                        _isSwiping = true;
                        break;

                    case TouchPhase.Moved:
                        if (_isSwiping && _swipeCooldownTimer <= 0f)
                            HandleSwipe(touch);
                        break;

                    case TouchPhase.Ended:
                        HandleTap(touch);
                        _isSwiping = false;
                        break;
                }
            }

            HandleSoftDropButton();
        }

        private void HandleSwipe(Touch touch)
        {
            Vector2 delta = touch.position - _touchStartPos;
            float distance = delta.magnitude;

            if (distance < SwipeThreshold) return;

            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            if (angle > -45f && angle <= 45f)
            {
                OnMove?.Invoke(Vector3Int.right);
            }
            else if (angle > 45f && angle <= 135f)
            {
                OnMove?.Invoke(Vector3Int.forward);
            }
            else if (angle > -135f && angle <= -45f)
            {
                OnMove?.Invoke(Vector3Int.back);
            }
            else
            {
                OnMove?.Invoke(Vector3Int.left);
            }

            _touchStartPos = touch.position;
            _swipeCooldownTimer = SwipeCooldown;
        }

        private void HandleTap(Touch touch)
        {
            float tapDuration = Time.time - _touchStartTime;

            if (tapDuration < 0.2f)
            {
                if (touch.position.x > Screen.width * 0.7f)
                    OnRotateZ?.Invoke();
                else if (touch.position.x < Screen.width * 0.3f)
                    OnRotateX?.Invoke();
                else
                    OnHardDrop?.Invoke();
            }
        }

        private void HandleSoftDropButton()
        {
            _softDropActive = false;
        }

        public void OnSoftDropButtonDown()
        {
            _softDropActive = true;
            OnSoftDrop?.Invoke();
        }

        public void OnSoftDropButtonUp()
        {
            _softDropActive = false;
        }

        public void OnHoldButton()
        {
            OnHold?.Invoke();
        }

        public void OnPauseButton()
        {
            OnPause?.Invoke();
        }

        public void OnRotateXButton()
        {
            OnRotateX?.Invoke();
        }

        public void OnRotateZButton()
        {
            OnRotateZ?.Invoke();
        }

        public void OnHardDropButton()
        {
            OnHardDrop?.Invoke();
        }

        public void OnMoveLeft()
        {
            OnMove?.Invoke(Vector3Int.left);
        }

        public void OnMoveRight()
        {
            OnMove?.Invoke(Vector3Int.right);
        }

        public void OnMoveForward()
        {
            OnMove?.Invoke(Vector3Int.forward);
        }

        public void OnMoveBack()
        {
            OnMove?.Invoke(Vector3Int.back);
        }
    }
}
