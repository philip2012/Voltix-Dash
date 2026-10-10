using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SentryTurret), typeof(EnemyHealth))]
public class SentryPresentation : MonoBehaviour
{
    [SerializeField] private Animator visual;
    [SerializeField] private EnemyDeathPresentation deathVisualPrefab;
    private SentryTurret sentry;
    private EnemyHealth health;
    private int lastState = -1;
    private float fireUntil;
    private bool deathShown;
    private SpriteRenderer sprite;
    private void Awake()
    {
        sentry = GetComponent<SentryTurret>(); health = GetComponent<EnemyHealth>();
        if (visual != null) sprite = visual.GetComponent<SpriteRenderer>();
    }
    private void LateUpdate()
    {
        if (Time.timeScale == 0f || visual == null) return;
        if (sprite != null) sprite.flipX = sentry.FacingDirection < 0;
        int state = sentry.VisualState;
        if (state == 2) fireUntil = Time.time + 0.15f;
        if (Time.time < fireUntil) state = 2;
        if (state == lastState) return;
        lastState = state;
        visual.Play(state == 2 ? "Fire" : state == 1 ? "Charge" : "Idle", 0, 0f);
    }
    private void OnDisable()
    {
        if (!Application.isPlaying || deathShown || health == null || health.CurrentHealth != 0 ||
            deathVisualPrefab == null || visual == null || !gameObject.scene.isLoaded) return;
        deathShown = true;
        var death = Instantiate(deathVisualPrefab, visual.transform.position, visual.transform.rotation);
        death.transform.localScale = visual.transform.lossyScale;
        death.SetFacing(sentry.FacingDirection < 0);
    }
}
