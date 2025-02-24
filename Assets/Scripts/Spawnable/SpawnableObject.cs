using UnityEngine;

[CreateAssetMenu] // Allows creation of this scriptable object from the Unity editor
public class SpawnableObject : ScriptableObject
{
    public GameObject Prefab; // The prefab to spawn
    public string Name; // The name of the spawnable object
    public int Score; // The score associated with the object
    public int Mass; // The mass of the object
}
