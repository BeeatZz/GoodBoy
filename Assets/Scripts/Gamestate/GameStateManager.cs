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

    public CameraManager cameraManager;
    public PlayerController playerController;
    public PlayerInput playerInput;

    public Image fadeImage;
    public float fadeDuration = 0.4f;

    private string _loadedMinigameScene;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetFadeAlpha(0f);
    }

  
    public void EnterMinigame(string sceneName, bool skipFadeOut = false, Action onComplete = null)
    {
        if (Current != GameState.World) return;
        StartCoroutine(EnterMinigameRoutine(sceneName, skipFadeOut, onComplete));
    }

    public void ExitMinigame(Action onComplete = null)
    {
        if (Current != GameState.Minigame) return;
        StartCoroutine(ExitMinigameRoutine(onComplete));
    }


    
    public void EnterMinigameStandalone(string sceneName, bool skipFadeOut = false)
    {
        if (Current != GameState.World) return;
        StartCoroutine(EnterMinigameStandaloneRoutine(sceneName, skipFadeOut));
    }


    public IEnumerator FadeIn(float duration) => FadeRoutine(1f, 0f, duration);
    public IEnumerator FadeOut(float duration) => FadeRoutine(0f, 1f, duration);


    private IEnumerator EnterMinigameRoutine(string sceneName, bool skipFadeOut, Action onComplete)
    {
        SetState(GameState.Transitioning);

        if (!skipFadeOut)
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


    private IEnumerator EnterMinigameStandaloneRoutine(string sceneName, bool skipFadeOut)
    {
        SetState(GameState.Transitioning);

        if (!skipFadeOut)
            yield return StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));

        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        SwitchInputMap("Minigame");
        SetState(GameState.Minigame);

        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));
    }


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


    private void SwitchInputMap(string mapName)
    {
        if (playerInput) playerInput.SwitchCurrentActionMap(mapName);
    }


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


    private void SetState(GameState state)
    {
        Current = state;
        OnStateChanged?.Invoke(state);
    }
}

public enum GameState { World, Transitioning, Minigame }