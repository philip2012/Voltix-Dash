using System;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int Score { get; private set; }
    public event Action<int> ScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        Score = 0;
    }

    public void AddScore(int points)
    {
        if (points <= 0)
        {
            return;
        }

        Score += points;
        ScoreChanged?.Invoke(Score);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
