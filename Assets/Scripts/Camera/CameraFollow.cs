using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetBody;

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private float verticalOffset = 0.5f;

    [Header("Dead Zone")]
    [SerializeField] private float horizontalDeadZone = 0.8f;
    [SerializeField] private float verticalDeadZone = 0.75f;

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 1.5f;
    [SerializeField] private float lookAheadSmoothTime = 0.1f;

    [Header("Bounds")]
    [SerializeField] private BoxCollider2D cameraBounds;

    private Camera cam;

    private Vector3 followVelocity;
    private float currentLookAhead;
    private float lookAheadVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = transform.position;

        float targetX = target.position.x;
        float targetY = target.position.y + verticalOffset;

        float deltaX = targetX - transform.position.x;
        float deltaY = targetY - transform.position.y;

        // Horizontal dead zone
        if (Mathf.Abs(deltaX) > horizontalDeadZone)
        {
            desiredPosition.x =
                targetX -
                Mathf.Sign(deltaX) * horizontalDeadZone;
        }

        // Vertical dead zone
        if (Mathf.Abs(deltaY) > verticalDeadZone)
        {
            desiredPosition.y =
                targetY -
                Mathf.Sign(deltaY) * verticalDeadZone;
        }

        // Horizontal look-ahead
        float targetLookAhead = 0f;

        if (targetBody != null &&
            Mathf.Abs(targetBody.linearVelocity.x) > 0.02f)
        {
            targetLookAhead =
                Mathf.Sign(targetBody.linearVelocity.x) *
                lookAheadDistance;
        }

        currentLookAhead = Mathf.SmoothDamp(
            currentLookAhead,
            targetLookAhead,
            ref lookAheadVelocity,
            lookAheadSmoothTime
        );

        desiredPosition.x += currentLookAhead;
        desiredPosition.z = -10f;

        Vector3 newPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            smoothTime
        );

        newPosition = ClampToBounds(newPosition);

        transform.position = newPosition;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (cameraBounds == null)
            return position;

        Bounds bounds = cameraBounds.bounds;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;

        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        if (minX <= maxX)
        {
            position.x = Mathf.Clamp(
                position.x,
                minX,
                maxX
            );
        }

        if (minY <= maxY)
        {
            position.y = Mathf.Clamp(
                position.y,
                minY,
                maxY
            );
        }

        return position;
    }

    private void OnDrawGizmos()
    {
        if (cameraBounds == null)
            return;

        Gizmos.DrawWireCube(
            cameraBounds.bounds.center,
            cameraBounds.bounds.size
        );
    }
}