using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Transform StartPoint;
    [SerializeField] Transform EndPoint;
    [SerializeField] Transform Platform;

    [SerializeField] float Speed = 2.5f;
    [SerializeField] float PauseDuration = 0.4f;

    private Vector2 currentTarget;
    private bool isPaused;

    public static bool Moving = true;

    void Start()
    {
        currentTarget = EndPoint.position;
        isPaused = false;
    }

    void Update()
    {

        if(!Moving)
        {
            return;
        }

        if (!isPaused)
        {
            MovePlatform();
        }
    }

    private void MovePlatform()
    {
        Platform.position = Vector2.MoveTowards(Platform.position, currentTarget, Speed * Time.deltaTime);

        if (Vector2.Distance(Platform.position, currentTarget) <= 0.1f)
        {
            StartCoroutine(PauseBeforeSwitching());
        }
    }

    private IEnumerator PauseBeforeSwitching()
    {
        isPaused = true;
        yield return new WaitForSeconds(PauseDuration);
        UpdateTarget();
        isPaused = false;
    }

    private void UpdateTarget()
    {
        currentTarget = currentTarget == (Vector2)EndPoint.position ? StartPoint.position : EndPoint.position;
    }

    private void OnDrawGizmos()
    {
        if (StartPoint == null || EndPoint == null || Platform == null) return;
        Gizmos.DrawLine(Platform.position, StartPoint.position);
        Gizmos.DrawLine(Platform.position, EndPoint.position);
    }

}
