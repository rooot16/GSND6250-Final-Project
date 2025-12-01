using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieMono : MonoBehaviour, IResettable
{
    NavMeshAgent navAgent;
    public GameObject detectorObj;
    Detector detector;

    public GameObject target = null;


    private bool isDead = false;
    
    private Vector3 startingLoc;
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingLoc = transform.position;
        navAgent = GetComponent<NavMeshAgent>();
        if(detectorObj != null) detector = detectorObj.GetComponent<Detector>();

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

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

        animator.SetTrigger("Dead");

        // Animator
        //Debug.Log("Zombie detected! Destroying in 5s...");

        // 2. destroy after 5 seconds
        //Destroy(gameObject, 5f);


    }

    public void OnReset() {
        if(navAgent.enabled) {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
            navAgent.ResetPath();
        }

        transform.position = startingLoc;
        detector.clearTarget();
        target = null;
        isDead = false;
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
}


