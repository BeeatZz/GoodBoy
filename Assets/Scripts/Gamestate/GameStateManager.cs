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
    public CameraManager cameraManager;
    public PlayerController playerController;
    public PlayerInput playerInput;

    [Header("Fade")]
    [Tooltip("Fullscreen black Image on a Screen Space Overlay canvas.")]
    public Image fadeImage;
    public float fadeDuration = 0.4f;

    private string _loadedMinigameScene;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        SetFadeAlpha(0f);
    }

    // ── Public API — minigame transitions ─────────────────────────────────────

    public void EnterMinigame(string sceneName, Action onComplete = null)
    {
        if (Current != GameState.World) return;
        StartCoroutine(EnterMinigameRoutine(sceneName, onComplete));
    }

    public void ExitMinigame(Action onComplete = null)
    {
        if (Current != GameState.Minigame) return;
        StartCoroutine(ExitMinigameRoutine(onComplete));
    }

    // ── Public API — fade (used by CinematicDirector and anything else) ───────

    public IEnumerator FadeIn(float duration) => FadeRoutine(1f, 0f, duration);
    public IEnumerator FadeOut(float duration) => FadeRoutine(0f, 1f, duration);

    // ── Minigame routines ─────────────────────────────────────────────────────

    private IEnumerator EnterMinigameRoutine(string sceneName, Action onComplete)
    {
        SetState(GameState.Transitioning);

        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));

        FreezeWorld();

        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _loadedMinigameScene = sceneName;

        SwitchInputMap("Minigame");
        SetState(GameState.Minigame);

        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));
    }

    private IEnumerator ExitMinigameRoutine(Action onComplete)
    {
        SetState(GameState.Transitioning);

        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));

        if (!string.IsNullOrEmpty(_loadedMinigameScene))
        {
            yield return SceneManager.UnloadSceneAsync(_loadedMinigameScene);
            _loadedMinigameScene = null;
        }

        ResumeWorld();
        SwitchInputMap("World");
        SetState(GameState.World);

        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));

        onComplete?.Invoke();
    }

    // ── World freeze / resume ─────────────────────────────────────────────────

    private void FreezeWorld()
    {
        if (cameraManager) cameraManager.enabled = false;
        if (playerController) playerController.enabled = false;
    }

    private void ResumeWorld()
    {
        if (cameraManager) cameraManager.enabled = true;
        if (playerController) playerController.enabled = true;
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void SwitchInputMap(string mapName)
    {
        if (playerInput) playerInput.SwitchCurrentActionMap(mapName);
    }

    // ── Fade ──────────────────────────────────────────────────────────────────

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(from, to, elapsed / duration));
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