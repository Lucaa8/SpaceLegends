using UnityEngine;

public class ScrollCamera : MonoBehaviour
{
    public float scrollSpeed = 2f;
    private Vector3 startPoint;
    public Transform endPoint;
    private int direction = 1;

    private void Start()
    {
        startPoint = transform.position;
    }

    void Update()
    {
        transform.position += new Vector3(direction, 0) * scrollSpeed * Time.deltaTime;

        if (direction == 1 && transform.position.x >= endPoint.position.x)
        {
            direction = -1;
        }

        if (direction == -1 && transform.position.x <= startPoint.x)
        {
            direction = 1;
        }

    }
}
