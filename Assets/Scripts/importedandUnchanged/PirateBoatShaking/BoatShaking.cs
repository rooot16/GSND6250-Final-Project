using UnityEngine;
using System.Collections;

public class BoatShaking : MonoBehaviour
{
    public float cycleDuration = 5f;
    public float smoothFactor = 5f;
    public float minSpeed = 0.3f;
    public float maxSpeed = 0.8f;

    [HideInInspector] public float CurrentTimeT;
    [HideInInspector] public float CurrentSpeed;

    private float targetSpeed;
    private float currentTime;

    public float boatPositionAmplitude = 0.05f;
    public float boatRotationAmplitude = 3.0f;

    private Vector3 originalBoatLocalPosition;
    private Quaternion originalBoatLocalRotation;

    private float xOffsetStart;
    private float yOffsetStart;
    private float zOffsetStart;
    private float rotXOffsetStart;
    private float rotZOffsetStart;


    void Awake()
    {
        SetNewTargets();
        CurrentSpeed = targetSpeed;
        StartCoroutine(CycleParameters());

        originalBoatLocalPosition = transform.localPosition;
        originalBoatLocalRotation = transform.localRotation;

        xOffsetStart = Random.Range(100f, 200f);
        yOffsetStart = Random.Range(200f, 300f);
        zOffsetStart = Random.Range(300f, 400f);
        rotXOffsetStart = Random.Range(400f, 500f);
        rotZOffsetStart = Random.Range(500f, 600f);
    }

    void Update()
    {
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed, Time.deltaTime * smoothFactor);

        currentTime += Time.deltaTime * CurrentSpeed;
        CurrentTimeT = currentTime;
    }

    void LateUpdate()
    {
        float time = CurrentTimeT;

        float noiseX = (Mathf.PerlinNoise(xOffsetStart + time, 0f) * 2f - 1f);
        float noiseY = (Mathf.PerlinNoise(yOffsetStart + time, 0f) * 2f - 1f);
        float noiseZ = (Mathf.PerlinNoise(zOffsetStart + time, 0f) * 2f - 1f);

        Vector3 positionOffset = new Vector3(noiseX, noiseY, noiseZ) * boatPositionAmplitude;
        transform.localPosition = originalBoatLocalPosition + positionOffset;

        float rotX = (Mathf.PerlinNoise(rotXOffsetStart + time, 0f) * 2f - 1f);
        float rotZ = (Mathf.PerlinNoise(rotZOffsetStart + time, 0f) * 2f - 1f);

        Quaternion rotationOffset = Quaternion.Euler(
            rotX * boatRotationAmplitude,
            0,
            rotZ * boatRotationAmplitude
        );

        transform.localRotation = originalBoatLocalRotation * rotationOffset;
    }


    IEnumerator CycleParameters()
    {
        while (true)
        {
            yield return new WaitForSeconds(cycleDuration);
            SetNewTargets();
        }
    }

    void SetNewTargets()
    {
        targetSpeed = Random.Range(minSpeed, maxSpeed);
    }
}