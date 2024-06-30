using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{

    [SerializeField] Transform Destination;

    private Collider2D player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            player = collision;
            return;
        }
        // Avoid teleporting back instant if destination is another portal
        if (Vector2.Distance(collision.transform.position, transform.position) > .3f)
        {
            StartCoroutine(Teleport(collision));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player = null;
        }
    }

    private void Update()
    {
        if (player != null && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Teleport(player));
            player = null;
        }
    }

    private IEnumerator Teleport(Collider2D collider)
    {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
        Animator animator = collider.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("Teleport_In");
        }
        StartCoroutine(MoveInPortal(collider));
        yield return new WaitForSeconds(.5f);
        collider.transform.position = Destination.position;
        if(rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        if (animator != null)
        {
            animator.Play("Teleport_Out");
        }
        yield return new WaitForSeconds(.5f);
        if (rb != null)
        {
            rb.simulated = true;
        }
    }

    private IEnumerator MoveInPortal(Collider2D collider)
    {
        float timer = 0;
        while(timer < .5f)
        {
            collider.transform.position = Vector2.MoveTowards(collider.transform.position, transform.position, 3 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
    }

}
