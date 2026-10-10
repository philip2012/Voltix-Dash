using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class LaunchPad : MonoBehaviour
{
    [SerializeField, Min(1f)] private float upwardVelocity = 25f;
    [SerializeField] private float horizontalVelocity;
    [SerializeField] private Animator visual;
    private Collider2D area;
    private readonly HashSet<PlayerHealth> launched = new HashSet<PlayerHealth>();
    public static event System.Action<Vector3> Launched;
    public float UpwardVelocity => upwardVelocity;

    private void Awake() => area = GetComponent<Collider2D>();
    private void OnTriggerEnter2D(Collider2D other) => TryLaunch(other);
    private void OnTriggerStay2D(Collider2D other) => TryLaunch(other);
    private void OnTriggerExit2D(Collider2D other)
    {
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) launched.Remove(health);
    }
    private void OnDisable() => launched.Clear();

    private void TryLaunch(Collider2D other)
    {
        if (Time.timeScale == 0f || !isActiveAndEnabled) return;
        var health = other.GetComponentInParent<PlayerHealth>();
        var body = other.attachedRigidbody;
        if (health == null || !health.isActiveAndEnabled || body == null || !body.simulated ||
            body.linearVelocity.y > 0.1f || other.bounds.min.y < area.bounds.min.y - 0.15f ||
            !launched.Add(health)) return;
        // A velocity impulse changes neither the controller nor its gravity/ground tuning.
        body.linearVelocity = new Vector2(horizontalVelocity == 0f ? body.linearVelocity.x : horizontalVelocity,
            upwardVelocity);
        if (visual != null) visual.Play("Activation", 0, 0f);
        Launched?.Invoke(transform.position);
    }
}
