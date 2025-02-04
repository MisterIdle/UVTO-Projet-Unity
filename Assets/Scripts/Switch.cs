using UnityEngine;

public class Switch : Interactive
{
    [SerializeField] private Light[] _light;

    public override void Interact()
    {
        ToggleLight();
    }

    private void ToggleLight()
    {
        foreach (var light in _light)
        {
            light.enabled = !light.enabled;
        }
    }
}
