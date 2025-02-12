using UnityEngine;

[CreateAssetMenu]
public class SpawnableObject : ScriptableObject
{
    public GameObject Prefab;
    public string Name;
    public int Score;
    public bool IsUnique;
    public bool IsMandatory;
    public bool CanBeBorrowed;
    [Range(0, 100)] public int BorrowedChance;
    public bool HasRandomRotationSpawn;
    [Range(0, 360)] public int RotationChance;
    public bool HasRandomPositionSpawn;
    [Range(0, 5)] public float PositionChance;
}
