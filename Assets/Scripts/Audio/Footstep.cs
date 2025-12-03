using UnityEngine;
using System.Collections;

public class Footstep : MonoBehaviour
{
    public GameObject target;
    private AudioSource source;
    public float footstepIntervalDistance = 5f;
    public float velocityThreshold = 2f;
    private AudioClip footStep;
    private Rigidbody rigidbody;

    void Awake() {

    }
    void Start() {
        source = GetComponent<AudioSource>();
        rigidbody = target.GetComponent<Rigidbody>();
        footStep = source.clip;
        if(footStep != null) StartCoroutine(playFootStep());
    }
    private IEnumerator playFootStep() {
        if(footStep.loadInBackground != true) footStep.LoadAudioData();

        while(footStep.loadState == AudioDataLoadState.Loading) {
            yield return null;
        }

        while(rigidbody.linearVelocity.x == 0 && rigidbody.linearVelocity.z == 0) {
            yield return null;
        }
        source.Play();

        Vector2 prevPos = new Vector2(rigidbody.position.x, rigidbody.position.z);
        Vector2 currentPos;

        float sum = 0;

        while(true) {
            currentPos = new Vector2(rigidbody.position.x, rigidbody.position.z);
            sum += (prevPos - currentPos).magnitude;
            
            if(sum >= footstepIntervalDistance && !source.isPlaying && rigidbody.linearVelocity.magnitude >= velocityThreshold) {
                sum = 0;
                source.Play();
            }
            prevPos = currentPos;
            yield return null;
        }
    }
}
