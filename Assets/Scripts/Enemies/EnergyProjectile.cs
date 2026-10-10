using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnergyProjectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float speed = 7f;
    [SerializeField, Min(0.1f)] private float lifetime = 2.5f;
    private Rigidbody2D body;
    private Transform owner;
    private Vector2 direction;
    private float remaining;
    private bool spent;
    private ContactFilter2D filter;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        filter = new ContactFilter2D().NoFilter();
        filter.useTriggers = false;
        remaining = lifetime;
    }
    public void Initialize(Transform source, Vector2 travelDirection)
    {
        owner = source;
        direction = travelDirection.normalized;
    }
    private void FixedUpdate()
    {
        if (spent) return;
        remaining -= Time.fixedDeltaTime;
        if (remaining <= 0f) { Finish(); return; }
        float distance = speed * Time.fixedDeltaTime;
        int count = body.Cast(direction, filter, hits, distance);
        for (int i = 0; i < count; i++) if (Hit(hits[i].collider)) return;
        body.MovePosition(body.position + direction * distance);
    }
    private void OnTriggerEnter2D(Collider2D other) => Hit(other);
    private bool Hit(Collider2D other)
    {
        if (spent || other == null || (owner != null && other.transform.IsChildOf(owner))) return false;
        var player = other.GetComponentInParent<PlayerHealth>();
        if (player != null) { player.TakeDamage(1); Finish(); return true; }
        if (!other.isTrigger) { Finish(); return true; }
        return false;
    }
    private void Finish() { if (spent) return; spent = true; body.simulated = false; Destroy(gameObject); }
}
