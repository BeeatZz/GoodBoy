using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[Serializable]
public struct CinematicShot
{
    public FixedCamera camera;
    public float holdDuration;
    public bool trackLive;
    public bool fadeToBlack;
    public string shotText;
    public AudioClip shotSound;
    public UnityEvent onShotStart;
}

public class CinematicDirector : MonoBehaviour
{
    public CinematicShot[] shots;
    public FixedCamera endCamera;
    public FollowTarget followTarget;
    public CameraManager cameraManager;
    public PlayerController playerController;
    public AudioSource audioSource;
    public TextMeshProUGUI subtitleText;
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

        SetupShot(shots[0], true);

        if (fadeInOnStart)
            yield return StartCoroutine(GameStateManager.Instance.FadeIn(fadeDuration));

        for (int i = 0; i < shots.Length; i++)
        {
            var shot = shots[i];

            if (i > 0 && shot.fadeToBlack)
            {
                yield return StartCoroutine(GameStateManager.Instance.FadeOut(fadeDuration));
                SetupShot(shot, true);
                yield return StartCoroutine(GameStateManager.Instance.FadeIn(fadeDuration));
            }
            else if (i > 0)
            {
                SetupShot(shot, false);
            }

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
                cameraManager.StopLiveTracking();
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