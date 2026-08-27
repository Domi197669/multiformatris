using UnityEngine;

namespace Multiformatris.Presentation
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        public Vector3 offset = new Vector3(0, 8, -12);

        [Header("Rotation")]
        public float rotationSpeed = 2f;
        public float minAngle = 10f;
        public float maxAngle = 80f;

        private float _currentAngleX = 0f;
        private float _currentAngleY = 25f;

        private void Start()
        {
            UpdatePosition();
        }

        private void LateUpdate()
        {
            HandleCameraRotation();
            UpdatePosition();
        }

        private void HandleCameraRotation()
        {
            if (UnityEngine.Input.GetMouseButton(1))
            {
                _currentAngleX += UnityEngine.Input.GetAxis("Mouse X") * rotationSpeed;
                _currentAngleY -= UnityEngine.Input.GetAxis("Mouse Y") * rotationSpeed;
                _currentAngleY = Mathf.Clamp(_currentAngleY, minAngle, maxAngle);
            }
        }

        private void UpdatePosition()
        {
            Vector3 targetPos = target != null ? target.position : Vector3.zero;

            Quaternion rotation = Quaternion.Euler(_currentAngleY, _currentAngleX, 0);
            Vector3 position = targetPos + rotation * offset;

            transform.position = position;
            transform.LookAt(targetPos);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            UpdatePosition();
        }

        public void RecalculatePosition()
        {
            UpdatePosition();
        }

        public void FitToGrid(float cellSize, int gridWidth, int gridHeight, int gridDepth,
            Vector3 defaultOffset)
        {
            float horizontalExtent = Mathf.Max(gridWidth, gridDepth) * cellSize;
            float verticalExtent = gridHeight * cellSize;

            Camera cam = Camera.main;
            float fovVertical = cam != null ? cam.fieldOfView : 50f;
            float aspect = (float)Screen.width / Mathf.Max(1f, Screen.height);
            float tanHalf = Mathf.Tan(fovVertical * 0.5f * Mathf.Deg2Rad);

            float safety = 1.4f;
            float distByWidth = (horizontalExtent * safety) / (2f * tanHalf * Mathf.Max(aspect, 0.1f));
            float distByHeight = (verticalExtent * safety) / (2f * tanHalf);

            float distance = Mathf.Max(distByWidth, distByHeight);
            distance = Mathf.Clamp(distance, 8f, 45f);

            Vector3 normalized = defaultOffset.normalized;
            if (normalized.sqrMagnitude < 0.0001f) normalized = new Vector3(0, 0.5f, -1).normalized;

            offset = normalized * distance;
            UpdatePosition();
        }
    }
}
