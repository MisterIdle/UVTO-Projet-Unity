using UnityEngine;

public class Door : Interactive
{
    public GameObject frame;

    public override void Interact()
    {
        if (frame.transform.rotation.y == 0)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    public void Open()
    {
        frame.transform.Rotate(0, 90, 0);
    }

    public void Close()
    {
        frame.transform.Rotate(0, -90, 0);
    }
}
