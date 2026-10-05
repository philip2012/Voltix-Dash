using UnityEngine;

/// <summary>Visual state only. The controller, physics and combat stay authoritative.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHealth), typeof(PlayerCombat), typeof(PlayerFeedback))]
[DefaultExecutionOrder(150)]
public class PlayerAnimationController : MonoBehaviour
{
    public enum State { Idle, Run, JumpStart, Rise, Fall, Land, Attack, Hurt }
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private AnimationClip jumpStartClip;
    [SerializeField] private AnimationClip landClip;
    [SerializeField] private AnimationClip attackClip;
    [SerializeField] private AnimationClip hurtClip;
    [SerializeField] private LevelManager levelManager;

    private Rigidbody2D body;
    private PlayerHealth health;
    private PlayerCombat combat;
    private PlayerFeedback feedback;
    private int lastHealth;
    private int attackFacing = 1;
    private State motion = State.Idle;
    private float jumpUntil, landUntil, attackUntil, hurtUntil;
    private bool attackActive, hurtActive;
    private static readonly int Motion = Animator.StringToHash("Motion");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Locomotion = Animator.StringToHash("Base Layer.Locomotion");
    public State CurrentState { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
        combat = GetComponent<PlayerCombat>();
        feedback = GetComponent<PlayerFeedback>();
    }

    private void OnEnable()
    {
        lastHealth = health.CurrentHealth;
        feedback.Jumped += OnJumped;
        feedback.Landed += OnLanded;
        combat.AttackPerformed += OnAcceptedAttack;
        health.HealthChanged += OnHealthChanged;
        health.DeathRespawned += ResetPresentation;
    }

    private void OnDisable()
    {
        feedback.Jumped -= OnJumped;
        feedback.Landed -= OnLanded;
        combat.AttackPerformed -= OnAcceptedAttack;
        health.HealthChanged -= OnHealthChanged;
        health.DeathRespawned -= ResetPresentation;
    }

    private void OnJumped() { jumpUntil = Time.time + jumpStartClip.length; landUntil = 0f; }
    private void OnLanded() => landUntil = Time.time + landClip.length;
    private void OnAcceptedAttack(int direction, float range, float height)
    {
        attackFacing = direction;
        attackUntil = Time.time + attackClip.length;
    }
    private void OnHealthChanged(int current, int maximum)
    {
        if (current < lastHealth) hurtUntil = Time.time + hurtClip.length;
        lastHealth = current;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f || animator == null || visual == null) return;
        bool complete = levelManager != null && levelManager.IsComplete;
        Vector2 velocity = body.linearVelocity;
        bool grounded = velocity.y <= 0.1f && groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, 0.15f, groundLayer) != null;
        State next;
        if (complete) next = State.Idle;
        else if (!grounded)
            next = velocity.y > 0.1f ? (Time.time < jumpUntil ? State.JumpStart : State.Rise) : State.Fall;
        else if (Time.time < landUntil) next = State.Land;
        else next = Mathf.Abs(velocity.x) > 0.1f ? State.Run : State.Idle;

        bool attack = !complete && Time.time < attackUntil;
        bool hurt = !complete && Time.time < hurtUntil;
        bool changed = next != motion || attack != attackActive || hurt != hurtActive;
        if (next != motion)
        {
            motion = next;
            // Exact integer blend-tree thresholds: no crossfades between sprite frames.
            animator.SetFloat(Motion, (int)motion);
            animator.Play(Locomotion, 0, 0f);
        }
        animator.SetBool(Attack, attack);
        animator.SetBool(Hurt, hurt);
        animator.SetLayerWeight(1, attack || hurt ? 1f : 0f);
        attackActive = attack; hurtActive = hurt;
        CurrentState = hurt ? State.Hurt : attack ? State.Attack : motion;
        visual.flipX = (attack && !hurt ? attackFacing : combat.FacingDirection) < 0;
        // Sample state changes after feedback events in LateUpdate, before rendering.
        if (changed) animator.Update(0f);
    }

    private void ResetPresentation()
    {
        jumpUntil = landUntil = attackUntil = hurtUntil = 0f;
        attackActive = hurtActive = false;
        motion = CurrentState = State.Idle;
        animator.Rebind();
        animator.SetFloat(Motion, 0f);
        animator.SetBool(Attack, false);
        animator.SetBool(Hurt, false);
        animator.SetLayerWeight(1, 0f);
        animator.Play(Locomotion, 0, 0f);
        animator.Update(0f);
        visual.flipX = combat.FacingDirection < 0;
    }
}
