using System.Collections;
using UnityEngine;

public class Switch : Interactive
{
    public Light[] lights;

    private void Start()
    {
        IsActivated = false;
        
        foreach (Light light in lights)
        {
            light.enabled = IsActivated;
        }
    }

    public override void Interact()
    {
        if (IsActivated == !IsActivated)
            return;

        IsActivated = !IsActivated;
        StartCoroutine(ToggleLights(IsActivated));
    }

    private IEnumerator ToggleLights(bool on)
    {
        foreach (Light light in lights)
        {
            light.enabled = on;
        }

        yield return null;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (Light light in lights)
        {
            Gizmos.DrawLine(transform.position, light.transform.position);
        }
    }

}
