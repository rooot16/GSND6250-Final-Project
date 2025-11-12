using UnityEngine;

public class PlayAndSwap : MonoBehaviour
{
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public float swapInterval = 30f; // seconds between swaps

    private float timer;
    private bool isFirstPlaying = true;

    void Start()
    {
        // Ensure only the first one starts playing
        if (audioSource1 != null) audioSource1.Play();
        if (audioSource2 != null) audioSource2.Stop();

        timer = swapInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Swap();
            timer = swapInterval; // reset timer
        }
    }

    void Swap()
    {
        if (audioSource1 == null || audioSource2 == null)
        {
            Debug.LogWarning("Assign both AudioSources in the inspector!");
            return;
        }

        if (isFirstPlaying)
        {
            audioSource1.Stop();
            audioSource2.Play();
        }
        else
        {
            audioSource2.Stop();
            audioSource1.Play();
        }

        isFirstPlaying = !isFirstPlaying;
    }
}
