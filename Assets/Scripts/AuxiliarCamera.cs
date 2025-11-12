using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class AuxiliarCamera : MonoBehaviour
{

    public Vector3 target;
    public bool isEnabled;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isEnabled = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isEnabled)
        {
            //transform.position = target;
            //transform.position = Vector3.SmoothDamp(transform.position, target, ref newV3, 2, 5);

            //if(transform.position.y != target.y)
            //{
            //    Vector3 targetPosY = new Vector3(transform.position.x, target.y, transform.position.z);
            //    transform.position = Vector3.MoveTowards(transform.position, targetPosY, 3 * Time.deltaTime);
            //}
        }
    }

    public void SetTarget(Vector3 position)
    {
        target = position;
        transform.position = target;
    }
}
