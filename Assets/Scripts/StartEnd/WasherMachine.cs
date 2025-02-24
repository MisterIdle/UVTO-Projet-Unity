using UnityEngine;

public class WasherMachine : MonoBehaviour
{
    public float RotationSpeed = 100f; // Speed of the washer's rotation
    public bool IsWashing = true; // Indicates if the washer is currently washing

    [SerializeField] private AudioClip _washerSound; // Sound played while washing
    [SerializeField] private AudioClip _washerEndSound; // Sound played when washing ends

    private void Start()
    {
        if (IsWashing)
        {
            // Play washing sound in a loop
            SoundManager.Instance.PlayLoopSound(_washerSound, transform, 1f);
        }
    }

    private void Update()
    {
        if (IsWashing)
        {
            // Rotate the washer
            transform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);
        }
        else
        {
            // Stop washing sound and play end sound
            SoundManager.Instance.StopSound(_washerSound);
            SoundManager.Instance.PlaySound(_washerEndSound, transform, 1f);
            enabled = false; // Disable the script
        }
    }
}
