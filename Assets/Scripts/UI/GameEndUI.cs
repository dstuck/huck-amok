using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameEndUI : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement endScreen;
    private Label titleLabel;
    private Label subtitleLabel;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        var root = uiDocument.rootVisualElement;
        var hudRoot = root.Q<VisualElement>("hud-root") ?? root;
        endScreen = hudRoot.Q<VisualElement>("end-screen");
        titleLabel = hudRoot.Q<Label>("end-title");
        subtitleLabel = hudRoot.Q<Label>("end-subtitle");

        HideEndScreen();

        if (titleLabel != null)
        {
            titleLabel.style.color = Color.white;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        }

        if (subtitleLabel != null)
        {
            subtitleLabel.style.color = new Color(0.78f, 0.78f, 0.78f);
            subtitleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameplayState state)
    {
        switch (state)
        {
            case GameplayState.GameOver:
                ShowEndScreen("Game Over", "Press any key to restart");
                break;
            case GameplayState.Victory:
                ShowEndScreen("You Win!", "Press any key to restart");
                break;
            default:
                HideEndScreen();
                break;
        }
    }

    private void ShowEndScreen(string title, string subtitle)
    {
        if (endScreen == null)
            return;

        if (titleLabel != null)
            titleLabel.text = title;

        if (subtitleLabel != null)
            subtitleLabel.text = subtitle;

        endScreen.style.display = DisplayStyle.Flex;
    }

    private void HideEndScreen()
    {
        if (endScreen != null)
            endScreen.style.display = DisplayStyle.None;
    }
}
