using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(200)]
public class CameraShake : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    private Camera view;
    private float strength;
    private float duration;
    private float until;
    private bool applied;

    private void Awake() => view = GetComponent<Camera>();

    private void OnEnable()
    {
        if (levelManager != null) levelManager.Completed += StopShake;
    }

    private void OnDisable()
    {
        if (levelManager != null) levelManager.Completed -= StopShake;
        StopShake();
    }

    public void Shake(float amplitude, float seconds)
    {
        if (!isActiveAndEnabled || (levelManager != null && levelManager.IsComplete)) return;
        strength = Time.time >= until ? amplitude : Mathf.Max(strength, amplitude);
        duration = Mathf.Max(0.01f, seconds);
        until = Mathf.Max(until, Time.time + duration);
    }

    private void LateUpdate()
    {
        if (Time.time >= until)
        {
            StopShake();
            return;
        }

        // Jitter only the rendered projection. CameraFollow never sees a displaced transform.
        view.ResetProjectionMatrix();
        Matrix4x4 projection = view.projectionMatrix;
        float fade = Mathf.Clamp01((until - Time.time) / duration);
        projection.m03 += Mathf.Sin(Time.time * 97f) * strength * fade / (view.orthographicSize * view.aspect);
        projection.m13 += Mathf.Sin(Time.time * 121f + 1f) * strength * fade / view.orthographicSize;
        view.projectionMatrix = projection;
        applied = true;
    }

    private void StopShake()
    {
        if (applied && view != null) view.ResetProjectionMatrix();
        applied = false;
        until = 0f;
        strength = 0f;
    }
}
