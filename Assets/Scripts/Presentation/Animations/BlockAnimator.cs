using UnityEngine;
using System.Collections;

namespace Multiformatris.Presentation.Animations
{
    public class BlockAnimator : MonoBehaviour
    {
        [Header("Lock Animation")]
        public float LockBounceHeight = 0.2f;
        public float LockBounceDuration = 0.15f;

        [Header("Clear Animation")]
        public float ClearShrinkDuration = 0.2f;
        public float ClearFadeDuration = 0.3f;

        [Header("Fall Animation")]
        public float FallSquash = 0.8f;
        public float FallStretch = 1.1f;

        private Renderer _renderer;
        private Material _material;
        private Color _originalColor;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _material = _renderer.material;
                _originalColor = _material.color;
            }
        }

        public void PlayLockAnimation()
        {
            StartCoroutine(LockBounceCoroutine());
        }

        private IEnumerator LockBounceCoroutine()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 squashedScale = new Vector3(
                originalScale.x * 1.1f,
                originalScale.y * FallSquash,
                originalScale.z * 1.1f);

            transform.localScale = squashedScale;

            float elapsed = 0f;
            while (elapsed < LockBounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / LockBounceDuration;
                float bounce = Mathf.Sin(t * Mathf.PI) * LockBounceHeight;
                transform.position += Vector3.up * bounce * Time.deltaTime;
                transform.localScale = Vector3.Lerp(squashedScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        public void PlayClearAnimation(System.Action onComplete = null)
        {
            StartCoroutine(ClearCoroutine(onComplete));
        }

        private IEnumerator ClearCoroutine(System.Action onComplete)
        {
            Vector3 originalScale = transform.localScale;
            Color targetColor = _originalColor;
            targetColor.a = 0f;

            float elapsed = 0f;
            while (elapsed < ClearShrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ClearShrinkDuration;
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

                if (_material != null)
                    _material.color = Color.Lerp(_originalColor, targetColor, t);

                yield return null;
            }

            onComplete?.Invoke();
        }

        public void PlayFallAnimation()
        {
            StartCoroutine(FallSquashCoroutine());
        }

        private IEnumerator FallSquashCoroutine()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 stretchScale = new Vector3(
                originalScale.x * FallStretch,
                originalScale.y * 0.9f,
                originalScale.z * FallStretch);

            transform.localScale = stretchScale;

            float elapsed = 0f;
            float duration = 0.1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(stretchScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        public void PlayFlash(Color flashColor, float duration = 0.1f)
        {
            StartCoroutine(FlashCoroutine(flashColor, duration));
        }

        private IEnumerator FlashCoroutine(Color flashColor, float duration)
        {
            if (_material == null) yield break;

            Color originalColor = _material.color;
            _material.color = flashColor;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _material.color = Color.Lerp(flashColor, originalColor, t);
                yield return null;
            }

            _material.color = originalColor;
        }

        public void SetColor(Color color)
        {
            _originalColor = color;
            if (_material != null)
                _material.color = color;
        }

        public void ResetAnimation()
        {
            StopAllCoroutines();
            transform.localScale = Vector3.one;
            if (_material != null)
                _material.color = _originalColor;
        }
    }
}
