using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameplayState
{
    Playing,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool HasStarted { get; private set; }

    [SerializeField] private string gameplaySceneName = "Gameplay";

    private int activeEnemyCount;
    private PlayerHealth playerHealth;
    private PlayerController playerController;

    public GameplayState State { get; private set; } = GameplayState.Playing;

    public event Action<GameplayState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HasStarted = true;
        activeEnemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;

        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerController = FindFirstObjectByType<PlayerController>();

        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;

        if (Instance == this)
            Instance = null;

        HasStarted = false;
    }

    private void Update()
    {
        if (State == GameplayState.Playing)
            return;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            RestartScene();
    }

    public void RegisterEnemy()
    {
        activeEnemyCount++;
    }

    public void UnregisterEnemy()
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);

        if (State == GameplayState.Playing && activeEnemyCount == 0)
            SetState(GameplayState.Victory);
    }

    private void HandlePlayerDied()
    {
        if (State != GameplayState.Playing)
            return;

        SetState(GameplayState.GameOver);
    }

    private void SetState(GameplayState newState)
    {
        if (State == newState)
            return;

        State = newState;
        FreezeGameplay();
        OnStateChanged?.Invoke(newState);
    }

    private void FreezeGameplay()
    {
        if (playerController != null)
            playerController.enabled = false;

        foreach (var brain in FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))
            brain.Pause();
    }

    public void RestartScene()
    {
        string sceneName = string.IsNullOrEmpty(gameplaySceneName)
            ? SceneManager.GetActiveScene().name
            : gameplaySceneName;

        SceneManager.LoadScene(sceneName);
    }
}
