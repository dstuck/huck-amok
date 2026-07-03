using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance { get; private set; }

    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private int maxConcurrentVoices = 10;
    [SerializeField] private int maxShootVoices = 4;
    [SerializeField] private int maxHurtVoices = 2;
    [SerializeField] private int maxEnemyHitVoices = 3;
    [SerializeField] private int maxPickupVoices = 2;
    [SerializeField] private int maxSplatVoices = 4;

    private readonly List<ActiveVoice> activeVoices = new();

    private struct ActiveVoice
    {
        public AudioSource Source;
        public SfxCategory Category;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlaySoundFXClip(
        AudioClip audioClip,
        Transform spawnTransform,
        float volume = 1f,
        SfxCategory category = SfxCategory.Default)
    {
        if (audioClip == null || soundFXObject == null || spawnTransform == null)
        {
            Debug.LogWarning("[SoundFXManager] Missing clip, template, or spawn transform.");
            return;
        }

        if (!CanPlayCategory(category))
            return;

        CleanupFinishedVoices();

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.gameObject.SetActive(true);
        audioSource.enabled = true;
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        activeVoices.Add(new ActiveVoice
        {
            Source = audioSource,
            Category = category
        });

        Destroy(audioSource.gameObject, audioClip.length);
    }

    public void PlayRandomSoundFXClip(
        AudioClip[] audioClips,
        Transform spawnTransform,
        float volume = 1f,
        SfxCategory category = SfxCategory.Default)
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("[SoundFXManager] No clips provided for random selection.");
            return;
        }

        AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
        PlaySoundFXClip(randomClip, spawnTransform, volume, category);
    }

    private bool CanPlayCategory(SfxCategory category)
    {
        CleanupFinishedVoices();

        if (activeVoices.Count >= maxConcurrentVoices)
            return false;

        int categoryCount = 0;
        int categoryCap = GetCategoryCap(category);

        foreach (var voice in activeVoices)
        {
            if (voice.Category == category)
                categoryCount++;
        }

        return categoryCount < categoryCap;
    }

    private int GetCategoryCap(SfxCategory category)
    {
        return category switch
        {
            SfxCategory.Shoot => maxShootVoices,
            SfxCategory.Hurt => maxHurtVoices,
            SfxCategory.EnemyHit => maxEnemyHitVoices,
            SfxCategory.Pickup => maxPickupVoices,
            SfxCategory.Splat => maxSplatVoices,
            _ => maxConcurrentVoices
        };
    }

    private void CleanupFinishedVoices()
    {
        for (int i = activeVoices.Count - 1; i >= 0; i--)
        {
            if (activeVoices[i].Source == null || !activeVoices[i].Source.isPlaying)
                activeVoices.RemoveAt(i);
        }
    }
}
