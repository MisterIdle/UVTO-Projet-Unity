using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    public Light flashlight;
    private PlayerController PlayerController;
    [SerializeField] AudioClip flashlightSound;

    private void Start()
    {
        PlayerController = GetComponentInParent<PlayerController>();

        if (flashlight != null)
        {
            flashlight.enabled = false;
        }
    }

    public void ToggleLight()
    {
        if (flashlight != null)
        {
            flashlight.enabled = !flashlight.enabled;
            SoundManager.Instance.PlaySound(flashlightSound, transform, 0.5f);
        }
    }

    public void OnFlashlight(InputAction.CallbackContext context)
    {
        if (context.performed && !PlayerController.IsDead)
        {
            ToggleLight();
            Debug.Log("Flashlight toggled");
        }
    }
}
