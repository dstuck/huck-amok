using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HealthHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Sprite heartSprite;

    private UIDocument uiDocument;
    private VisualElement heartsContainer;
    private readonly List<VisualElement> hearts = new();
    private int builtForMaxHealth;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;

        StartCoroutine(InitializeWhenReady());
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private IEnumerator InitializeWhenReady()
    {
        yield return null;

        int maxHealth = playerHealth != null ? playerHealth.MaxHealth : 0;
        EnsureHearts(maxHealth);
        ApplyHeartLayout();

        if (playerHealth != null)
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        var root = uiDocument.rootVisualElement;
        if (root != null)
            root.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
    }

    private void OnRootGeometryChanged(GeometryChangedEvent evt)
    {
        ApplyHeartLayout();
    }

    private void EnsureHearts(int maxHealth)
    {
        if (heartsContainer == null)
            CacheHeartsContainer();

        if (heartsContainer == null || maxHealth <= 0)
            return;

        if (builtForMaxHealth == maxHealth && hearts.Count == maxHealth)
            return;

        heartsContainer.Clear();
        hearts.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            var heart = new VisualElement { name = $"heart-{i + 1}" };
            heart.AddToClassList("heart");
            heartsContainer.Add(heart);
            hearts.Add(heart);
        }

        builtForMaxHealth = maxHealth;
    }

    private void ApplyHeartLayout()
    {
        if (heartsContainer == null)
            CacheHeartsContainer();

        if (heartsContainer == null || hearts.Count == 0)
            return;

        float screenWidth = uiDocument.rootVisualElement.resolvedStyle.width;
        float screenHeight = uiDocument.rootVisualElement.resolvedStyle.height;
        if (screenWidth <= 0f || screenHeight <= 0f)
            return;

        float heartSize = Mathf.Clamp(screenWidth * 0.04f, 32f, 72f);

        heartsContainer.style.position = Position.Absolute;
        heartsContainer.style.left = screenWidth * 0.02f;
        heartsContainer.style.top = screenHeight * 0.02f;
        heartsContainer.style.flexDirection = FlexDirection.Row;

        var background = heartSprite != null ? new StyleBackground(heartSprite) : default;

        foreach (var heart in hearts)
        {
            heart.style.width = heartSize;
            heart.style.height = heartSize;
            heart.style.flexGrow = 0;
            heart.style.flexShrink = 0;
            heart.style.marginRight = heartSize * 0.15f;
            heart.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

            if (heartSprite != null)
                heart.style.backgroundImage = background;
        }
    }

    private void CacheHeartsContainer()
    {
        var root = uiDocument.rootVisualElement;
        if (root == null)
            return;

        var hudRoot = root.Q<VisualElement>("hud-root");
        if (hudRoot == null)
            return;

        heartsContainer = hudRoot.Q<VisualElement>("hearts-container");
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        EnsureHearts(maxHealth);
        ApplyHeartLayout();

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].style.display = i < currentHealth ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
