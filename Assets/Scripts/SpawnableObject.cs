using UnityEngine;

[CreateAssetMenu]
public class SpawnableObject : ScriptableObject
{
    public GameObject Prefab;
    public int Score;
    public bool IsUnique;
    public bool IsMandatory;
    public bool CanBeBorrowed;
    [Range(0, 100)] public int BorrowedChance;
}
