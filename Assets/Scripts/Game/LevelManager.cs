using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ScoreManager scoreManager;
    [Tooltip("Optional next scene name or build path. Leave empty for the final level.")]
    [SerializeField] private string nextLevelScene;
    [SerializeField] private string completionTitle = "LEVEL COMPLETE";

    private double startTime;
    private double completionTime;
    private bool restartRequested;

    public bool IsComplete { get; private set; }
    public int DeathCount { get; private set; }
    public int FinalScore { get; private set; }
    public string CompletionTitle => string.IsNullOrWhiteSpace(completionTitle) ? "LEVEL COMPLETE" : completionTitle;
    public bool HasNextLevel => !string.IsNullOrWhiteSpace(nextLevelScene) &&
        Application.CanStreamedLevelBeLoaded(nextLevelScene);
    public double ElapsedTime => IsComplete ? completionTime : Time.timeAsDouble - startTime;
    public event Action Completed;

    private void Awake()
    {
        startTime = Time.timeAsDouble;
    }

    private void OnEnable()
    {
        if (playerHealth != null) playerHealth.DeathRespawned += CountDeath;
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.DeathRespawned -= CountDeath;
    }

    private void CountDeath()
    {
        if (!IsComplete) DeathCount++;
    }

    public bool CompleteLevel(PlayerHealth player)
    {
        if (IsComplete || player == null || player != playerHealth) return false;

        completionTime = ElapsedTime;
        FinalScore = scoreManager != null ? scoreManager.Score : 0;
        IsComplete = true;

        var input = player.GetComponent<PlayerInput>();
        if (input != null)
        {
            input.DeactivateInput();
            input.enabled = false;
        }
        // Stop callbacks and physics without changing normal gameplay behavior.
        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
        var combat = player.GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;
        var visual = player.GetComponent<PlayerAttackVisual>();
        if (visual != null) visual.enabled = false;
        player.enabled = false;

        var body = player.GetComponent<Rigidbody2D>();
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.simulated = false;

        Completed?.Invoke();
        return true;
    }

    public void RestartLevel()
    {
        if (!IsComplete || restartRequested) return;
        restartRequested = true;
        SceneNavigation.LoadLevel(gameObject.scene.path);
    }

    public void NextLevel()
    {
        if (!IsComplete || restartRequested || !HasNextLevel) return;
        restartRequested = true;
        SceneNavigation.LoadLevel(nextLevelScene);
    }
}
