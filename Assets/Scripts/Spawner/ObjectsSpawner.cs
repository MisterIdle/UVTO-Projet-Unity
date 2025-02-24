using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    // List of objects that can be spawned
    public List<SpawnableObject> spawnableObjects;
    private GlobalObjectSpawner globalObjectSpawner;
    // Angle range for spawning objects
    [Range(0, 360)] public float spawnAngle = 0;
    // Chance percentage to spawn an object
    [Range(0, 100)] public int spawnChance = 100;

    private void Awake()
    {
        // Get reference to the global object spawner
        globalObjectSpawner = GetComponentInParent<GlobalObjectSpawner>();
    }

    public void SpawnObject()
    {
        // Check if the spawn chance is met
        if (Random.Range(0, 100) > spawnChance)
        {
            Debug.Log("Spawn chance not met. No object spawned.");
            return;
        }

        // Check if there are objects to spawn
        if (spawnableObjects.Count == 0)
        {
            Debug.Log("No objects to spawn.");
            return;
        }

        // Select a random object to spawn
        var randomObject = spawnableObjects[Random.Range(0, spawnableObjects.Count)];

        // Check if the object can be spawned
        if (!globalObjectSpawner.CanSpawnObject(randomObject))
        {
            Debug.Log($"Object {randomObject.Name} has already been spawned. Skipping...");
            return;
        }

        // Instantiate the object
        GameObject obj = Instantiate(randomObject.Prefab, transform.position, transform.rotation);
        obj.transform.SetParent(globalObjectSpawner.ObjectsParent.transform);

        // Apply random rotation within the specified angle range
        obj.transform.rotation = Quaternion.Euler(0, Random.Range(-spawnAngle, spawnAngle), 0) * obj.transform.rotation;

        // Add Grabbable component and set its score value
        var grabbable = obj.AddComponent<Grabbable>();
        grabbable.ScoreValue = randomObject.Score;

        // Ensure the object has a Rigidbody component and set its mass
        var rigidbody = obj.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = obj.AddComponent<Rigidbody>();
        }
        rigidbody.mass = randomObject.Mass;

        // Set the object's name
        obj.name = randomObject.Name;

        // Mark the object as spawned in the global spawner
        globalObjectSpawner.MarkObjectAsSpawned(randomObject);

        Debug.Log($"Spawned: {randomObject.Name}");
    }
}
