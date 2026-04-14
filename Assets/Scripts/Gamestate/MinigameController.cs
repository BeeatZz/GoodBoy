using UnityEngine;
using System;
public class MinigameController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Fired when the minigame scene is done and control should return to the world.")]
    private Action _onComplete;

    private bool _finished;

    /// <summary>
    /// Inject the completion callback. GameStateManager calls this automatically —
    /// you don't need to call it yourself.
    /// </summary>
    public void Initialise(Action onComplete) => _onComplete = onComplete;

    /// <summary>
    /// Call this when the player successfully finishes the minigame.
    /// </summary>
    public void Complete()
    {
        if (_finished) return;
        _finished = true;
        GameStateManager.Instance.ExitMinigame(_onComplete);
    }

    /// <summary>
    /// Call this if the minigame can be exited early (skipped, failed, etc.)
    /// without triggering the success callback.
    /// </summary>
    public void Abort()
    {
        if (_finished) return;
        _finished = true;
        GameStateManager.Instance.ExitMinigame(onComplete: null);
    }
}
