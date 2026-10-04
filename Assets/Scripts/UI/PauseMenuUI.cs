using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
[DefaultExecutionOrder(100)]
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private LevelPause levelPause;
    private VisualElement overlay;
    private Button resumeButton;
    private Button restartButton;
    private Button menuButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        overlay = root.Q("pause-overlay");
        resumeButton = root.Q<Button>("resume-button");
        restartButton = root.Q<Button>("pause-restart-button");
        menuButton = root.Q<Button>("main-menu-button");
        resumeButton.clicked += Resume;
        restartButton.clicked += Restart;
        menuButton.clicked += MainMenu;
        levelPause.PauseChanged += ShowPause;
        ShowPause(levelPause.IsPaused);
    }

    private void OnDisable()
    {
        resumeButton.clicked -= Resume;
        restartButton.clicked -= Restart;
        menuButton.clicked -= MainMenu;
        if (levelPause != null) levelPause.PauseChanged -= ShowPause;
    }

    private void ShowPause(bool paused)
    {
        overlay.style.display = paused ? DisplayStyle.Flex : DisplayStyle.None;
        if (paused) resumeButton.Focus();
        else GetComponent<UIDocument>().rootVisualElement.focusController?.focusedElement?.Blur();
    }

    private void Resume() => levelPause.Resume();
    private void Restart() => levelPause.Restart();
    private void MainMenu() => levelPause.MainMenu();
}
