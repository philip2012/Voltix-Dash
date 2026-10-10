using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Scene transitions always leave global playback in a clean state.</summary>
public static class SceneNavigation
{
    public const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    public const string LevelPath = "Assets/Scenes/Level01.unity";

    public static void ResetPlayback()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public static void PlayLevel()
    {
        LoadLevel(LevelPath);
    }

    public static void LoadLevel(string scenePath)
    {
        ResetPlayback();
        SceneManager.LoadScene(scenePath);
    }

    public static void MainMenu()
    {
        ResetPlayback();
        SceneManager.LoadScene(MainMenuPath);
    }

    public static void Quit()
    {
#if !UNITY_EDITOR && !UNITY_WEBGL
        Application.Quit();
#endif
    }
}
