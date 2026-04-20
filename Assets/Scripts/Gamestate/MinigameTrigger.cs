using System.Diagnostics;
using UnityEngine;

public class MinigameTrigger : MonoBehaviour
{
    public string minigameSceneName;

    public bool loadAdditive = true;

    public bool autoTrigger = false;
    public bool disableOnComplete = true;

    private bool _completed;

    private void OnTriggerEnter(Collider other)
    {
        if (!autoTrigger) return;
        if (other.CompareTag("Player")) StartMinigame(skipFade: false);
    }

    public void Interact() => StartMinigame(skipFade: false);


    public void InteractNoFade() => StartMinigame(skipFade: true);

    private void StartMinigame(bool skipFade)
    {
        if (_completed) return;
        if (string.IsNullOrEmpty(minigameSceneName))
        {
            return;
        }

        if (loadAdditive)
        {
            GameStateManager.Instance.EnterMinigame(
                minigameSceneName,
                skipFadeOut: skipFade,
                onComplete: () =>
                {
                    _completed = true;
                    if (disableOnComplete) gameObject.SetActive(false);
                });
        }
        else
        {
            GameStateManager.Instance.EnterMinigameStandalone(
                minigameSceneName,
                skipFadeOut: skipFade);
        }
    }
}