using UnityEngine;
using System.Collections;
using System;

public class Bark : MonoBehaviour
{
    public AudioClip[] clips;
    public float minDelay = 5f;
    public float maxDelay = 15f;

    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        StartCoroutine(PlayRandomClips());
    }

    IEnumerator PlayRandomClips()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                UnityEngine.Random.Range(minDelay, maxDelay)
            );

            if (clips.Length > 0)
            {
                source.clip = clips[UnityEngine.Random.Range(0, clips.Length)];
                source.Play();
            }
        }
    }
}