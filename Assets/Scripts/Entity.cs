using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
        {
            if (hit.collider.TryGetComponent(out Grabbable grabbable))
            {
                if (grabbable.CanBeBorrowed)
                {
                    grabbable.Borrow();
                }
            }
        }
    }
}
