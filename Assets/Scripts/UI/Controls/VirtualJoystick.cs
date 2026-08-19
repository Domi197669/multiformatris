using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Multiformatris.UI.Controls
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Settings")]
        public float DeadZone = 0.2f;
        public float MaxDistance = 100f;

        [Header("References")]
        public RectTransform Background;
        public RectTransform Knob;

        private Vector2 _inputVector = Vector2.zero;
        private Canvas _canvas;
        private Camera _camera;

        public Vector2 InputVector => _inputVector;
        public float Horizontal => _inputVector.x;
        public float Vertical => _inputVector.y;

        public event System.Action<Vector2Int> OnDirectionChanged;

        private Vector2Int _lastDirection;
        private float _directionCooldown = 0.15f;
        private float _directionTimer;

        private void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
                _camera = _canvas.worldCamera;

            if (Background == null)
                Background = GetComponent<RectTransform>();
        }

        private void Update()
        {
            _directionTimer -= Time.deltaTime;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Background != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    Background, eventData.position, _camera, out Vector2 localPoint))
            {
                UpdateKnob(localPoint);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Background != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    Background, eventData.position, _camera, out Vector2 localPoint))
            {
                UpdateKnob(localPoint);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _inputVector = Vector2.zero;
            if (Knob != null)
                Knob.anchoredPosition = Vector2.zero;

            _lastDirection = Vector2Int.zero;
        }

        private void UpdateKnob(Vector2 localPoint)
        {
            float distance = localPoint.magnitude;

            if (distance > MaxDistance)
            {
                localPoint = localPoint.normalized * MaxDistance;
                distance = MaxDistance;
            }

            _inputVector = localPoint / MaxDistance;

            if (_inputVector.magnitude < DeadZone)
            {
                _inputVector = Vector2.zero;
                _lastDirection = Vector2Int.zero;
            }
            else
            {
                DetectDirection();
            }

            if (Knob != null)
                Knob.anchoredPosition = localPoint;
        }

        private void DetectDirection()
        {
            if (_directionTimer > 0f) return;

            Vector2Int newDirection = Vector2Int.zero;

            float angle = Mathf.Atan2(_inputVector.y, _inputVector.x) * Mathf.Rad2Deg;

            if (angle >= -22.5f && angle < 22.5f)
                newDirection = Vector2Int.right;
            else if (angle >= 22.5f && angle < 67.5f)
                newDirection = new Vector2Int(1, 1);
            else if (angle >= 67.5f && angle < 112.5f)
                newDirection = Vector2Int.up;
            else if (angle >= 112.5f && angle < 157.5f)
                newDirection = new Vector2Int(-1, 1);
            else if (angle >= 157.5f || angle < -157.5f)
                newDirection = Vector2Int.left;
            else if (angle >= -157.5f && angle < -112.5f)
                newDirection = new Vector2Int(-1, -1);
            else if (angle >= -112.5f && angle < -67.5f)
                newDirection = Vector2Int.down;
            else if (angle >= -67.5f && angle < -22.5f)
                newDirection = new Vector2Int(1, -1);

            if (newDirection != _lastDirection && newDirection != Vector2Int.zero)
            {
                _lastDirection = newDirection;
                _directionTimer = _directionCooldown;
                OnDirectionChanged?.Invoke(newDirection);
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
