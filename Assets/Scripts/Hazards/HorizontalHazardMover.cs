using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(HazardDamage))]
public class HorizontalHazardMover : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 2f;
    [Tooltip("Distance to travel on each side of the starting position.")]
    [SerializeField, Min(0f)] private float travelDistance = 3f;

    private Rigidbody2D body;
    private Vector2 startPosition;
    private int direction = 1;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        startPosition = body.position;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (speed <= 0f || travelDistance <= 0f)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        float limit = startPosition.x + direction * travelDistance;
        float nextX = Mathf.MoveTowards(body.position.x, limit, speed * Time.fixedDeltaTime);
        body.MovePosition(new Vector2(nextX, startPosition.y));
        if (nextX == limit) direction = -direction;
    }

    private void OnDisable()
    {
        if (body != null) body.linearVelocity = Vector2.zero;
    }
}
