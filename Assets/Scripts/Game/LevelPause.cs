using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-200)]
public class LevelPause : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private PlayerInput playerInput;

    private InputAction pauseAction;
    private PlayerMovement movement;
    private PlayerCombat combat;
    private CameraFollow cameraFollow;
    private bool cameraWasEnabled;
    private bool movementWasEnabled;
    private bool combatWasEnabled;
    private bool inputWasActive;
    private InputActionMap previousMap;
    private float previousTimeScale;
    private bool previousAudioPause;
    private bool navigating;

    public bool IsPaused { get; private set; }
    public event Action<bool> PauseChanged;

    private void Awake()
    {
        pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        if (Camera.main != null) cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (playerInput != null)
        {
            movement = playerInput.GetComponent<PlayerMovement>();
            combat = playerInput.GetComponent<PlayerCombat>();
        }
    }

    private void OnEnable()
    {
        pauseAction.performed += OnPause;
        pauseAction.Enable();
        if (levelManager != null) levelManager.Completed += OnCompleted;
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPause;
        pauseAction.Disable();
        if (levelManager != null) levelManager.Completed -= OnCompleted;
        ReleasePause(levelManager == null || !levelManager.IsComplete);
    }

    private void OnDestroy() => pauseAction.Dispose();
    private void OnPause(InputAction.CallbackContext context) => TogglePause();

    public void TogglePause()
    {
        if (navigating || (levelManager != null && levelManager.IsComplete)) return;
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused || navigating || (levelManager != null && levelManager.IsComplete)) return;
        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        movementWasEnabled = movement != null && movement.enabled;
        combatWasEnabled = combat != null && combat.enabled;
        cameraWasEnabled = cameraFollow != null && cameraFollow.enabled;
        inputWasActive = playerInput != null && playerInput.inputIsActive;
        previousMap = playerInput != null ? playerInput.currentActionMap : null;

        // Keep buffered controller state and body momentum intact. Suppress canceled
        // Send Messages callbacks: releasing Jump here would otherwise cut its velocity.
        if (movement != null) movement.enabled = false;
        if (combat != null) combat.enabled = false;
        if (cameraFollow != null) cameraFollow.enabled = false;
        SetInputActive(false);
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        PauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (!IsPaused || navigating || (levelManager != null && levelManager.IsComplete)) return;
        ReleasePause(true);
    }

    private void SetInputActive(bool active)
    {
        if (playerInput == null) return;
        PlayerNotifications notifications = playerInput.notificationBehavior;
        InputActionMap map = previousMap;
        playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        try
        {
            // Changing notification behavior clears PlayerInput's current-map reference.
            // Keep the same map before disabling it, so its callbacks are suppressed too.
            if (map != null) playerInput.currentActionMap = map;
            if (active) playerInput.ActivateInput();
            else playerInput.DeactivateInput();
            map = playerInput.currentActionMap;
        }
        finally
        {
            playerInput.notificationBehavior = notifications;
            playerInput.currentActionMap = map;
            if (!active) playerInput.DeactivateInput();
        }
    }

    private void ReleasePause(bool restoreGameplay)
    {
        if (!IsPaused) return;
        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        IsPaused = false;
        if (restoreGameplay)
        {
            if (inputWasActive) SetInputActive(true);
            if (movement != null) movement.enabled = movementWasEnabled;
            if (combat != null) combat.enabled = combatWasEnabled;
            if (cameraFollow != null) cameraFollow.enabled = cameraWasEnabled;
        }
        PauseChanged?.Invoke(false);
    }

    private void OnCompleted() => ReleasePause(false);

    public void Restart()
    {
        if (!IsPaused || navigating || (levelManager != null && levelManager.IsComplete)) return;
        navigating = true;
        ReleasePause(false);
        SceneNavigation.LoadLevel(gameObject.scene.path);
    }

    public void MainMenu()
    {
        if (!IsPaused || navigating || (levelManager != null && levelManager.IsComplete)) return;
        navigating = true;
        ReleasePause(false);
        SceneNavigation.MainMenu();
    }
}
