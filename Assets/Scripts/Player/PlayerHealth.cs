using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1)] private int maxHealth = 3;
    [Tooltip("Seconds of protection after taking damage or respawning.")]
    [SerializeField, Min(0f)] private float damageCooldown = 0.75f;

    [Header("Respawn")]
    [Tooltip("Falling below this world-space Y position immediately respawns the player.")]
    [SerializeField] private float fallRespawnY = -40f;

    private Rigidbody2D body;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private float startingGravityScale;
    private float nextDamageTime;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public Vector3 StartingPosition => startingPosition;
    public float FallRespawnY => fallRespawnY;
    public bool IsInvulnerable => Time.time < nextDamageTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        startingGravityScale = body.gravityScale;
        CurrentHealth = maxHealth;
    }

    private void LateUpdate()
    {
        // Fall deaths bypass the contact-damage cooldown.
        if (transform.position.y < fallRespawnY)
        {
            Respawn();
        }
    }

    /// <summary>Returns true when damage was accepted; lethal damage respawns immediately.</summary>
    public bool TakeDamage(int amount)
    {
        if (!isActiveAndEnabled || amount <= 0 || IsInvulnerable)
        {
            return false;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        nextDamageTime = Time.time + damageCooldown;

        if (CurrentHealth == 0)
        {
            Respawn();
        }

        return true;
    }

    public void Respawn()
    {
        CurrentHealth = maxHealth;
        nextDamageTime = Time.time + damageCooldown;

        // Teleport both the transform and physics body, then discard pre-death momentum.
        transform.SetPositionAndRotation(startingPosition, startingRotation);
        body.position = startingPosition;
        body.rotation = startingRotation.eulerAngles.z;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.gravityScale = startingGravityScale;
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        damageCooldown = Mathf.Max(0f, damageCooldown);
    }
}
