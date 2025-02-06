using UnityEngine;

public class Switch : Interactive
{
    public Light[] lights;
    public bool IsOn;

    private void Start()
    {
        TurnOffAllLights();
    }

    public override void Interact()
    {
        ToggleLights();
    }

    public void ToggleLights()
    {
        if (lights == null || lights.Length == 0)
        {
            Debug.LogWarning("No lights assigned to the switch.");
            return;
        }

        foreach (var light in lights)
        {
            if (light != null)
            {
                light.enabled = !light.enabled;
            }
            else
            {
                Debug.LogWarning("A light in the array is null.");
            }
        }

        IsOn = !IsOn;
    }


    private void TurnOffAllLights()
    {
        if (lights == null || lights.Length == 0)
        {
            Debug.LogWarning("No lights assigned to the switch.");
            return;
        }

        foreach (var light in lights)
        {
            if (light != null)
            {
                light.enabled = false;
            }
            else
            {
                Debug.LogWarning("A light in the array is null.");
            }
        }
    }
}
