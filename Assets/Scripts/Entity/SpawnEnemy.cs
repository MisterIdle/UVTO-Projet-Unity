using System.Collections;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    private Animation _anim;
    public GameObject enemy;
    public Transform spawnPoint;

    private void Start()
    {
        // Get the Animation component attached to this GameObject
        _anim = GetComponent<Animation>();

        // Start the spawning process
        Spawn();
    }

    public void Spawn()
    {
        // Play the animation
        _anim.Play();
        
        // Wait for the animation to finish before spawning the enemy
        StartCoroutine(WaitForAnimation());
    }

    private IEnumerator WaitForAnimation()
    {
        // Wait until the animation is no longer playing
        while (_anim.isPlaying)
        {
            yield return null;
        }
        
        // Instantiate the enemy at the spawn point
        Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);
    }

}
