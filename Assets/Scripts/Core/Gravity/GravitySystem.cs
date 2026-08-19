using UnityEngine;
using System;

namespace Multiformatris.Core.Gravity
{
    [CreateAssetMenu(fileName = "GravitySystem", menuName = "Multiformatris/Gravity System")]
    public class GravityConfig : ScriptableObject
    {
        [Header("Gravity Directions")]
        public Vector3Int[] GravitySequence = new Vector3Int[]
        {
            Vector3Int.down,    // Niveles 1-3: arriba a abajo
            Vector3Int.up,      // Niveles 4-6: abajo a arriba
            Vector3Int.right,   // Niveles 7-9: izquierda a derecha
            Vector3Int.left     // Niveles 10-12: derecha a izquierda
        };

        [Header("Speed Curve")]
        public float BaseSpeed = 1.0f;
        public float SpeedIncrementPerLevel = 0.15f;
        public float MaxSpeed = 10f;

        public float GetSpeedForLevel(int level)
        {
            float speed = BaseSpeed + (level - 1) * SpeedIncrementPerLevel;
            return Mathf.Min(speed, MaxSpeed);
        }

        public Vector3Int GetGravityForLevel(int level)
        {
            int index = (level - 1) / 3 % GravitySequence.Length;
            return GravitySequence[index];
        }

        public int GetLayersPerLevel()
        {
            return 10;
        }

        public float GetDropInterval(int level)
        {
            return 1f / GetSpeedForLevel(level);
        }
    }
}
