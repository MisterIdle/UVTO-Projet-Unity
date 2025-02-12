using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    public Light flashlight;

    private void Start()
    {
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
        if (context.performed)
        {
            ToggleLight();
            Debug.Log("Flashlight toggled");
        }
    }
}
