using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HealthHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Sprite heartSprite;

    private UIDocument uiDocument;
    private VisualElement heartsContainer;
    private VisualElement[] hearts;

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

        CacheHeartElements();
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

    private void ApplyHeartLayout()
    {
        if (heartsContainer == null || hearts == null)
            CacheHeartElements();

        if (heartsContainer == null || hearts == null)
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
            if (heart == null)
                continue;

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

    private void CacheHeartElements()
    {
        var root = uiDocument.rootVisualElement;
        if (root == null)
            return;

        var hudRoot = root.Q<VisualElement>("hud-root");
        if (hudRoot == null)
            return;

        heartsContainer = hudRoot.Q<VisualElement>("hearts-container");
        hearts = new[]
        {
            hudRoot.Q<VisualElement>("heart-1"),
            hudRoot.Q<VisualElement>("heart-2"),
            hudRoot.Q<VisualElement>("heart-3")
        };
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (hearts == null)
            CacheHeartElements();

        ApplyHeartLayout();

        if (hearts == null)
            return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            hearts[i].style.display = i < currentHealth ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
