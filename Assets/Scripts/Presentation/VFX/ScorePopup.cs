using UnityEngine;
using UnityEngine.UI;

namespace Multiformatris.Presentation.VFX
{
    public class ScorePopup : MonoBehaviour
    {
        [Header("Settings")]
        public float FloatSpeed = 2f;
        public float FadeDuration = 1f;
        public float ScaleMultiplier = 1.5f;

        [Header("References")]
        public Text ScoreText;

        private Color _originalColor;
        private float _timer;
        private bool _isActive;

        private void Awake()
        {
            if (ScoreText != null)
                _originalColor = ScoreText.color;
        }

        private void Update()
        {
            if (!_isActive) return;

            _timer += Time.deltaTime;

            transform.position += Vector3.up * FloatSpeed * Time.deltaTime;

            if (ScoreText != null)
            {
                float alpha = 1f - (_timer / FadeDuration);
                Color c = _originalColor;
                c.a = alpha;
                ScoreText.color = c;
            }

            float scale = 1f + (_timer / FadeDuration) * (ScaleMultiplier - 1f);
            transform.localScale = Vector3.one * scale;

            if (_timer >= FadeDuration)
            {
                gameObject.SetActive(false);
                _isActive = false;
            }
        }

        public void Show(Vector3 position, int points, string suffix = "")
        {
            transform.position = position;
            _timer = 0f;
            _isActive = true;

            if (ScoreText != null)
            {
                ScoreText.text = $"+{points:N0}{suffix}";
                ScoreText.color = _originalColor;
            }

            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        public void ShowCombo(Vector3 position, int comboCount)
        {
            string comboText = comboCount switch
            {
                2 => "DOUBLE!",
                3 => "TRIPLE!",
                4 => "TETRIS!",
                _ => $"COMBO x{comboCount}!"
            };

            Show(position, 0, comboText);
        }

        private void OnDisable()
        {
            _isActive = false;
        }
    }
}
