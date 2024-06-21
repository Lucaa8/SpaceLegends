using UnityEngine;

public class Ennemy : MonoBehaviour
{

    [SerializeField] float attackCooldown;
    [SerializeField] float range;
    [SerializeField] float colliderDistance;
    [SerializeField] int damage;
    [SerializeField] BoxCollider2D boxCollider;
    [SerializeField] LayerMask playerLayer;
    private float cooldownTimer = Mathf.Infinity;

    private Animator anim;

    private Player Player;
    private Rigidbody2D PlayerRb;

    private EnnemyPatrol Patrol;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        Patrol = GetComponentInParent<EnnemyPatrol>();
    }

    // Update is called once per frame
    void Update()
    {

        cooldownTimer += Time.deltaTime;

        bool inSight = IsPlayerInSight();

        if(inSight && Player != null && Player.IsAlive && PlayerRb.simulated)
        {
            if (cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0;
                anim.SetTrigger("Attack");
            }
        }

        if (Patrol != null)
        {
            Patrol.enabled = !inSight;
        }
    }

    private bool IsPlayerInSight()
    {
        Vector3 size = new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z);
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center + transform.right * range * colliderDistance, size, 0, Vector2.left, 0, playerLayer);
        if(Player == null && hit.collider != null)
        {
            //Grab the player reference the first time
            Player = hit.transform.GetComponent<Player>();
            PlayerRb = Player.transform.GetComponent<Rigidbody2D>();
        }
        return hit.collider != null;
    }

    private void DamagePlayer()
    {
        if (IsPlayerInSight())
        {
            Player.TakeDamage(damage);
        }
    }

    void OnDrawGizmos()
    {
        Vector3 size = new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * colliderDistance, size);
    }

}
