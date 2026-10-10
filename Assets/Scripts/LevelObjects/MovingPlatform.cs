using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
[DefaultExecutionOrder(-100)]
public class MovingPlatform : MonoBehaviour
{
    [Tooltip("Two endpoints relative to this object's initial world position.")]
    [SerializeField] private Vector2 pointA;
    [SerializeField] private Vector2 pointB = new Vector2(6f, 0f);
    [SerializeField, Min(0.1f)] private float speed = 2.5f;
    [SerializeField, Min(0f)] private float endpointPause = 0.2f;
    private Rigidbody2D body;
    private BoxCollider2D surface;
    private Vector2 origin;
    private bool toB = true;
    private float waitUntil;
    public Vector2 Velocity { get; private set; }
    private readonly List<Rider> riders = new List<Rider>(2);
    private struct Rider { public Rigidbody2D body; public Collider2D collider; public PlayerHealth health; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        surface = GetComponent<BoxCollider2D>();
        origin = body.position;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.position = origin + pointA;
    }

    private void FixedUpdate()
    {
        if (Time.time < waitUntil) { Velocity = Vector2.zero; body.linearVelocity = Vector2.zero; return; }
        Vector2 target = origin + (toB ? pointB : pointA);
        Vector2 next = Vector2.MoveTowards(body.position, target, speed * Time.fixedDeltaTime);
        Vector2 delta = next - body.position;
        Velocity = delta / Time.fixedDeltaTime;
        // Add only the deck's displacement; the controller retains its input velocity.
        // Do not parent the player, overwrite its velocity, or carry it after jumping/respawning.
        for (int i = riders.Count - 1; i >= 0; i--)
        {
            Rider rider = riders[i];
            if (rider.body == null || rider.health == null || !rider.health.isActiveAndEnabled ||
                !rider.body.simulated || rider.body.linearVelocity.y > Mathf.Max(0.5f, Velocity.y + 0.5f) || !IsOnTop(rider.collider))
            { riders.RemoveAt(i); continue; }
            rider.body.position += delta;
        }
        body.MovePosition(next);
        if (next == target) { toB = !toB; waitUntil = Time.time + endpointPause; }
    }

    private bool IsOnTop(Collider2D other)
    {
        if (other == null) return false;
        Bounds a = surface.bounds, b = other.bounds;
        return b.max.x > a.min.x && b.min.x < a.max.x &&
            Mathf.Abs(b.min.y - a.max.y) < 0.18f;
    }
    private void Update()
    {
        if (Time.timeScale == 0f || Mathf.Abs(Velocity.y) < 0.01f) return;
        // Physics can impart the lift's normal velocity to a resting rider. Remove
        // that carried velocity after physics, before PlayerMovement.Update:
        // the deck already transports its
        // position, and the unchanged controller must still see a resting body
        // as grounded for buffered/coyote jumping. Never cancel a real jump.
        for (int i = 0; i < riders.Count; i++)
        {
            Rider rider = riders[i];
            if (rider.body == null || rider.health == null || !rider.health.isActiveAndEnabled ||
                !rider.body.simulated || !IsOnTop(rider.collider)) continue;
            Vector2 velocity = rider.body.linearVelocity;
            if (Mathf.Abs(velocity.y - Velocity.y) < 0.15f)
            {
                velocity.y -= Velocity.y;
                rider.body.linearVelocity = velocity;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) => AddRider(collision);
    private void OnCollisionStay2D(Collision2D collision) => AddRider(collision);
    private void AddRider(Collision2D collision)
    {
        if (collision.rigidbody == null || !IsOnTop(collision.collider)) return;
        for (int i = 0; i < riders.Count; i++) if (riders[i].body == collision.rigidbody) return;
        var health = collision.rigidbody.GetComponent<PlayerHealth>();
        if (health != null) riders.Add(new Rider { body = collision.rigidbody, collider = collision.collider, health = health });
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        for (int i = riders.Count - 1; i >= 0; i--)
            if (riders[i].body == collision.rigidbody) riders.RemoveAt(i);
    }
    private void OnDisable() { riders.Clear(); Velocity = Vector2.zero; if (body != null) body.linearVelocity = Vector2.zero; }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = Application.isPlaying ? (Vector3)origin : transform.position;
        Gizmos.DrawLine(start + (Vector3)pointA, start + (Vector3)pointB);
    }
}
