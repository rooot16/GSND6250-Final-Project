using UnityEngine;

public class PlaySequenceAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    [Header("Timer Settings")]
    public float delayInSeconds = 2f;
    public float cooldownTime = 5f;

    [Header("Loop Settings")]
    public bool loop = true;

    private void Start()
    {
        if (audioSource1 == null || audioSource2 == null)
        {
            return;
        }

        StartCoroutine(PlayLoop());
    }

    private System.Collections.IEnumerator PlayLoop()
    {
        while (loop)
        {
            audioSource1.PlayDelayed(0f);
            audioSource2.PlayDelayed(delayInSeconds);

            yield return new WaitForSeconds(cooldownTime);
        }
    }
}
