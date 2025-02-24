using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    // Reference to the Light component
    public Light flashlight;
    // Reference to the PlayerController script
    private PlayerController PlayerController;
    // Audio clip to play when the flashlight is toggled
    [SerializeField] AudioClip flashlightSound;

    private void Start()
    {
        // Get the PlayerController component from the parent object
        PlayerController = GetComponentInParent<PlayerController>();

        // Ensure the flashlight is initially turned off
        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
    }

    // Method to toggle the flashlight on and off
    public void ToggleLight()
    {
        if (flashlight != null)
        {
            flashlight.enabled = !flashlight.enabled;
            // Play the flashlight toggle sound
            SoundManager.Instance.PlaySound(flashlightSound, transform, 0.5f);
        }
    }

    // Input action callback for toggling the flashlight
    public void OnFlashlight(InputAction.CallbackContext context)
    {
        // Check if the action was performed and the player is not dead
        if (context.performed && !PlayerController.IsDead)
        {
            ToggleLight();
            Debug.Log("Flashlight toggled");
        }
    }
}
