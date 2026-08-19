using UnityEngine;
using System.Collections;

namespace Multiformatris.Presentation.VFX
{
    public class ScreenShake : MonoBehaviour
    {
        [Header("Shake Settings")]
        public float DefaultDuration = 0.2f;
        public float DefaultMagnitude = 0.1f;

        [Header("Drop Shake")]
        public float DropDuration = 0.15f;
        public float DropMagnitude = 0.05f;

        [Header("Clear Shake")]
        public float ClearDuration = 0.3f;
        public float ClearMagnitude = 0.15f;

        [Header("Level Up Shake")]
        public float LevelUpDuration = 0.5f;
        public float LevelUpMagnitude = 0.2f;

        private Vector3 _originalPosition;
        private float _shakeDuration;
        private float _shakeMagnitude;
        private bool _isShaking;

        private static ScreenShake _instance;
        public static ScreenShake Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void LateUpdate()
        {
            if (_isShaking)
            {
                if (_shakeDuration > 0f)
                {
                    transform.localPosition = _originalPosition + Random.insideUnitSphere * _shakeMagnitude;
                    _shakeDuration -= Time.deltaTime;
                }
                else
                {
                    transform.localPosition = _originalPosition;
                    _isShaking = false;
                }
            }
        }

        public void Shake(float duration, float magnitude)
        {
            _originalPosition = transform.localPosition;
            _shakeDuration = duration;
            _shakeMagnitude = magnitude;
            _isShaking = true;
        }

        public void ShakeDrop()
        {
            Shake(DropDuration, DropMagnitude);
        }

        public void ShakeClear(int layersCleared = 1)
        {
            float magnitude = ClearMagnitude * layersCleared;
            Shake(ClearDuration, magnitude);
        }

        public void ShakeLevelUp()
        {
            Shake(LevelUpDuration, LevelUpMagnitude);
        }

        public void ShakeCustom(float duration, float magnitude)
        {
            Shake(duration, magnitude);
        }

        public void StopShake()
        {
            _isShaking = false;
            transform.localPosition = _originalPosition;
        }
    }
}
