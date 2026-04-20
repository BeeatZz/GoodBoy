using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

public class IntroDog : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public string promptMessage = "Press E to pick up";
    public float animationWaitDuration = 2f;
    public float blackScreenWaitDuration = 2f;
    public float fadeDuration = 0.5f;
    public AudioSource audioSource;
    public AudioClip[] blackScreenSounds;
    public TextMeshProUGUI blackScreenText;
    public string[] blackScreenLines;
    public float timeBetweenLines = 1f;
    public PlayerController playerController;
    public Vector3 newPlayerScale = Vector3.one;
    public float newMoveSpeed = 5f;
    public float newGravity = -2f;
    public Transform teleportTarget;
    public FixedCamera postTeleportCamera;
    public GameObject objectToActivate;
    public bool skipToMinigame = false;
    public MinigameTrigger minigameTrigger;
    public UnityEvent OnPickupPressed;
    public UnityEvent OnAnimationStart;
    public UnityEvent OnFadeToBlack;
    public UnityEvent OnBlackScreenStart;
    public UnityEvent OnSequenceComplete;

    private bool _playerInRange = false;
    private bool _hasBeenPickedUp = false;

    private void Update()
    {
        if (_playerInRange && !_hasBeenPickedUp && Keyboard.current.eKey.wasPressedThisFrame)
        {
            _hasBeenPickedUp = true;
            HidePrompt();
            StartCoroutine(PlayPickupSequence());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) { _playerInRange = true; ShowPrompt(); }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) { _playerInRange = false; HidePrompt(); }
    }

    private IEnumerator PlayPickupSequence()
    {
        if (playerController != null) playerController.enabled = false;
        OnPickupPressed?.Invoke();
        OnAnimationStart?.Invoke();
        yield return new WaitForSeconds(animationWaitDuration);
        OnFadeToBlack?.Invoke();
        yield return StartCoroutine(GameStateManager.Instance.FadeOut(fadeDuration));
        OnBlackScreenStart?.Invoke();

        if (blackScreenText != null) blackScreenText.text = "";

        if (audioSource != null && blackScreenSounds.Length > 0)
        {
            foreach (var clip in blackScreenSounds)
            {
                if (clip == null) continue;
                audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length);
            }
        }
        else
        {
            yield return new WaitForSeconds(blackScreenWaitDuration);
        }

        if (blackScreenText != null && blackScreenLines.Length > 0)
        {
            foreach (var line in blackScreenLines)
            {
                blackScreenText.text = line;
                yield return new WaitForSeconds(timeBetweenLines);
            }
            blackScreenText.text = "";
        }

        if (skipToMinigame)
        {
            if (minigameTrigger != null) minigameTrigger.InteractNoFade();
            yield break;
        }

        if (playerController != null)
        {
            playerController.transform.localScale = newPlayerScale;
            playerController.moveSpeed = newMoveSpeed;
            playerController.gravity = newGravity;
        }

        if (playerController != null && teleportTarget != null)
        {
            CameraManager.Instance.ClearActiveZones();
            var cc = playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerController.transform.SetPositionAndRotation(teleportTarget.position, teleportTarget.rotation);
            if (cc != null) cc.enabled = true;
        }

        if (postTeleportCamera != null)
        {
            var snap = postTeleportCamera.GetSnapshot();
            CameraManager.Instance.mainCamera.transform.SetPositionAndRotation(snap.position, snap.rotation);
            CameraManager.Instance.mainCamera.fieldOfView = snap.fieldOfView;
        }

        CameraManager.Instance.ClearActiveZones();
        var colliders = Physics.OverlapSphere(playerController.transform.position, 1f);
        foreach (var col in colliders)
        {
            var zone = col.GetComponent<CameraZone>();
            if (zone != null) zone.HandlePlayerEntry();
        }

        if (objectToActivate != null) objectToActivate.SetActive(true);

        yield return StartCoroutine(GameStateManager.Instance.FadeIn(fadeDuration));
        if (playerController != null) playerController.enabled = true;
        OnSequenceComplete?.Invoke();
    }

    private void ShowPrompt() { if (promptText != null) promptText.text = promptMessage; }
    private void HidePrompt() { if (promptText != null) promptText.text = ""; }
}