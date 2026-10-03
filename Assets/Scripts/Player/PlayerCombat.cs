using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField, Min(0f)] private float attackCooldown = 0.35f;
    [Tooltip("Forward reach from the player's center, in world units.")]
    [SerializeField, Min(0.05f)] private float attackRange = 1.4f;
    [SerializeField, Min(0.05f)] private float attackHeight = 1.5f;

    private int facingDirection = 1;
    private float nextAttackTime;
    private readonly HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    public int FacingDirection => facingDirection;
    public event System.Action<int, float, float> AttackPerformed;

    private Vector3 AttackCenter => transform.position +
        Vector3.right * (facingDirection * attackRange * 0.5f);

    // PlayerInput sends Move to both this component and PlayerMovement.
    public void OnMove(InputValue value)
    {
        float horizontal = value.Get<Vector2>().x;
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            facingDirection = horizontal > 0f ? 1 : -1;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            TryAttack();
        }
    }

    public bool TryAttack()
    {
        if (!isActiveAndEnabled || Time.time < nextAttackTime)
        {
            return false;
        }

        nextAttackTime = Time.time + attackCooldown;
        hitEnemies.Clear();

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            AttackCenter, new Vector2(attackRange, attackHeight), 0f, Physics2D.AllLayers);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth health = hit.GetComponentInParent<EnemyHealth>();
            if (health != null && hitEnemies.Add(health))
            {
                // Child or multiple colliders still receive only one damage per strike.
                health.TakeDamage(1);
            }
        }

        AttackPerformed?.Invoke(facingDirection, attackRange, attackHeight);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(AttackCenter, new Vector3(attackRange, attackHeight, 0f));
        Gizmos.DrawLine(transform.position,
            transform.position + Vector3.right * (facingDirection * attackRange));
    }

    private void OnValidate()
    {
        attackCooldown = Mathf.Max(0f, attackCooldown);
        attackRange = Mathf.Max(0.05f, attackRange);
        attackHeight = Mathf.Max(0.05f, attackHeight);
    }
}
