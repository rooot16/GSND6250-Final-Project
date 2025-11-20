using UnityEngine;

public class Detector : MonoBehaviour
{
    public GameObject target => _target;
    GameObject _target = null;

    void OnTriggerEnter(Collider collider) {
        if(collider.gameObject.tag == "Player") {
            _target = collider.gameObject;
            Debug.Log("Yee");
        }
    }

    void OnTriggerExit(Collider collider) {
        if(collider.gameObject == _target) _target = null;
    }
}
