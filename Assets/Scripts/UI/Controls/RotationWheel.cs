using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Multiformatris.UI.Controls
{
    public class RotationWheel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Settings")]
        public float RotationThreshold = 30f;
        public float RotationCooldown = 0.3f;

        [Header("References")]
        public RectTransform WheelBackground;
        public RectTransform WheelKnob;

        private Canvas _canvas;
        private Camera _camera;
        private float _lastAngle;
        private float _rotationTimer;
        private bool _isDragging;

        public event Action OnRotateLeft;
        public event Action OnRotateRight;
        public event Action OnRotateUp;
        public event Action OnRotateDown;

        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
                _camera = _canvas.worldCamera;

            if (WheelBackground == null)
                WheelBackground = GetComponent<RectTransform>();
        }

        private void Update()
        {
            _rotationTimer -= Time.deltaTime;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            UpdateKnobPosition(eventData);
            CalculateAngle(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            UpdateKnobPosition(eventData);
            DetectRotation(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
            _lastAngle = 0f;

            if (WheelKnob != null)
                WheelKnob.anchoredPosition = Vector2.zero;
        }

        private void UpdateKnobPosition(PointerEventData eventData)
        {
            if (WheelBackground != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    WheelBackground, eventData.position, _camera, out Vector2 localPoint))
            {
                float maxRadius = WheelBackground.rect.width * 0.5f;
                float distance = Mathf.Min(localPoint.magnitude, maxRadius);
                Vector2 clampedPoint = localPoint.normalized * distance;

                if (WheelKnob != null)
                    WheelKnob.anchoredPosition = clampedPoint;
            }
        }

        private void CalculateAngle(PointerEventData eventData)
        {
            if (WheelBackground != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    WheelBackground, eventData.position, _camera, out Vector2 localPoint))
            {
                _lastAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
            }
        }

        private void DetectRotation(PointerEventData eventData)
        {
            if (_rotationTimer > 0f) return;

            if (WheelBackground != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    WheelBackground, eventData.position, _camera, out Vector2 localPoint))
            {
                float currentAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
                float angleDiff = Mathf.DeltaAngle(_lastAngle, currentAngle);

                if (Mathf.Abs(angleDiff) >= RotationThreshold)
                {
                    if (angleDiff > 0)
                        OnRotateLeft?.Invoke();
                    else
                        OnRotateRight?.Invoke();

                    _lastAngle = currentAngle;
                    _rotationTimer = RotationCooldown;
                }

                float yComponent = localPoint.y;
                float xComponent = localPoint.x;

                if (Mathf.Abs(yComponent) > Mathf.Abs(xComponent))
                {
                    if (Mathf.Abs(yComponent) > WheelBackground.rect.width * 0.3f)
                    {
                        if (yComponent > 0)
                            OnRotateUp?.Invoke();
                        else
                            OnRotateDown?.Invoke();
                    }
                }
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
