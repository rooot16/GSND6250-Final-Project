using UnityEngine;
using UnityEngine.Audio;

public class PlayRandomAudioZombie : MonoBehaviour
{
    [Header("Audio Components")]
    public AudioSource audioSource;
    public AudioClip clip1;
    public AudioClip clip2;

    [Header("Timer Settings")]
    public float cooldownTime = 5f;

    [Header("Loop Settings")]
    public bool loop = true;

    private ZombieMono zombie;
    private AudioClip currentAudioClip;

    private void Start()
    {

        zombie = GetComponent<ZombieMono>();

        if (clip1 == null || clip2 == null)
        {
            return;
        }

        StartCoroutine(PlayRandomAudioLoop());
    }

    private System.Collections.IEnumerator PlayRandomAudioLoop()
    {
        bool isDead = zombie.GetIsDead();
        while (!isDead)
        {
            PlayRandomClip();
            yield return new WaitForSeconds(currentAudioClip.length + cooldownTime);
            isDead = zombie.GetIsDead();
            Debug.Log("Zombie is " + isDead);
        }
    }

    private void PlayRandomClip()
    {
        currentAudioClip = Random.value < 0.5f ? clip1 : clip2;
        audioSource.clip = currentAudioClip;
        audioSource.Play();
    }
}
