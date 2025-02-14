using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    public Light flashlight;
    public PlayerController PlayerController;

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
