using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    private Connection connection;
    private Rigidbody2D player;
    private SpriteRenderer sprite;
    private Animator animator;
    private Vector3 initialScale;

    [SerializeField] GameObject StartPoint;
    [SerializeField] GameObject EndPoint;
    private float levelLength;

    [SerializeField] CheckpointController checkpointController;
    private Vector3 lastCheckpoint;

    [SerializeField] GameObject DeathScreen;
    [SerializeField] TMP_Text DeathScreenRemaining;
    [SerializeField] CanvasGroup RespawnButton;

    public bool IsAlive { get; private set; } = true;
    [SerializeField] float MaxHealth;
    private float currentHealth;
    [SerializeField] Image ImageHealth;
    [SerializeField] float Damage;
    [SerializeField] float Armor;

    [SerializeField] float PassiveDamageAmount = .25f;
    private float damageInterval = .5f; // Time interval between each passive damage tick
    private float timeSinceLastDamage;
    private bool isTakingPassiveDamage = false;

    private int kills;
    private int deaths;
    private int stars;
    private int livesLeft = 0;

    private void Start()
    {
        connection = transform.GetComponent<Connection>();
        player = transform.GetComponent<Rigidbody2D>();
        transform.position = StartPoint.transform.position;
        sprite = transform.GetComponent<SpriteRenderer>();
        animator = transform.GetComponent<Animator>();    
        initialScale = transform.localScale;
        currentHealth = MaxHealth;
        lastCheckpoint = transform.position;
        levelLength = Mathf.Abs(EndPoint.transform.position.x - StartPoint.transform.position.x);
        UpdateHealth();
        connection.GetLives((j) =>
        {
            livesLeft = j.Value<int>("count");
        });
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
        connection.AddDeath();
        if (animate)
        {
            animator.SetBool("IsDead", true);
        }
        else
        {
            transform.position = lastCheckpoint; //Reset position of the player to avoid him to see unwanted background transitions
            ShowDeathScreen();
        }
    }

    public void ShowDeathScreen()
    {
        DeathScreenRemaining.text = "You have " + livesLeft.ToString();
        RespawnButton.alpha = livesLeft <= 0 ? 0.5f : 1f;
        RespawnButton.interactable = livesLeft > 0;
        StartCoroutine(SetDeathScreen(true));
    }

    //Called by the end of death sprite animation
    public void TryRespawn()
    {
        connection.DecreaseLives((j) =>
        {
            if(j.Value<bool>("status"))
            {
                Respawn();
            }
        });
        livesLeft--;
    }

    private void Respawn()
    {
        StartCoroutine(SetDeathScreen(false));
        currentHealth = MaxHealth;
        UpdateHealth();
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
                TakeDamage(PassiveDamageAmount);
                timeSinceLastDamage = 0f;
            }
        }
        else
        {
            timeSinceLastDamage = damageInterval;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealth();
        StartCoroutine(ShowDamage(damageInterval / 2));
    }

    private void UpdateHealth()
    {
        float ratio = currentHealth / MaxHealth;
        ImageHealth.transform.localScale = new Vector2(ratio < 0f ? 0f : ratio, ImageHealth.transform.localScale.y);
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
            stars++;
            connection.AddStar();
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

    private IEnumerator SetDeathScreen(bool show)
    {
        if (show) //Cannot do DeathScreen.SetActive(show); because in the false case, the gameobject would be deactivated before the canvas opacity animation played.
        {
            DeathScreen.SetActive(true);
        }

        float duration = 0.3f; // seconds
        float elapsedTime = 0f;
        CanvasGroup group = DeathScreen.GetComponent<CanvasGroup>();
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / duration;
            group.alpha = show ? alpha : 1f - alpha;
            yield return null;
        }
        group.alpha = show ? 1f : 0f;

        if (!show)
        {
            DeathScreen.SetActive(false);
        }
    }

}
