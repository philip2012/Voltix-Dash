using UnityEngine;

/// <summary>Scene-owned, bounded 2D sound feedback; no persistent sources or listener.</summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(200)]
public class GameplayAudio : MonoBehaviour
{
    public enum Sound { Jump, Landing, Attack, Damage, EnemyDeath, Completion }
    private const int VoiceCount = 3;

    [Header("Event sources")]
    [SerializeField] private PlayerFeedback playerFeedback;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private LevelManager levelManager;

    [Header("Short mono clips")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landingClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip enemyDeathClip;
    [SerializeField] private AudioClip completionClip;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.55f;
    [SerializeField] private AudioSource[] voices;

    private readonly float[] nextAllowed = new float[6];
    private readonly double[] busyUntil = new double[VoiceCount];
    private readonly int[] voicePriority = new int[VoiceCount];
    private int lastHealth;
    private bool completed;

    private void Awake()
    {
        // Configure once, never on enable/restart callbacks. Reuse scene sources when present.
        AudioSource[] existing = GetComponents<AudioSource>();
        voices = new AudioSource[VoiceCount];
        for (int i = 0; i < VoiceCount; i++)
        {
            AudioSource source = i < existing.Length ? existing[i] : gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.reverbZoneMix = 0f;
            source.pitch = 1f;
            voices[i] = source;
        }
    }

    private void OnEnable()
    {
        if (playerFeedback != null)
        {
            playerFeedback.Jumped += OnJump;
            playerFeedback.Landed += OnLanding;
        }
        if (playerCombat != null) playerCombat.AttackPerformed += OnAttack;
        if (playerHealth != null)
        {
            lastHealth = playerHealth.CurrentHealth;
            playerHealth.HealthChanged += OnHealthChanged;
        }
        EnemyHealth.Killed += OnEnemyKilled;
        if (levelManager != null) levelManager.Completed += OnCompleted;
    }

    private void OnDisable()
    {
        if (playerFeedback != null)
        {
            playerFeedback.Jumped -= OnJump;
            playerFeedback.Landed -= OnLanding;
        }
        if (playerCombat != null) playerCombat.AttackPerformed -= OnAttack;
        if (playerHealth != null) playerHealth.HealthChanged -= OnHealthChanged;
        EnemyHealth.Killed -= OnEnemyKilled;
        if (levelManager != null) levelManager.Completed -= OnCompleted;
        StopVoices();
    }

    private void OnJump() => Play(Sound.Jump);
    private void OnLanding() => Play(Sound.Landing);
    private void OnAttack(int direction, float range, float height) => Play(Sound.Attack);
    private void OnEnemyKilled(Vector3 position) => Play(Sound.EnemyDeath);

    private void OnHealthChanged(int current, int maximum)
    {
        if (current < lastHealth) Play(Sound.Damage);
        lastHealth = current;
    }

    private void OnCompleted()
    {
        if (completed) return;
        completed = true;
        StopVoices();
        Play(Sound.Completion);
    }

    /// <summary>Audio-only cooldowns and a fixed pool prevent overlapping spam.</summary>
    public bool Play(Sound sound)
    {
        int kind = (int)sound;
        if (!isActiveAndEnabled || AudioListener.pause || kind < 0 || kind >= nextAllowed.Length ||
            (completed && sound != Sound.Completion) || Time.time < nextAllowed[kind])
            return false;

        AudioClip clip = GetClip(sound);
        if (clip == null || voices == null) return false;
        int priority = sound == Sound.Completion ? 4 : sound == Sound.Damage ? 3 :
            sound == Sound.Attack || sound == Sound.EnemyDeath ? 2 : sound == Sound.Jump ? 1 : 0;
        double now = AudioSettings.dspTime;
        int slot = -1;
        for (int i = 0; i < VoiceCount; i++)
        {
            if (voices[i] == null) continue;
            if (busyUntil[i] <= now && !voices[i].isPlaying) { slot = i; break; }
            if (voicePriority[i] <= priority && (slot < 0 || voicePriority[i] < voicePriority[slot] ||
                (voicePriority[i] == voicePriority[slot] && busyUntil[i] < busyUntil[slot])))
                slot = i;
        }
        if (slot < 0) return false; // Drop low-priority feedback instead of queuing late sounds.

        AudioSource source = voices[slot];
        source.Stop();
        source.clip = clip;
        source.volume = masterVolume * GetGain(sound);
        source.Play();
        voicePriority[slot] = priority;
        busyUntil[slot] = now + clip.length;
        nextAllowed[kind] = Time.time + GetCooldown(sound);
        return true;
    }

    private AudioClip GetClip(Sound sound)
    {
        switch (sound)
        {
            case Sound.Jump: return jumpClip;
            case Sound.Landing: return landingClip;
            case Sound.Attack: return attackClip;
            case Sound.Damage: return damageClip;
            case Sound.EnemyDeath: return enemyDeathClip;
            default: return completionClip;
        }
    }

    private static float GetGain(Sound sound)
    {
        switch (sound)
        {
            case Sound.Jump: return 0.5f;
            case Sound.Landing: return 0.32f;
            case Sound.Attack: return 0.55f;
            case Sound.Damage: return 0.6f;
            case Sound.EnemyDeath: return 0.5f;
            default: return 0.6f;
        }
    }

    private static float GetCooldown(Sound sound)
    {
        switch (sound)
        {
            case Sound.Attack: return 0.2f;
            case Sound.Damage: return 0.25f;
            case Sound.Completion: return 0.6f;
            default: return 0.12f;
        }
    }

    private void StopVoices()
    {
        if (voices == null) return;
        for (int i = 0; i < voices.Length; i++)
        {
            if (voices[i] != null) voices[i].Stop();
            busyUntil[i] = 0;
        }
    }
}
