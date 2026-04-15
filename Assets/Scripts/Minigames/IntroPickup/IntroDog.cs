using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

public class IntroDog : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI promptText;
    public string promptMessage = "Press E to pick up";

    [Header("Timing")]
    public float animationWaitDuration = 2f;
    public float blackScreenWaitDuration = 2f;
    public float fadeDuration = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] blackScreenSounds;

    [Header("Black Screen Text")]
    public TextMeshProUGUI blackScreenText;
    public string[] blackScreenLines;
    public float timeBetweenLines = 1f;

    [Header("Sequence — Player")]
    public PlayerController playerController;
    public Vector3 newPlayerScale = Vector3.one;
    public float newMoveSpeed = 5f;
    public Transform teleportTarget;

    [Header("Sequence — Camera")]
    public FixedCamera postTeleportCamera;

    [Header("Sequence — World")]
    public GameObject objectToActivate;

    [Header("Events")]
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
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            HidePrompt();
        }
    }

    private IEnumerator PlayPickupSequence()
    {
        // Step 1 — Pickup pressed, disable controls
        Debug.Log("[IntroDog] Pickup pressed.");
        if (playerController != null) playerController.enabled = false;
        OnPickupPressed?.Invoke();

        // Step 2 — Play animation
        Debug.Log("[IntroDog] Playing pickup animation.");
        OnAnimationStart?.Invoke();
        yield return new WaitForSeconds(animationWaitDuration);

        // Step 3 — Fade to black
        Debug.Log("[IntroDog] Fading to black.");
        OnFadeToBlack?.Invoke();
        yield return StartCoroutine(GameStateManager.Instance.FadeOut(fadeDuration));

        // Step 4 — Black screen: sounds, text & hold
        Debug.Log("[IntroDog] Black screen started.");
        OnBlackScreenStart?.Invoke();

        if (blackScreenText != null)
            blackScreenText.text = "";

        if (audioSource != null && blackScreenSounds.Length > 0)
        {
            foreach (var clip in blackScreenSounds)
            {
                if (clip == null) continue;
                Debug.Log($"[IntroDog] Playing sound: {clip.name}");
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

        // Step 5 — Rescale player
        if (playerController != null)
        {
            Debug.Log($"[IntroDog] Rescaling player to {newPlayerScale}.");
            playerController.transform.localScale = newPlayerScale;
        }

        // Step 6 — Change move speed
        if (playerController != null)
        {
            Debug.Log($"[IntroDog] Setting move speed to {newMoveSpeed}.");
            playerController.moveSpeed = newMoveSpeed;
        }

        // Step 7 — Teleport player
        // Step 7 — Teleport player
        if (playerController != null && teleportTarget != null)
        {
            Debug.Log($"[IntroDog] Teleporting player to {teleportTarget.position}.");

            // Clear old zones so their OnTriggerExit doesn't fight the new camera
            CameraManager.Instance.ClearActiveZones();

            var cc = playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerController.transform.SetPositionAndRotation(teleportTarget.position, teleportTarget.rotation);
            if (cc != null) cc.enabled = true;
        }

        // Step 8 — Switch camera and activate any zones at the new location
        if (postTeleportCamera != null)
        {
            Debug.Log("[IntroDog] Switching to post-teleport camera.");
            CameraManager.Instance.BlendToCamera(postTeleportCamera);
        }

        // Manually trigger any CameraZones the player landed inside
        var colliders = Physics.OverlapSphere(playerController.transform.position, 1f);
        foreach (var col in colliders)
        {
            var zone = col.GetComponent<CameraZone>();
            if (zone != null)
            {
                Debug.Log($"[IntroDog] Force-activating zone: {zone.gameObject.name}");
                zone.HandlePlayerEntry();
            }
        }

        // Step 9 — Activate world object
        if (objectToActivate != null)
        {
            Debug.Log($"[IntroDog] Activating {objectToActivate.name}.");
            objectToActivate.SetActive(true);
        }

        // Step 10 — Fade back in
        Debug.Log("[IntroDog] Fading in.");
        yield return StartCoroutine(GameStateManager.Instance.FadeIn(fadeDuration));

        // Step 11 — Re-enable controls
        if (playerController != null)
        {
            Debug.Log("[IntroDog] Re-enabling player controls.");
            playerController.enabled = true;
        }

        Debug.Log("[IntroDog] Sequence complete.");
        OnSequenceComplete?.Invoke();
    }

    private void ShowPrompt()
    {
        if (promptText != null)
            promptText.text = promptMessage;
    }

    private void HidePrompt()
    {
        if (promptText != null)
            promptText.text = "";
    }
}