using UnityEngine;

public class ShampooSponge : BathTool
{
    public float foamRadius = 0.4f;

    protected override void Update()
    {
        base.Update();


        if (_isHeld && BathMinigame.Instance.CurrentState == BathState.Soaping)
            FoamSystem.Instance.UpdateSpongePosition(transform.position, foamRadius);
        else
            FoamSystem.Instance.ClearSpongeContact();
    }


    protected override void OnHeld() { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, foamRadius);
    }
}