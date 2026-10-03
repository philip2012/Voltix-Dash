using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerAttackVisual : MonoBehaviour
{
    [SerializeField] private LineRenderer slash;
    [SerializeField, Min(0.01f)] private float duration = 0.12f;
    [SerializeField] private Color color = new Color(0.25f, 0.95f, 1f, 1f);

    private PlayerCombat combat;
    private float endTime;
    private int direction;
    private float range;
    private float height;
    private readonly Vector3[] points = new Vector3[13];

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        if (slash != null)
        {
            slash.useWorldSpace = true;
            slash.positionCount = points.Length;
            slash.enabled = false;
        }
    }

    private void OnEnable()
    {
        combat.AttackPerformed += ShowSlash;
    }

    private void OnDisable()
    {
        combat.AttackPerformed -= ShowSlash;
        if (slash != null) slash.enabled = false;
    }

    private void ShowSlash(int facing, float attackRange, float attackHeight)
    {
        if (slash == null) return;
        direction = facing;
        range = attackRange;
        height = attackHeight;
        endTime = Time.time + duration;
        slash.enabled = true;
        UpdateSlash();
    }

    private void LateUpdate()
    {
        if (slash == null || !slash.enabled) return;
        if (Time.time >= endTime)
        {
            slash.enabled = false;
            return;
        }
        UpdateSlash();
    }

    private void UpdateSlash()
    {
        for (int i = 0; i < points.Length; i++)
        {
            float angle = Mathf.Lerp(-65f, 65f, (float)i / (points.Length - 1)) * Mathf.Deg2Rad;
            // Small alternating offsets give the arc a simple electric edge.
            float jagged = i % 2 == 0 ? 0f : 0.07f;
            points[i] = transform.position + new Vector3(
                direction * (range * (0.15f + 0.85f * Mathf.Cos(angle)) - jagged),
                height * 0.45f * Mathf.Sin(angle), 0f);
        }
        slash.SetPositions(points);
        Color fadingColor = color;
        fadingColor.a *= Mathf.Clamp01((endTime - Time.time) / duration);
        slash.startColor = fadingColor;
        slash.endColor = fadingColor;
    }
}
