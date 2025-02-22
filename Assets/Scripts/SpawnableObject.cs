using UnityEngine;

[CreateAssetMenu]
public class SpawnableObject : ScriptableObject
{
    public GameObject Prefab;
    public string Name;
    public int Score;
    public int Mass;
    [Range(0, 360)] public float RandomizeRotationChance;
    [Range(0, 2)] public float RandomizePositionChance;

}
