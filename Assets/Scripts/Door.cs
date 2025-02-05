using UnityEngine;
using System.Collections;

public class Door : Interactive
{
    public GameObject frame;
    public float openDuration = 1.0f;
    public float moveDistance = 0.5f; // Distance to move the door when opening/closing

    private bool isOpen = false;
    private bool isAnimating = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine currentCoroutine;

    private void Start()
    {
        closedRotation = frame.transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, 90, 0);
        closedPosition = frame.transform.position;
        openPosition = closedPosition + frame.transform.right * moveDistance;
    }

    public override void Interact()
    {
        if (isAnimating)
        {
            StopCoroutine(currentCoroutine);
            isAnimating = false;
        }

        if (isOpen)
        {
            currentCoroutine = StartCoroutine(MoveAndRotateDoor(closedRotation, closedPosition));
        }
        else
        {
            currentCoroutine = StartCoroutine(MoveAndRotateDoor(openRotation, openPosition));
        }
        isOpen = !isOpen;
    }

    private IEnumerator MoveAndRotateDoor(Quaternion targetRotation, Vector3 targetPosition)
    {
        isAnimating = true;
        Quaternion startRotation = frame.transform.rotation;
        Vector3 startPosition = frame.transform.position;
        float elapsedTime = 0;

        while (elapsedTime < openDuration)
        {
            frame.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / openDuration);
            frame.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / openDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        frame.transform.rotation = targetRotation;
        frame.transform.position = targetPosition;
        isAnimating = false;
    }
}
