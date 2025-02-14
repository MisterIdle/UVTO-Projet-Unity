using Unity.Cinemachine;
using UnityEngine;

public class CameraPlayer : MonoBehaviour
{
    private CinemachineCamera _cineMachineCamera;
    private PlayerController _playerController;

    private void Start()
    {
        _cineMachineCamera = GetComponent<CinemachineCamera>();
        _playerController = FindFirstObjectByType<PlayerController>();

        SetFollowTarget(_playerController.CameraHeadTransform);
    }

    private void Update()
    {
        if (_playerController.IsDead)
        {
            SetFollowTarget(_playerController.DeathCameraTransform);
        }
    }

    public void SetFollowTarget(Transform target)
    {
        _cineMachineCamera.Follow = target;
    }
}
