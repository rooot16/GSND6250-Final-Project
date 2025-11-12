using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GhostSpirit : MonoBehaviour
{
    [SerializeField]
    public float speed = 0.1f;

    [SerializeField]
    public float smoothTime = 0.3f;

    private GameObject player;
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        targetPosition = player.transform.position;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, speed);
    }
}
