using UnityEngine;
using System;
public class MinigameController : MonoBehaviour
{
    private Action _onComplete;

    private bool _finished;


    public void Initialise(Action onComplete) => _onComplete = onComplete;


    public void Complete()
    {
        if (_finished) return;
        _finished = true;
        GameStateManager.Instance.ExitMinigame(_onComplete);
    }

  
    public void Abort()
    {
        if (_finished) return;
        _finished = true;
        GameStateManager.Instance.ExitMinigame(onComplete: null);
    }
}
