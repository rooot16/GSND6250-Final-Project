using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieMono : MonoBehaviour
{
    NavMeshAgent navAgent;
    public GameObject detectorObj;
    Detector detector;

    public GameObject target = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if(detectorObj != null) detector = detectorObj.GetComponent<Detector>();
    }

    // Update is called once per frame
    void Update()
    {
        if(target == null) target = detector.target;
        if(target != null)  navAgent.destination = target.transform.position;
    }
}
