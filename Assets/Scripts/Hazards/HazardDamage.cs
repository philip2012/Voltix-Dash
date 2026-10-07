using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class HazardDamage : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 1;

    private struct Contact
    {
        public Collider2D Collider;
        public PlayerHealth Health;
    }

    private readonly List<Contact> contacts = new List<Contact>(2);
    private Collider2D[] hazardColliders;

    private void Awake() => hazardColliders = GetComponents<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other) => TrackContact(other);
    private void OnTriggerStay2D(Collider2D other) => TrackContact(other);
    private void OnTriggerExit2D(Collider2D other) => RemoveExitedContact(other);
    private void OnCollisionEnter2D(Collision2D collision) => TrackContact(collision.collider);
    private void OnCollisionStay2D(Collision2D collision) => TrackContact(collision.collider);
    private void OnCollisionExit2D(Collision2D collision) => RemoveExitedContact(collision.collider);

    private void TrackContact(Collider2D other)
    {
        // Unity can send contact callbacks to disabled MonoBehaviours.
        if (!isActiveAndEnabled || Time.timeScale == 0f) return;
        for (int i = 0; i < contacts.Count; i++)
            if (contacts[i].Collider == other) return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.isActiveAndEnabled || !IsStillTouching(other)) return;

        if (!HasHealth(health)) health.DeathRespawned += ClearContacts;
        contacts.Add(new Contact { Collider = other, Health = health });
        TryDamage(health);
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;
        // Stay callbacks stop when a Rigidbody2D sleeps. Retain contact and check
        // its actual geometry, without waking bodies or changing physics tuning.
        for (int i = contacts.Count - 1; i >= 0; i--)
        {
            Contact contact = contacts[i];
            if (contact.Health == null || !contact.Health.isActiveAndEnabled ||
                !IsStillTouching(contact.Collider))
            {
                RemoveContact(i);
                continue;
            }
            // Lethal damage synchronously clears contacts via DeathRespawned.
            if (TryDamage(contact.Health)) return;
        }
    }

    private bool TryDamage(PlayerHealth health)
    {
        // PlayerHealth remains the sole cooldown authority across all hazards.
        return !health.IsInvulnerable && health.TakeDamage(damage);
    }

    private bool IsStillTouching(Collider2D other)
    {
        if (other == null || !other.enabled || !other.gameObject.activeInHierarchy ||
            (other.attachedRigidbody != null && !other.attachedRigidbody.simulated)) return false;

        foreach (Collider2D own in hazardColliders)
        {
            if (own == null || !own.enabled || !own.gameObject.activeInHierarchy ||
                (own.attachedRigidbody != null && !own.attachedRigidbody.simulated)) continue;
            ColliderDistance2D distance = own.Distance(other);
            if (!distance.isValid) continue;
            if (distance.isOverlapped) return true;
            // Solid contact can have the normal physics contact-offset separation.
            // Require an actual contact too; proximity alone never deals damage.
            if (!own.isTrigger && !other.isTrigger && own.IsTouching(other) &&
                distance.distance <= Physics2D.defaultContactOffset * 2f) return true;
        }
        return false;
    }

    private void RemoveExitedContact(Collider2D other)
    {
        // Another collider on the same hazard may still be touching this player.
        if (IsStillTouching(other)) return;
        for (int i = contacts.Count - 1; i >= 0; i--)
            if (contacts[i].Collider == other) RemoveContact(i);
    }

    private bool HasHealth(PlayerHealth health)
    {
        for (int i = 0; i < contacts.Count; i++)
            if (contacts[i].Health == health) return true;
        return false;
    }

    private void RemoveContact(int index)
    {
        PlayerHealth health = contacts[index].Health;
        contacts.RemoveAt(index);
        if (health != null && !HasHealth(health)) health.DeathRespawned -= ClearContacts;
    }

    private void ClearContacts()
    {
        foreach (Contact contact in contacts)
            if (contact.Health != null) contact.Health.DeathRespawned -= ClearContacts;
        contacts.Clear();
    }

    private void OnDisable() => ClearContacts();

    private void OnValidate() => damage = Mathf.Max(1, damage);
}
