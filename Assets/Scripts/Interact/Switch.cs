using System.Collections;
using UnityEngine;

public class Switch : Interactive
{
    [SerializeField] private AudioClip _switchSoundOn;
    [SerializeField] private AudioClip _switchSoundOff;

    public Light[] lights;

    private void Start()
    {
        IsActivated = false; // Initialize switch state to off
        
        // Set all lights to the initial state
        foreach (Light light in lights)
        {
            light.enabled = IsActivated;
        }
    }

    public override void Interact()
    {
        // Toggle switch state
        if (IsActivated == !IsActivated)
            return;

        // Play appropriate sound based on switch state
        SoundManager.Instance.PlaySound(IsActivated ? _switchSoundOff : _switchSoundOn, transform, 0.5f);

        IsActivated = !IsActivated; // Update switch state
        StartCoroutine(ToggleLights(IsActivated)); // Toggle lights
    }

    private IEnumerator ToggleLights(bool on)
    {
        // Enable or disable lights based on switch state
        foreach (Light light in lights)
        {
            light.enabled = on;
        }

        yield return null; // Coroutine requirement
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; // Set gizmo color
        // Draw lines from switch to each light in the editor
        foreach (Light light in lights)
        {
            Gizmos.DrawLine(transform.position, light.transform.position);
        }
    }
}
