using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PatrolEnemy), typeof(EnemyHealth))]
public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private EnemyDeathPresentation deathVisualPrefab;
    private PatrolEnemy patrol;
    private EnemyHealth health;
    private bool deathShown;

    private void Awake()
    {
        patrol = GetComponent<PatrolEnemy>();
        health = GetComponent<EnemyHealth>();
    }

    private void LateUpdate()
    {
        if (Time.timeScale > 0f && visual != null) visual.flipX = patrol.FacingDirection < 0;
    }

    private void OnDisable()
    {
        if (!Application.isPlaying || deathShown || health == null || health.CurrentHealth != 0 ||
            deathVisualPrefab == null || visual == null || !gameObject.scene.isLoaded) return;
        deathShown = true;
        // EnemyHealth still disables/destroys its gameplay object immediately. This copy
        // has no collider, health, score or movement and cannot receive another hit.
        var death = Instantiate(deathVisualPrefab, visual.transform.position, visual.transform.rotation);
        death.transform.localScale = visual.transform.lossyScale;
        death.SetFacing(patrol.FacingDirection < 0);
    }
}
