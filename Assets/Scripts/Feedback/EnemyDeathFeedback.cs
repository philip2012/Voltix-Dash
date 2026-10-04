using UnityEngine;

[DisallowMultipleComponent]
public class EnemyDeathFeedback : MonoBehaviour
{
    [SerializeField] private ParticleSystem burst;
    [SerializeField] private CameraShake cameraShake;

    private void OnEnable() => EnemyHealth.Killed += OnEnemyKilled;
    private void OnDisable() => EnemyHealth.Killed -= OnEnemyKilled;

    private void OnEnemyKilled(Vector3 position)
    {
        if (burst != null)
        {
            burst.Play(false);
            for (int i = 0; i < 8; i++)
            {
                float angle = (i * 45f + 22.5f) * Mathf.Deg2Rad;
                var particle = new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 2.4f,
                    startLifetime = 0.32f,
                    startSize = 0.14f,
                    startColor = i % 2 == 0 ? new Color(0.32f, 0.91f, 0.96f) : new Color(0.57f, 0.4f, 0.93f),
                    randomSeed = (uint)(i + 1)
                };
                burst.Emit(particle, 1);
            }
        }
        if (cameraShake != null) cameraShake.Shake(0.04f, 0.12f);
    }
}
