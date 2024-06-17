using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{

    private Rigidbody2D player;
    private SpriteRenderer sprite;
    private Animator animator;
    private Vector3 initialScale;

    [SerializeField] GameObject StartPoint;
    [SerializeField] GameObject EndPoint;
    private float levelLength;

    [SerializeField] CheckpointController checkpointController;
    private Vector3 lastCheckpoint;

    public bool IsAlive { get; private set; } = true;
    [SerializeField] float MaxHealth;
    private float currentHealth;
    [SerializeField] float Damage;
    [SerializeField] float Armor;

    [SerializeField] float PassiveDamageAmount = .25f;
    private float damageInterval = .5f; // Time interval between each passive damage tick
    private float timeSinceLastDamage;
    private bool isTakingPassiveDamage = false;


    private void Start()
    {
        player = transform.GetComponent<Rigidbody2D>();
        transform.position = StartPoint.transform.position;
        sprite = transform.GetComponent<SpriteRenderer>();
        animator = transform.GetComponent<Animator>();    
        initialScale = transform.localScale;
        currentHealth = MaxHealth;
        lastCheckpoint = transform.position;
        levelLength = Mathf.Abs(EndPoint.transform.position.x - StartPoint.transform.position.x);
    }

    private void Update()
    {
        if (!IsAlive)
        {
            return;
        }
        if (currentHealth <= 0f)
        {
            Die(true);
        }
        TakeDamage();
        float playerPosition = player.position.x - StartPoint.transform.position.x;
        checkpointController.PositionPlayer(playerPosition / levelLength);
    }

    public void Die(bool animate)
    {
        IsAlive = false;
        if (animate)
        {
            animator.SetBool("IsDead", true);
        }
        else
        {
            //Display death ui here and same at the end of dead animation
            Respawn();
        }
        
    }

    //Called by the end of death sprite animation
    public void Respawn()
    {

        currentHealth = MaxHealth;
        player.velocity = Vector3.zero;
        transform.position = lastCheckpoint;
        IsAlive = true;
        animator.SetBool("IsDead", false);
    }

    private void TakeDamage()
    {
        if (isTakingPassiveDamage)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= damageInterval)
            {
                currentHealth -= PassiveDamageAmount;
                timeSinceLastDamage = 0f;
                StartCoroutine(ShowDamage(damageInterval / 2));
            }
        }
        else
        {
            timeSinceLastDamage = damageInterval;
        }
    }

    private IEnumerator ShowDamage(float seconds)
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(seconds);
        sprite.color = Color.white;
    }

    private bool isPassiveDamage(GameObject go)
    {
        return go.CompareTag("Spikes") || go.CompareTag("Campfire");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject g = collision.gameObject;
        if(isPassiveDamage(g) && !isTakingPassiveDamage)
        {
            isTakingPassiveDamage = true;
        }
        else if(g.CompareTag("Star"))
        {
            g.GetComponent<Animator>().SetBool("Pick", true);
            Debug.Log("New star!!");
        }
        else if(g.CompareTag("Checkpoint"))
        {
            CheckpointAnimations anim = g.GetComponent<CheckpointAnimations>();
            anim.Touch();
            collision.enabled = false;
            lastCheckpoint = collision.transform.position;
            checkpointController.PositionCheckpoint();
        }
        else if(g.CompareTag("DeadLine"))
        {
            Die(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isPassiveDamage(collision.gameObject) && isTakingPassiveDamage)
        {
            isTakingPassiveDamage = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isPassiveDamage(collision.gameObject) && !isTakingPassiveDamage)
        {
            isTakingPassiveDamage = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isPassiveDamage(collision.gameObject) && isTakingPassiveDamage)
        {
            isTakingPassiveDamage = false;
        }
    }

}
