using UnityEngine;

namespace Multiformatris.Core.Levels
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Multiformatris/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Info")]
        public int LevelNumber = 1;
        public string LevelName = "Level 1";

        [Header("Grid")]
        public int GridWidth = 5;
        public int GridHeight = 10;
        public int GridDepth = 5;

        [Header("Gravity")]
        public Vector3Int GravityDirection = Vector3Int.down;
        public float DropSpeed = 1.0f;
        public float LockDelay = 0.5f;

        [Header("Difficulty")]
        public int LinesToNextLevel = 10;
        public float SpeedMultiplier = 1.0f;

        [Header("Visual")]
        public Color WellTint = Color.white;
        public Material BackgroundMaterial;

        [Header("Audio")]
        public AudioClip BackgroundMusic;
        public float MusicVolume = 0.5f;
    }
}
