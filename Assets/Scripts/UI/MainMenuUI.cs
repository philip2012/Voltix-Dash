using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour
{
    private Button playButton;
    private Button quitButton;
    private bool loading;

    private void Awake() => SceneNavigation.ResetPlayback();

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        playButton = root.Q<Button>("play-button");
        quitButton = root.Q<Button>("quit-button");
        playButton.clicked += Play;
        quitButton.clicked += SceneNavigation.Quit;
        root.schedule.Execute(() => playButton.Focus());
    }

    private void OnDisable()
    {
        playButton.clicked -= Play;
        quitButton.clicked -= SceneNavigation.Quit;
    }

    private void Play()
    {
        if (loading) return;
        loading = true;
        SceneNavigation.PlayLevel();
    }
}
