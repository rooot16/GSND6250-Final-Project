using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieMono : MonoBehaviour
{
    NavMeshAgent navAgent;
    public GameObject detectorObj;
    Detector detector;

    public GameObject target = null;

    private bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if(detectorObj != null) detector = detectorObj.GetComponent<Detector>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        if (target == null) target = detector.target;
        if(target != null)  navAgent.destination = target.transform.position;
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
            navAgent.enabled = false;
        }

        // Animator
        Debug.Log("Zombie detected! Destroying in 5s...");

        // 2. destroy after 5 seconds
        Destroy(gameObject, 5f);
    }
}
