using UnityEngine;
using System.Collections;
using Multiformatris.Core.Gravity;

namespace Multiformatris.Presentation
{
    public class WellRotator : MonoBehaviour
    {
        [Header("Settings")]
        public float RotationDuration = 1.0f;
        public AnimationCurve RotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Visual")]
        public bool RotateWithGravity = true;

        private Quaternion _targetRotation;
        private bool _isRotating;

        public event System.Action OnRotationComplete;

        private void Start()
        {
            if (RotateWithGravity)
                UpdateRotationForGravity(Vector3Int.down);
        }

        public void RotateToGravity(Vector3Int newGravity, float duration = -1f)
        {
            if (_isRotating) return;

            float dur = duration > 0 ? duration : RotationDuration;
            Quaternion targetRot = GetRotationForGravity(newGravity);

            if (targetRot == transform.rotation) return;

            _targetRotation = targetRot;
            StartCoroutine(RotateCoroutine(dur));
        }

        private IEnumerator RotateCoroutine(float duration)
        {
            _isRotating = true;
            Quaternion startRotation = transform.rotation;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveT = RotationCurve.Evaluate(t);
                transform.rotation = Quaternion.Slerp(startRotation, _targetRotation, curveT);
                yield return null;
            }

            transform.rotation = _targetRotation;
            _isRotating = false;
            OnRotationComplete?.Invoke();
        }

        private Quaternion GetRotationForGravity(Vector3Int gravity)
        {
            if (gravity == Vector3Int.down)
                return Quaternion.identity;
            else if (gravity == Vector3Int.up)
                return Quaternion.Euler(180f, 0f, 0f);
            else if (gravity == Vector3Int.right)
                return Quaternion.Euler(0f, 0f, 90f);
            else if (gravity == Vector3Int.left)
                return Quaternion.Euler(0f, 0f, -90f);
            else if (gravity == Vector3Int.forward)
                return Quaternion.Euler(-90f, 0f, 0f);
            else if (gravity == Vector3Int.back)
                return Quaternion.Euler(90f, 0f, 0f);

            return Quaternion.identity;
        }

        public void UpdateRotationForGravity(Vector3Int gravity)
        {
            transform.rotation = GetRotationForGravity(gravity);
        }

        public bool IsRotating => _isRotating;
    }
}
