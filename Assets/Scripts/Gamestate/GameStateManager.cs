using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public static event Action<GameState> OnStateChanged;

    public GameState Current { get; private set; }

    [Header("References")]
    public CameraManager    cameraManager;
    public PlayerController playerController;
    public PlayerInput      playerInput;

    [Header("Fade")]
    public Image  fadeImage;
    public float  fadeDuration = 0.4f;

    private string _loadedMinigameScene;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Make sure the fade image starts fully transparent
        SetFadeAlpha(0f);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Call this from any trigger or interaction in the world to start a minigame.
    /// onComplete runs after the player returns to the world.
    /// </summary>
    public void EnterMinigame(string sceneName, Action onComplete = null)
    {
        if (Current != GameState.World) return;
        StartCoroutine(EnterMinigameRoutine(sceneName, onComplete));
    }

    /// <summary>
    /// Called by MinigameController when the minigame is finished.
    /// </summary>
    public void ExitMinigame(Action onComplete = null)
    {
        if (Current != GameState.Minigame) return;
        StartCoroutine(ExitMinigameRoutine(onComplete));
    }

    // ── Routines ──────────────────────────────────────────────────────────────

    private IEnumerator EnterMinigameRoutine(string sceneName, Action onComplete)
    {
        SetState(GameState.Transitioning);

        // Fade out
        yield return StartCoroutine(Fade(0f, 1f));

        // Freeze world
        FreezeWorld();

        // Load minigame scene on top
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _loadedMinigameScene = sceneName;

        // Switch input to minigame map
        SwitchInputMap("Minigame");

        SetState(GameState.Minigame);

        // Fade in
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator ExitMinigameRoutine(Action onComplete)
    {
        SetState(GameState.Transitioning);

        // Fade out
        yield return StartCoroutine(Fade(0f, 1f));

        // Unload minigame — world was never touched so nothing to restore
        if (!string.IsNullOrEmpty(_loadedMinigameScene))
        {
            yield return SceneManager.UnloadSceneAsync(_loadedMinigameScene);
            _loadedMinigameScene = null;
        }

        // Resume world
        ResumeWorld();

        // Switch input back to world map
        SwitchInputMap("World");

        SetState(GameState.World);

        // Fade in
        yield return StartCoroutine(Fade(1f, 0f));

        onComplete?.Invoke();
    }

    // ── World freeze / resume ─────────────────────────────────────────────────

    private void FreezeWorld()
    {
        if (cameraManager)    cameraManager.enabled    = false;
        if (playerController) playerController.enabled = false;
    }

    private void ResumeWorld()
    {
        if (cameraManager)    cameraManager.enabled    = true;
        if (playerController) playerController.enabled = true;
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void SwitchInputMap(string mapName)
    {
        if (playerInput) playerInput.SwitchCurrentActionMap(mapName);
    }

    // ── Fade ──────────────────────────────────────────────────────────────────

    private IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }

        SetFadeAlpha(to);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;
        var c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(alpha > 0f);
    }

    // ── State ─────────────────────────────────────────────────────────────────

    private void SetState(GameState state)
    {
        Current = state;
        OnStateChanged?.Invoke(state);
    }
}

public enum GameState { World, Transitioning, Minigame }
