using UnityEngine;

[CreateAssetMenu]
public class SpawnableObject : ScriptableObject
{
    public GameObject Prefab;
    public string Name;
    public int Score;
    public int Mass;

    public bool IsUniqueGlobal;
    public bool IsUniquePerRoom;

    public bool CanBeBorrowed;
    [Range(0, 100)] public int BorrowedChance;

    private void OnValidate()
    {
        if (IsUniqueGlobal && IsUniquePerRoom)
        {
            if (IsUniqueGlobal) 
                IsUniquePerRoom = false;
            else 
                IsUniqueGlobal = false;
        }
    }

}
