using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Animator visual;
    public bool IsActivated { get; private set; }
    public static event System.Action<Vector3> Activated;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsActivated || Time.timeScale == 0f) return;
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.isActiveAndEnabled) return;
        health.SetRespawnPosition(respawnPoint != null ? respawnPoint.position : transform.position);
        IsActivated = true;
        if (visual != null) visual.Play("Active", 0, 0f);
        Activated?.Invoke(transform.position);
    }
}
