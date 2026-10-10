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
    private Label titleLabel;
    private Label timeLabel;
    private Label deathsLabel;
    private Button restartButton;
    private Button nextLevelButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        overlay = root.Q("completion-overlay");
        scoreLabel = root.Q<Label>("final-score-label");
        titleLabel = root.Q<Label>("completion-title");
        timeLabel = root.Q<Label>("final-time-label");
        deathsLabel = root.Q<Label>("final-deaths-label");
        restartButton = root.Q<Button>("restart-button");
        nextLevelButton = root.Q<Button>("next-level-button");

        if (restartButton != null) restartButton.clicked += Restart;
        if (nextLevelButton != null) nextLevelButton.clicked += NextLevel;
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
        if (nextLevelButton != null) nextLevelButton.clicked -= NextLevel;
    }

    private void ShowCompletion()
    {
        if (overlay == null) return;
        if (titleLabel != null) titleLabel.text = levelManager.CompletionTitle;
        if (scoreLabel != null) scoreLabel.text = $"SCORE: {levelManager.FinalScore:D4}";
        TimeSpan elapsed = TimeSpan.FromSeconds(levelManager.ElapsedTime);
        if (timeLabel != null)
            timeLabel.text = $"TIME: {(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
        if (deathsLabel != null) deathsLabel.text = $"DEATHS: {levelManager.DeathCount}";
        if (nextLevelButton != null)
            nextLevelButton.style.display = levelManager.HasNextLevel ? DisplayStyle.Flex : DisplayStyle.None;
        overlay.style.display = DisplayStyle.Flex;
    }

    private void Restart()
    {
        if (levelManager != null) levelManager.RestartLevel();
    }

    private void NextLevel()
    {
        if (levelManager != null) levelManager.NextLevel();
    }
}
