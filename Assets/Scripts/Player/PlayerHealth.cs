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
    private Vector3 respawnPosition;
    private Quaternion startingRotation;
    private float startingGravityScale;
    private float nextDamageTime;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public Vector3 StartingPosition => startingPosition;
    public Vector3 RespawnPosition => respawnPosition;
    public float FallRespawnY => fallRespawnY;
    public bool IsInvulnerable => Time.time < nextDamageTime;
    public event System.Action<int, int> HealthChanged;
    public event System.Action DeathRespawned;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
        respawnPosition = startingPosition;
        startingRotation = transform.rotation;
        startingGravityScale = body.gravityScale;
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void LateUpdate()
    {
        // A paused frame must not process a pending fall death.
        if (Time.timeScale == 0f) return;

        // Fall deaths bypass the contact-damage cooldown.
        if (transform.position.y < fallRespawnY)
        {
            RespawnAfterDeath();
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

        HealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth == 0)
        {
            RespawnAfterDeath();
        }

        return true;
    }

    private void RespawnAfterDeath()
    {
        Respawn();
        DeathRespawned?.Invoke();
    }

    public void Respawn()
    {
        CurrentHealth = maxHealth;
        nextDamageTime = Time.time + damageCooldown;

        // Teleport both the transform and physics body, then discard pre-death momentum.
        transform.SetPositionAndRotation(respawnPosition, startingRotation);
        body.position = respawnPosition;
        body.rotation = startingRotation.eulerAngles.z;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.gravityScale = startingGravityScale;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>Scene-owned checkpoints change only the destination; reload restores the original spawn.</summary>
    public void SetRespawnPosition(Vector3 position) => respawnPosition = position;

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        damageCooldown = Mathf.Max(0f, damageCooldown);
    }
}
