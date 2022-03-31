using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy Settings/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public EnemyType Type;
    public GameObject Prefab;
    public int DifficultyPoints;
    public int Size;
}
