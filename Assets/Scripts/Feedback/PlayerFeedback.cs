using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHealth), typeof(Rigidbody2D))]
[DefaultExecutionOrder(100)]
public class PlayerFeedback : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundRadius = 0.15f;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private Vector2 jumpStretch = new Vector2(0.93f, 1.09f);
    [SerializeField] private Vector2 fallStretch = new Vector2(0.96f, 1.06f);
    [SerializeField] private Vector2 landingSquash = new Vector2(1.12f, 0.9f);
    [SerializeField] private float landingDuration = 0.12f;
    [SerializeField] private float flashDuration = 0.12f;

    private PlayerHealth health;
    private Rigidbody2D body;
    private MaterialPropertyBlock properties;
    private Vector3 baseScale;
    private Vector3 basePosition;
    private bool wasGrounded;
    private float lastVelocityY;
    private int lastHealth;
    private float landingUntil;
    private float flashUntil;
    private float lastFlash = -1f;
    private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        body = GetComponent<Rigidbody2D>();
        properties = new MaterialPropertyBlock();
        if (visual != null)
        {
            baseScale = visual.transform.localScale;
            basePosition = visual.transform.localPosition;
            visual.GetPropertyBlock(properties);
        }
    }

    private void OnEnable()
    {
        lastHealth = health.CurrentHealth;
        health.HealthChanged += OnHealthChanged;
        health.DeathRespawned += ResetPose;
    }

    private void OnDisable()
    {
        health.HealthChanged -= OnHealthChanged;
        health.DeathRespawned -= ResetPose;
        ResetPose();
        SetFlash(0f);
    }

    private void OnHealthChanged(int current, int maximum)
    {
        if (current < lastHealth)
        {
            flashUntil = Time.time + flashDuration;
            SetFlash(1f);
            if (cameraShake != null) cameraShake.Shake(0.06f, 0.14f);
        }
        lastHealth = current;
    }

    private void LateUpdate()
    {
        if (visual == null) return;
        if (!health.isActiveAndEnabled)
        {
            ResetPose();
            SetFlash(0f);
            return;
        }

        float velocityY = body.linearVelocity.y;
        bool grounded = groundCheck != null && velocityY <= 0.1f &&
            Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer) != null;
        if (grounded && !wasGrounded && lastVelocityY < -1f)
            landingUntil = Time.time + landingDuration;

        Vector2 shape = Vector2.one;
        if (Time.time < landingUntil) shape = landingSquash;
        else if (!grounded && velocityY > 1f) shape = jumpStretch;
        else if (!grounded && velocityY < -2f) shape = fallStretch;

        Vector3 target = Vector3.Scale(baseScale, new Vector3(shape.x, shape.y, 1f));
        visual.transform.localScale = Vector3.Lerp(visual.transform.localScale, target,
            1f - Mathf.Exp(-20f * Time.deltaTime));
        Vector3 position = basePosition;
        // Anchor the feet while scaling only the sprite, never the player or collider.
        position.y += visual.sprite.bounds.min.y * (baseScale.y - visual.transform.localScale.y);
        visual.transform.localPosition = position;

        SetFlash(Mathf.Clamp01((flashUntil - Time.time) / Mathf.Max(0.01f, flashDuration)));
        wasGrounded = grounded;
        lastVelocityY = velocityY;
    }

    private void ResetPose()
    {
        if (visual != null)
        {
            visual.transform.localScale = baseScale;
            visual.transform.localPosition = basePosition;
        }
        wasGrounded = false;
        lastVelocityY = 0f;
        landingUntil = 0f;
    }

    private void SetFlash(float amount)
    {
        if (visual == null || properties == null || Mathf.Approximately(lastFlash, amount)) return;
        properties.SetFloat(FlashAmount, amount);
        visual.SetPropertyBlock(properties);
        lastFlash = amount;
    }
}
