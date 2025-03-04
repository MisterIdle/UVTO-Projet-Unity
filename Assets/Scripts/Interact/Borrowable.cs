using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody), typeof(MeshCollider), typeof(NavMeshObstacle))]
public class Borrowable : Collectible
{
    private PlayerController _playerController;
    public float ScoreValue;
    private Renderer _renderer;
    private MeshCollider _meshCollider;
    private NavMeshObstacle _navMeshObstacle;
    private Rigidbody _rigidbody;
    public bool IsBorrowed = false;

    private void Awake()
    {
        // Initialize components
        _renderer = GetComponent<Renderer>();
        _meshCollider = GetComponent<MeshCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerController = FindFirstObjectByType<PlayerController>();
        _navMeshObstacle = GetComponent<NavMeshObstacle>();

        // Configure components
        _meshCollider.convex = true;
        _navMeshObstacle.carving = true;
    }

    public override void Collect()
    {
        // Mark as borrowed and update score
        IsBorrowed = true;
        _playerController.AddScore(ScoreValue);
        SoundManager.Instance.PlaySound(_playerController.BorrowSound, transform, 1f);
        StartCoroutine(DisappearAnimation());
    }

    public void UpdateScore(float score)
    {
        // Update the score value
        ScoreValue = score;
    }

    private IEnumerator DisappearAnimation()
    {
        // Animate disappearance
        float duration = 0.5f;
        float time = 0;
        Vector3 initialScale = transform.localScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, time / duration);
            float scale = Mathf.Lerp(1, 0.5f, time / duration);

            if (_renderer != null)
            {
                Color color = _renderer.material.color;
                _renderer.material.color = new Color(color.r, color.g, color.b, alpha);
            }

            transform.localScale = initialScale * scale;
            yield return null;
        }

        // Deactivate the object
        gameObject.SetActive(false);
    }
}
