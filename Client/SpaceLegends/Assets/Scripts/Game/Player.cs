using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] GameObject PlayerGO;
    private Rigidbody2D player;
    private SpriteRenderer sprite;

    private bool isAlive = true;
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
        player = GetComponent<Rigidbody2D>();
        sprite = transform.GetComponent<SpriteRenderer>();
        currentHealth = MaxHealth;
    }

    private void Update()
    {
        TakeDamage();
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
        if(isPassiveDamage(collision.gameObject) && !isTakingPassiveDamage)
        {
            isTakingPassiveDamage = true;
        }
        else if(collision.gameObject.CompareTag("Star"))
        {
            collision.gameObject.GetComponent<Animator>().SetBool("Pick", true);
            Debug.Log("New star!!");
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
