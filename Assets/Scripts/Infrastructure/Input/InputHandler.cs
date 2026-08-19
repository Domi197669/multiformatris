using UnityEngine;
using System;

namespace Multiformatris.Infrastructure.Input
{
    public class InputHandler : MonoBehaviour
    {
        public event Action<Vector3Int> OnMove;
        public event Action OnRotateX;
        public event Action OnRotateZ;
        public event Action OnHardDrop;
        public event Action OnSoftDrop;
        public event Action OnHold;
        public event Action OnPause;

        private float _moveTimer;
        private float _moveDelay = 0.1f;
        private bool _moveHeld;

        private void Update()
        {
            HandleMove();
            HandleRotate();
            HandleDrop();
            HandleHold();
            HandlePause();
        }

        private void HandleMove()
        {
            float h = UnityEngine.Input.GetAxisRaw("Horizontal");
            float v = UnityEngine.Input.GetAxisRaw("Vertical");

            _moveTimer -= Time.deltaTime;

            if (_moveTimer <= 0f)
            {
                if (Mathf.Abs(h) > 0.5f)
                {
                    int x = h > 0 ? 1 : -1;
                    OnMove?.Invoke(new Vector3Int(x, 0, 0));
                    _moveTimer = _moveDelay;
                }

                if (Mathf.Abs(v) > 0.5f)
                {
                    int z = v > 0 ? 1 : -1;
                    OnMove?.Invoke(new Vector3Int(0, 0, z));
                    _moveTimer = _moveDelay;
                }
            }

            if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
                _moveTimer = 0f;
        }

        private void HandleRotate()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
                OnRotateX?.Invoke();

            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
                OnRotateZ?.Invoke();
        }

        private void HandleDrop()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                OnHardDrop?.Invoke();

            if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
                OnSoftDrop?.Invoke();
        }

        private void HandleHold()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftControl))
                OnHold?.Invoke();
        }

        private void HandlePause()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                OnPause?.Invoke();
        }
    }
}
