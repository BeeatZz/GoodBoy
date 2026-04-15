using System;
using System.Collections;
using UnityEngine;

[Serializable]

public struct CinematicShot

{

    [Tooltip("The FixedCamera to blend to for this shot.")]

    public FixedCamera camera;


    [Tooltip("How long to hold on this shot before moving to the next one.")]

    public float holdDuration;

}
public class CinematicDirector : MonoBehaviour
{
    [Header("Shots")]
    public CinematicShot[] shots;

    [Header("Ending Configuration")]
    [Tooltip("If assigned, the camera will stay on this fixed camera after finishing.")]
    public FixedCamera endCamera;

    [Tooltip("If endCamera is null, assign this to follow the player after finishing.")]
    public FollowTarget followTarget;

    [Header("References")]
    public CameraManager cameraManager;
    public PlayerController playerController;

    [Header("Settings")]
    public bool fadeInOnStart = true;
    public float fadeInDuration = 0.5f;

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

        // Snap to first shot
        if (shots[0].camera != null)
        {
            var snap = shots[0].camera.GetSnapshot();
            cameraManager.mainCamera.transform.SetPositionAndRotation(snap.position, snap.rotation);
            cameraManager.mainCamera.fieldOfView = snap.fieldOfView;
        }

        if (fadeInOnStart)
            yield return StartCoroutine(GameStateManager.Instance.FadeIn(fadeInDuration));

        for (int i = 0; i < shots.Length; i++)
        {
            if (i > 0 && shots[i].camera != null)
                cameraManager.BlendToCamera(shots[i].camera);

            yield return new WaitForSeconds(shots[i].holdDuration);
        }

        Finish();
    }

    private void Finish()
    {
        Debug.Log("CINEMATIC FINISHED");

        // Logic check: Priority to endCamera, then followTarget, then Zones
        if (endCamera != null)
        {
            cameraManager.BlendToCamera(endCamera);
        }
        else if (followTarget != null)
        {
            cameraManager.EnterFollowMode(followTarget);
        }
        else
        {
            cameraManager.ExitFollowMode(); // Reverts to zone/fallback logic
        }

        // Always re-enable the player
        if (playerController)
            playerController.enabled = true;

        OnCinematicComplete?.Invoke();
    }
}