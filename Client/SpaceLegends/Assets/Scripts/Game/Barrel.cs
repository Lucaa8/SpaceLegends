using UnityEngine;

public class Barrel : MonoBehaviour
{

    [SerializeField] bool Enabled = false;
    [SerializeField] bool Breakable = false;

    [SerializeField] float Health;

    private float currentHealth;

    private Animator anim;

    [SerializeField] GameObject ItemToSpawn;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        anim = GetComponent<Animator>();
        currentHealth = Health;
    }

    public bool IsIntact()
    {
        return currentHealth > 0;
    }

    public bool IsBreakable()
    {
        return Breakable;
    }

    private void Break(bool spawn)
    {
        currentHealth = 0;
        anim.SetTrigger("Break");
        AudioManager.Instance.PlaySound(AudioManager.Instance.sfxDestroyBarrel);
        GetComponent<Rigidbody2D>().simulated = false;
        if (ItemToSpawn != null && spawn)
        {
            ItemToSpawn.transform.position = transform.position;
        }
    }

    public void Damage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Break(true);
        }
        else
        {
            anim.SetTrigger("Damage");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("DeadLine"))
        {
            return;
        }
        if(Enabled)
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            transform.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
        else
        {
            Break(false);
        }
    }

}
