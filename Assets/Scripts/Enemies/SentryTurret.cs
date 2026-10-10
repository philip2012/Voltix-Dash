using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHealth))]
public class SentryTurret : MonoBehaviour
{
    [SerializeField] private PlayerHealth target;
    [SerializeField] private EnergyProjectile projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField, Min(1f)] private float detectionRange = 7.5f;
    [SerializeField, Min(0.1f)] private float fireInterval = 1.5f;
    [SerializeField, Range(-1, 1)] private int facingDirection = -1;
    [SerializeField] private bool swivelTowardTarget;
    [SerializeField, Min(0.1f)] private float verticalTolerance = 1.65f;
    private float nextFire;
    private bool visibleLastFrame;
    private readonly RaycastHit2D[] sightHits = new RaycastHit2D[8];
    private ContactFilter2D sightFilter;
    private Camera viewCamera;
    public int VisualState { get; private set; }
    public int ShotsFired { get; private set; }
    public int FacingDirection => facingDirection;
    public static event System.Action<Vector3> Fired;
    private void Awake()
    {
        if (target == null) target = FindFirstObjectByType<PlayerHealth>();
        viewCamera = Camera.main;
        sightFilter = new ContactFilter2D().NoFilter();
        sightFilter.useTriggers = false;
    }
    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (swivelTowardTarget && target != null &&
            Mathf.Abs(target.transform.position.x - transform.position.x) <= detectionRange)
        {
            int facing = target.transform.position.x < transform.position.x ? -1 : 1;
            if (facing != facingDirection)
            {
                facingDirection = facing;
                visibleLastFrame = false;
                if (muzzle != null)
                    muzzle.localPosition = new Vector3(Mathf.Abs(muzzle.localPosition.x) * facing,
                        muzzle.localPosition.y, muzzle.localPosition.z);
            }
        }
        bool visible = CanSeeTarget();
        if (!visible) { visibleLastFrame = false; VisualState = 0; return; }
        if (!visibleLastFrame) nextFire = Time.time + fireInterval;
        visibleLastFrame = true;
        VisualState = nextFire - Time.time <= 0.35f ? 1 : 0;
        if (Time.time < nextFire || projectilePrefab == null) return;
        nextFire = Time.time + fireInterval;
        Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
        var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
        projectile.Initialize(transform, Vector2.right * (facingDirection < 0 ? -1 : 1));
        ShotsFired++;
        VisualState = 2;
        Fired?.Invoke(origin);
    }
    private bool CanSeeTarget()
    {
        if (target == null || !target.isActiveAndEnabled || !target.gameObject.activeInHierarchy) return false;
        if (viewCamera != null)
        {
            Vector3 screen = viewCamera.WorldToViewportPoint(transform.position);
            if (screen.z <= 0f || screen.x < 0.04f || screen.x > 0.96f ||
                screen.y < 0.04f || screen.y > 0.96f) return false;
        }
        Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
        Vector2 offset = (Vector2)target.transform.position - origin;
        if (offset.x * (facingDirection < 0 ? -1 : 1) <= 0f ||
            Mathf.Abs(offset.y) > verticalTolerance || offset.sqrMagnitude > detectionRange * detectionRange) return false;
        int count = Physics2D.Raycast(origin, offset.normalized, sightFilter, sightHits, offset.magnitude);
        for (int i = 0; i < count; i++)
        {
            var hit = sightHits[i].collider;
            if (hit.transform.IsChildOf(transform)) continue;
            return hit.GetComponentInParent<PlayerHealth>() == target;
        }
        return false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.55f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
