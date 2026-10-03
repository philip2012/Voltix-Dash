using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class HazardDamage : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DamagePlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        DamagePlayer(collision.collider);
    }

    private void DamagePlayer(Collider2D other)
    {
        // Supports child colliders as well as a collider on the player root.
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            // The player's cooldown also protects against multiple simultaneous contacts.
            health.TakeDamage(damage);
        }
    }

    private void OnValidate()
    {
        damage = Mathf.Max(1, damage);
    }
}
