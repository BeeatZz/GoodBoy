using UnityEngine;

public class ShowerHead : BathTool
{
    public float rinseRadius = 0.5f;
    public float rinseRate = 0.05f;
    public ParticleSystem waterParticles;

    private float _nextRinseTime;

    protected override void Update()
    {
        base.Update();

        if (waterParticles)
        {
            if (_isHeld && !waterParticles.isPlaying) waterParticles.Play();
            if (!_isHeld && waterParticles.isPlaying)  waterParticles.Stop();
        }
    }

    protected override void OnHeld()
    {
        if (BathMinigame.Instance.CurrentState != BathState.Rinsing) return;
        if (Time.time < _nextRinseTime) return;

        FoamSystem.Instance.RinseAtPosition(transform.position, rinseRadius);
        _nextRinseTime = Time.time + rinseRate;

        if (FoamSystem.Instance.AllFoamCleared())
            BathMinigame.Instance.OnRinsingComplete();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rinseRadius);
    }
}
