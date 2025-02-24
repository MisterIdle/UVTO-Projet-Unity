using UnityEngine;

// Base class for collectible items
public class Collectible : MonoBehaviour
{
    // Method to be overridden for collecting the item
    public virtual void Collect() {}
}