using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [FormerlySerializedAs("hitSounds")]
    [SerializeField] private AudioClip[] hurtSounds;

    private int currentHealth;
    private InvulnerabilityController invulnerability;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        currentHealth = maxHealth;
        invulnerability = GetComponent<InvulnerabilityController>();
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0)
            return;

        if (invulnerability != null && invulnerability.IsInvulnerable)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (hurtSounds != null && hurtSounds.Length > 0 && SoundFXManager.Instance != null)
        {
            SoundFXManager.Instance.PlayRandomSoundFXClip(
                hurtSounds,
                transform,
                category: SfxCategory.Hurt);
        }

        invulnerability?.BeginInvulnerability();

        if (currentHealth == 0)
            OnDied?.Invoke();
    }
}
