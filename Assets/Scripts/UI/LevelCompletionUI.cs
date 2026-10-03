using System;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
[DefaultExecutionOrder(100)]
public class LevelCompletionUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    private VisualElement overlay;
    private Label scoreLabel;
    private Label timeLabel;
    private Label deathsLabel;
    private Button restartButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        overlay = root.Q("completion-overlay");
        scoreLabel = root.Q<Label>("final-score-label");
        timeLabel = root.Q<Label>("final-time-label");
        deathsLabel = root.Q<Label>("final-deaths-label");
        restartButton = root.Q<Button>("restart-button");

        if (restartButton != null) restartButton.clicked += Restart;
        if (levelManager != null)
        {
            levelManager.Completed += ShowCompletion;
            if (levelManager.IsComplete) ShowCompletion();
            else if (overlay != null) overlay.style.display = DisplayStyle.None;
        }
    }

    private void OnDisable()
    {
        if (levelManager != null) levelManager.Completed -= ShowCompletion;
        if (restartButton != null) restartButton.clicked -= Restart;
    }

    private void ShowCompletion()
    {
        if (overlay == null) return;
        if (scoreLabel != null) scoreLabel.text = $"SCORE: {levelManager.FinalScore:D4}";
        TimeSpan elapsed = TimeSpan.FromSeconds(levelManager.ElapsedTime);
        if (timeLabel != null)
            timeLabel.text = $"TIME: {(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
        if (deathsLabel != null) deathsLabel.text = $"DEATHS: {levelManager.DeathCount}";
        overlay.style.display = DisplayStyle.Flex;
    }

    private void Restart()
    {
        if (levelManager != null) levelManager.RestartLevel();
    }
}
