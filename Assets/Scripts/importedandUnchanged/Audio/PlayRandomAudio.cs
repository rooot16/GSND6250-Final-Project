using UnityEngine;

public class PlayRandomAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    [Header("Timer Settings")]
    public float cooldownTime = 5f;

    [Header("Loop Settings")]
    public bool loop = true;

    private AudioSource currentAudioSource;

    private void Start()
    {
        if (audioSource1 == null || audioSource2 == null)
        {
            return;
        }

        StartCoroutine(PlayRandomAudioLoop());
    }

    private System.Collections.IEnumerator PlayRandomAudioLoop()
    {
        while (loop)
        {
            PlayRandomClip();
            yield return new WaitForSeconds(currentAudioSource.clip.length + cooldownTime);
        }
    }

    private void PlayRandomClip()
    {
        currentAudioSource = Random.value < 0.5f ? audioSource1 : audioSource2;
        currentAudioSource.Play();
    }
}
