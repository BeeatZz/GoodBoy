using UnityEngine;

public class ShowerHead : BathTool
{
    [Header("Rinsing")]
    [Tooltip("World-space radius in which foam zones are cleared per tick.")]
    public float rinseRadius = 0.5f;

    [Tooltip("Seconds between rinse ticks while dragging.")]
    public float rinseRate = 0.05f;

    [Header("Water Particles")]
    [Tooltip("Optional particle system that plays while the shower is held.")]
    public ParticleSystem waterParticles;

    private float _nextRinseTime;

    protected override void Update()
    {
        base.Update();

        // Drive water particles with held state
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

        // Once all foam is gone the bath is done
        if (FoamSystem.Instance.AllFoamCleared())
            BathMinigame.Instance.OnRinsingComplete();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rinseRadius);
    }
}
