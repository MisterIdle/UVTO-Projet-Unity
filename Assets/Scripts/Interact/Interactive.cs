using UnityEngine;

public class Interactive : MonoBehaviour
{
    // Indicates if the object is activated
    public bool IsActivated;

    // Indicates if the object should ignore interactions from bots
    public bool IgnoreBot;

    // Virtual method to handle interaction logic
    public virtual void Interact() {}
}
