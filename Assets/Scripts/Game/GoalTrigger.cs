using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (levelManager != null)
        {
            levelManager.CompleteLevel(other.GetComponentInParent<PlayerHealth>());
        }
    }
}
