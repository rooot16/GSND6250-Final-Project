using UnityEngine;

public class Door : MonoBehaviour, Interaction.IInteractable
{
    public float openDegree = 60f;
    public float rotationSpeed = 25f;
 
    public bool state {
        get {
            return _state;
        }
        set {
            _state = value;
        }
    }

    private bool _state;
    private bool isMoving = false;
    private Vector3 targetAngle;
    private Vector3 originalAngle;
    private Vector3 currentAngle;
    private Rigidbody rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        originalAngle = rigidbody.rotation.eulerAngles;
        currentAngle = originalAngle;
        targetAngle = originalAngle;
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 difference = targetAngle - currentAngle;
        //Debug.Log(targetAngle);
        if(Mathf.Abs(difference.magnitude) > 1f) {
            Vector3 rotationAmount = rotationSpeed * Time.deltaTime * difference.normalized;
            if(Mathf.Abs(difference.magnitude) < Mathf.Abs(rotationAmount.magnitude)) {
                currentAngle = targetAngle;
                rigidbody.rotation = Quaternion.Euler(targetAngle);
            } else {
                currentAngle = currentAngle + rotationAmount;
                rigidbody.rotation = Quaternion.Euler(currentAngle);
            }
        }
    }

    public void setState(bool newState) {
        if(_state = newState) {
            if(newState) targetAngle = new Vector3(originalAngle.x, originalAngle.y + openDegree, originalAngle.z);
            else targetAngle = originalAngle;
        }
    }

    void Interaction.IInteractable.OnInteract(Interaction.IInteractor interactor) {
        setState(true);
    }
}
