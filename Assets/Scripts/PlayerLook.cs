using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    private Camera _camera;

    private void Start() {
        _camera = Camera.main;
    }

    private void Update() {
        if (_camera != null) {
            transform.rotation = Quaternion.Euler(_camera.transform.eulerAngles.x, _camera.transform.eulerAngles.y, 0);
        }
    }
}
