using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(HazardDamage))]
public class PatrolEnemy : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 2f;
    [Tooltip("Horizontal distance to each side of the starting position, in world units.")]
    [SerializeField, Min(0f)] private float patrolDistance = 3f;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private float startingX;
    private int direction = 1;

    public int FacingDirection => direction;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startingX = body.position.x;

        // This simple flat-platform patrol follows a fixed route and cannot be pushed.
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (speed <= 0f || patrolDistance <= 0f)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        float limit = startingX + direction * patrolDistance;
        float nextX = Mathf.MoveTowards(body.position.x, limit, speed * Time.fixedDeltaTime);
        body.MovePosition(new Vector2(nextX, body.position.y));

        // MoveTowards lands exactly on the limit, including when a step would overshoot.
        if (nextX == limit)
        {
            direction = -direction;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0;
        }
    }

    private void OnDisable()
    {
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        patrolDistance = Mathf.Max(0f, patrolDistance);
    }
}
