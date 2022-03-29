using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomSettings", menuName = "Enemy Settings/Enemy Room Settings")]
public class EnemyRoomSettings : ScriptableObject
{
    public SpawnRate[] Enemies;

    public int MinimumRoomSize;
    public int MaximumRoomSize;

    public int MinimumDifficultyPoints;
    public int DifficultyVariety;

    public EnemyType MaxEnemyDifficulty;

    [Serializable]
    public struct SpawnRate
    {
        public EnemyType Type;

        [Range(0, 1)]
        public float Rate;
    }
}
