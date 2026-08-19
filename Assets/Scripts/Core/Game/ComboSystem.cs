using UnityEngine;
using System;

namespace Multiformatris.Core.Game
{
    public class ComboSystem : MonoBehaviour
    {
        [Header("Combo Settings")]
        public int ComboThreshold = 2;
        public float ComboTimeWindow = 2.0f;
        public float ComboMultiplierIncrement = 0.5f;

        [Header("Bonus Points")]
        public int SingleLinePoints = 100;
        public int DoubleLinePoints = 300;
        public int TripleLinePoints = 500;
        public int TetrisPoints = 800;

        private int _currentCombo;
        private float _lastClearTime;
        private float _currentMultiplier;

        public int CurrentCombo => _currentCombo;
        public float CurrentMultiplier => _currentMultiplier;

        public event Action<int, float> OnComboUpdated;
        public event Action OnComboBroken;

        private void Awake()
        {
            ResetCombo();
        }

        public int CalculatePoints(int linesCleared, int level)
        {
            int basePoints = GetBasePoints(linesCleared);
            int levelMultiplier = level;

            UpdateCombo(linesCleared);

            float comboBonus = 1f;
            if (_currentCombo >= ComboThreshold)
            {
                comboBonus = 1f + (_currentCombo - ComboThreshold + 1) * ComboMultiplierIncrement;
            }

            int totalPoints = Mathf.RoundToInt(basePoints * levelMultiplier * comboBonus);
            return totalPoints;
        }

        private int GetBasePoints(int linesCleared)
        {
            switch (linesCleared)
            {
                case 1: return SingleLinePoints;
                case 2: return DoubleLinePoints;
                case 3: return TripleLinePoints;
                case 4: return TetrisPoints;
                default: return linesCleared * 200;
            }
        }

        private void UpdateCombo(int linesCleared)
        {
            float currentTime = Time.time;

            if (currentTime - _lastClearTime <= ComboTimeWindow)
            {
                _currentCombo++;
            }
            else
            {
                _currentCombo = 1;
            }

            _lastClearTime = currentTime;
            _currentMultiplier = 1f + (_currentCombo - 1) * ComboMultiplierIncrement;

            OnComboUpdated?.Invoke(_currentCombo, _currentMultiplier);
        }

        public void ResetCombo()
        {
            _currentCombo = 0;
            _currentMultiplier = 1f;
            _lastClearTime = -ComboTimeWindow;
        }

        public string GetComboText()
        {
            if (_currentCombo < ComboThreshold)
                return "";

            return $"COMBO x{_currentCombo}!";
        }

        public float GetComboMultiplier()
        {
            return _currentMultiplier;
        }
    }
}
