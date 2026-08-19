using UnityEngine;
using System.Collections.Generic;
using Multiformatris.Core.Gravity;

namespace Multiformatris.Core.Levels
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Settings")]
        public int StartLevel = 1;
        public int LinesPerLevel = 10;
        public GravityConfig GravityConfig;

        [Header("Level Configs")]
        public List<LevelConfig> LevelConfigs;

        private int _currentLevel;
        private int _totalLinesCleared;

        public int CurrentLevel => _currentLevel;
        public int TotalLinesCleared => _totalLinesCleared;

        public event System.Action<int> OnLevelChanged;

        private void Awake()
        {
            _currentLevel = StartLevel;
        }

        public void AddLinesCleared(int count)
        {
            _totalLinesCleared += count;

            int newLevel = StartLevel + (_totalLinesCleared / LinesPerLevel);

            if (newLevel > _currentLevel)
            {
                _currentLevel = newLevel;
                OnLevelChanged?.Invoke(_currentLevel);
            }
        }

        public Vector3Int GetCurrentGravity()
        {
            if (GravityConfig != null)
                return GravityConfig.GetGravityForLevel(_currentLevel);

            return Vector3Int.down;
        }

        public float GetCurrentSpeed()
        {
            if (GravityConfig != null)
                return GravityConfig.GetSpeedForLevel(_currentLevel);

            return 1.0f;
        }

        public float GetDropInterval()
        {
            if (GravityConfig != null)
                return GravityConfig.GetDropInterval(_currentLevel);

            return 1.0f;
        }

        public LevelConfig GetLevelConfig()
        {
            if (LevelConfigs == null || LevelConfigs.Count == 0)
                return null;

            int index = (_currentLevel - 1) % LevelConfigs.Count;
            return LevelConfigs[index];
        }

        public void Reset()
        {
            _currentLevel = StartLevel;
            _totalLinesCleared = 0;
        }
    }
}
