using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieMono : MonoBehaviour, IResettable
{
    NavMeshAgent navAgent;
    public GameObject detectorObj;
    Detector detector;

    public GameObject target = null;

    public AudioSource audioSource;
    public AudioClip zombieDied_SFX;
    public AudioClip shot_SFX;

    private bool isDead = false;
    private bool paused = false;
    private Vector3 startingLoc;
    private Animator animator;
    private Collider collider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        paused = false;
        startingLoc = transform.position;
        navAgent = GetComponent<NavMeshAgent>();
        if(detectorObj != null) detector = detectorObj.GetComponent<Detector>();

        animator = GetComponent<Animator>();
        collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || paused) return;

        if (target == null && navAgent.enabled) {
            if(target == null && detector.target != null) {
                animator.SetTrigger("Alerted");
            }
            
            target = detector.target;
        }
            
        if(target != null && navAgent.enabled) {   
            animator.SetFloat("WalkRunBlend", navAgent.velocity.magnitude / navAgent.speed);
            navAgent.destination = target.transform.position;
        }
    }
    
    public bool GetIsDead() { return isDead; }

    public void FreezeAndDie()
    {
        if (isDead) return;
        isDead = true;

        // 1. Zombie needs to stop moving/attacking
        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
            navAgent.ResetPath();
        }

        if (audioSource != null && zombieDied_SFX != null)
        {
            Debug.Log("Zombie AudioSource and Died SFX exists!");
            //audioSource.clip = zombieDied_SFX;
            audioSource.PlayOneShot(shot_SFX);
            audioSource.PlayOneShot(zombieDied_SFX);
        }
        animator.SetTrigger("Dead");
        collider.enabled = false;
        //audioSource.enabled = false;

        // Animator
        //Debug.Log("Zombie detected! Destroying in 5s...");

        // 2. destroy after 5 seconds
        //Destroy(gameObject, 5f);


    }

    public void OnReset() {
        paused = false;
        if(navAgent.enabled) {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
            navAgent.ResetPath();
        }

        transform.position = startingLoc;
        detector.clearTarget();
        target = null;
        isDead = false;
        collider.enabled = true;
        animator.SetTrigger("Reset");
        
        

        navAgent.isStopped = false;
       
    }

    public void OnCollisionEnter(Collision collision) {
        if(collision.collider.tag == "Player" && !isDead) {
            if(navAgent.enabled) navAgent.isStopped = true;

            Player player = collision.rigidbody.gameObject.GetComponent<Player>();
            player.TriggerRespawnSequence();
        }
    }

    public void StopBehaviour() {
        paused = true;

        if(navAgent.enabled) {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
            navAgent.ResetPath();
        }
        detector.clearTarget();
        target = null;
    }

    public void StartBehaviour() {
        paused = false;
    }
}


