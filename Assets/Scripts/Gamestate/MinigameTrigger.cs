using UnityEngine;

/// <summary>
/// Place on any world object to start a minigame when the player interacts with it.
/// Handles both trigger-based (walk up to) and interaction-based (press button) entry.
/// </summary>
public class MinigameTrigger : MonoBehaviour
{
    [Header("Minigame")]
    [Tooltip("The name of the scene to load. Must be added to Build Settings.")]
    public string minigameSceneName;

    [Tooltip("If true the minigame starts automatically when the player enters the trigger. " +
             "If false, call Interact() manually from your interaction system.")]
    public bool autoTrigger = false;

    [Header("On Complete")]
    [Tooltip("Disable this trigger after the minigame is completed once " +
             "(e.g. a puzzle that only needs solving once).")]
    public bool disableOnComplete = true;

    private bool _completed;

    private void OnTriggerEnter(Collider other)
    {
        if (!autoTrigger) return;
        if (other.CompareTag("Player"))
            StartMinigame();
    }

    /// <summary>
    /// Call this from your interaction system when the player presses the interact button.
    /// </summary>
    public void Interact()
    {
        StartMinigame();
    }

    private void StartMinigame()
    {
        if (_completed) return;
        if (string.IsNullOrEmpty(minigameSceneName))
        {
            Debug.LogWarning($"MinigameTrigger on {gameObject.name} has no scene name set.");
            return;
        }

        GameStateManager.Instance.EnterMinigame(minigameSceneName, onComplete: () =>
        {
            _completed = true;
            if (disableOnComplete) gameObject.SetActive(false);
        });
    }
}
