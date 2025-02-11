using UnityEngine;

public class Interactive : MonoBehaviour
{
    public bool IsActivated { get; protected set; }

    public virtual void Interact() {}
}
