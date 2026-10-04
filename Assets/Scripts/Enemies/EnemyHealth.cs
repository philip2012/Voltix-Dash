using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 1;
    [SerializeField, Min(0)] private int scoreValue = 100;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public static event System.Action<Vector3> Killed;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (!isActiveAndEnabled || amount <= 0 || CurrentHealth <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (CurrentHealth == 0)
        {
            // Stop movement and contact damage immediately, before end-of-frame destruction.
            gameObject.SetActive(false);
            // CurrentHealth is already zero, so repeat hits cannot award score again.
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(scoreValue);
            }
            Killed?.Invoke(transform.position);
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        scoreValue = Mathf.Max(0, scoreValue);
    }
}
