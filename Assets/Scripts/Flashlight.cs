using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private Light _light;

    private void Start()
    {
        _light.enabled = false;
    }

    public void ToggleLight()
    {
        _light.enabled = !_light.enabled;
    }

    public void OnFlashlight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleLight();
        }
    }
}
