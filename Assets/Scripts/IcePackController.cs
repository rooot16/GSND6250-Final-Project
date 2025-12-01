using UnityEngine;

public class IcePackController : MonoBehaviour
{
    [Header("Status")]
    public float icePackDuration = 10f;

    [SerializeField]
    private float currentIcePackTimeLeft = 0f;

    private PlayerTemperature _tempController;

    void Awake()
    {
        _tempController = GetComponent<PlayerTemperature>();
    }

    void Update()
    {
        if (currentIcePackTimeLeft > 0f)
        {
            currentIcePackTimeLeft -= Time.deltaTime;

            if (currentIcePackTimeLeft <= 0f)
            {
                EndIcePackUse();
            }
        }
    }

    public void ActivateIcePack()
    {
        // reset 10 sec
        currentIcePackTimeLeft = icePackDuration;

        if (_tempController != null)
        {
            _tempController.isHoldingIcepack = true;
        }

        Debug.Log("Ice pack activated/refilled!");
    }

    private void EndIcePackUse()
    {
        currentIcePackTimeLeft = 0f;
        if (_tempController != null)
        {
            _tempController.isHoldingIcepack = false;
        }
        Debug.Log("Ice pack finished.");
    }
}