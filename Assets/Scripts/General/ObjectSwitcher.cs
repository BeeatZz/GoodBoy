using UnityEngine;
using System.Collections;

public class ObjectSwitcher : MonoBehaviour
{
    public GameObject objectToTurnOff;
    public GameObject objectToTurnOn;

    public void StartSwitchTimer()
    {
        StartCoroutine(SwitchAfterDelay());
    }

    private IEnumerator SwitchAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        objectToTurnOff.SetActive(false);
        objectToTurnOn.SetActive(true);
    }
}