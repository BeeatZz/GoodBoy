using UnityEngine;

public class QuiverEffect : MonoBehaviour
{

    public float intensity = 0.1f;
    public float speed = 50f;
    public bool isQuivering = true;
    private Vector3 _originalPosition;
    private Vector3 _targetPosition;

    void Start()
    {
        _originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (isQuivering)
        {
            DoQuiver();
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _originalPosition, Time.deltaTime * 5f);
        }
    }

    void DoQuiver()
    {
        _targetPosition = _originalPosition + Random.insideUnitSphere * intensity;
        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPosition, Time.deltaTime * speed);
    }
}