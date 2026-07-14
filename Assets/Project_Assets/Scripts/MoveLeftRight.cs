using UnityEngine;

public class MoveLeftRight : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float distance = 5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float newX = startPosition.x + Mathf.PingPong(Time.time * speed, distance);
        transform.position = new Vector3(newX, startPosition.y, transform.position.z);
    }
}
