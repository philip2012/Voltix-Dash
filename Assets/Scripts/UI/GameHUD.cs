using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
[DefaultExecutionOrder(100)]
public class GameHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ScoreManager scoreManager;

    private Label healthLabel;
    private Label scoreLabel;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        healthLabel = root.Q<Label>("health-label");
        scoreLabel = root.Q<Label>("score-label");

        if (playerHealth != null)
        {
            playerHealth.HealthChanged += UpdateHealth;
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        if (scoreManager != null)
        {
            scoreManager.ScoreChanged += UpdateScore;
            UpdateScore(scoreManager.Score);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= UpdateHealth;
        }

        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -= UpdateScore;
        }
    }

    private void UpdateHealth(int current, int maximum)
    {
        if (healthLabel != null)
        {
            healthLabel.text = $"HP: {current}/{maximum}";
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreLabel != null)
        {
            scoreLabel.text = $"SCORE: {score:D4}";
        }
    }
}
