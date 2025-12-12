using UnityEngine;

public class OpenLid : MonoBehaviour, IResettable
{
    [Header("Lid Settings")]
    public float openAngle = -90f;        // How much to rotate on Y
    public float openSpeed = 2f;         // How fast the lid opens
    public bool isOpen = false;          // Toggle

    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        closedRot = transform.localRotation;

        openRot = closedRot * Quaternion.AngleAxis(openAngle, Vector3.up);
    }

    void Update()
    {
        Quaternion target = isOpen ? openRot : closedRot;

        transform.localRotation =
            Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * openSpeed);
    }

    public void ToggleLid()
    {
        isOpen = !isOpen;
    }

    //Close Lid on Reset
    public void OnReset() {
        isOpen = false;
        openRot = closedRot * Quaternion.AngleAxis(openAngle, Vector3.up);


        //Get all icepack script components in children 
        
        IcePackItem[] icePacks = GetComponentsInChildren<IcePackItem>(true);
        foreach(IcePackItem pack in icePacks) {
            pack.gameObject.SetActive(true);
        }
    }
}
