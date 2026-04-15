using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[Serializable]
public struct CinematicShot
{
    [Header("Camera Settings")]
    [Tooltip("The FixedCamera to use for this shot.")]
    public FixedCamera camera;
    [Tooltip("How long to hold on this shot.")]
    public float holdDuration;
    [Tooltip("If true, the camera tracks the camera transform live (for moving parents).")]
    public bool trackLive;

    [Header("Transitions & UI")]
    [Tooltip("If true, the screen fades to black before this shot starts.")]
    public bool fadeToBlack;
    [Tooltip("Text to display on screen during this shot.")]
    public string shotText;

    [Header("Audio & Logic")]
    [Tooltip("Sound effect to play at the start of this shot.")]
    public AudioClip shotSound;
    [Tooltip("Unity Events to trigger when this shot starts.")]
    public UnityEvent onShotStart;
}

public class CinematicDirector : MonoBehaviour
{
    [Header("Shots")]
    public CinematicShot[] shots;

    [Header("Ending Configuration")]
    public FixedCamera endCamera;
    public FollowTarget followTarget;

    [Header("References")]
    public CameraManager cameraManager;
    public PlayerController playerController;
    public AudioSource audioSource;
    public TextMeshProUGUI subtitleText;

    [Header("Settings")]
    public bool fadeInOnStart = true;
    public float fadeDuration = 0.5f;

    public static event Action OnCinematicComplete;

    private void Start()
    {
        if (playerController) playerController.enabled = false;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        if (shots.Length == 0)
        {
            Finish();
            yield break;
        }

        // Setup the very first shot state
        SetupShot(shots[0], true);

        if (fadeInOnStart)
            yield return StartCoroutine(GameStateManager.Instance.FadeIn(fadeDuration));

        for (int i = 0; i < shots.Length; i++)
        {
            var shot = shots[i];

            // Handle mid-sequence fades
            if (i > 0 && shot.fadeToBlack)
            {
                yield return StartCoroutine(GameStateManager.Instance.FadeOut(fadeDuration));
                SetupShot(shot, true); // Snap/Track while black
                yield return StartCoroutine(GameStateManager.Instance.FadeIn(fadeDuration));
            }
            else if (i > 0)
            {
                SetupShot(shot, false); // Blend or Track live
            }

            // UI and Audio
            if (subtitleText != null) subtitleText.text = shot.shotText;
            if (shot.shotSound != null && audioSource != null) audioSource.PlayOneShot(shot.shotSound);

            shot.onShotStart?.Invoke();

            yield return new WaitForSeconds(shot.holdDuration);

            if (subtitleText != null) subtitleText.text = "";
        }

        Finish();
    }

    private void SetupShot(CinematicShot shot, bool snapImmediately)
    {
        if (shot.camera == null) return;

        if (shot.trackLive)
        {
            cameraManager.TrackLive(shot.camera.transform);
        }
        else
        {
            if (snapImmediately)
            {
                var snap = shot.camera.GetSnapshot();
                cameraManager.mainCamera.transform.SetPositionAndRotation(snap.position, snap.rotation);
                cameraManager.mainCamera.fieldOfView = snap.fieldOfView;
                cameraManager.StopLiveTracking(); // Ensure we aren't stuck in live mode
            }
            else
            {
                cameraManager.BlendToCamera(shot.camera);
            }
        }
    }

    private void Finish()
    {
        cameraManager.StopLiveTracking();

        if (endCamera != null)
            cameraManager.BlendToCamera(endCamera);
        else if (followTarget != null)
            cameraManager.EnterFollowMode(followTarget);
        else
            cameraManager.ExitFollowMode();

        if (playerController) playerController.enabled = true;

        OnCinematicComplete?.Invoke();
    }
}