using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    //ATTACK (SMALL) WITH MOUSE LEFT OR J
    //ATTACK (HEAVY) WITH MOUSE RIGHT OR L

    [SerializeField] float Attack1Cooldown;
    [SerializeField] float Attack2Cooldown;
    [SerializeField] float Damage;
    public static float DamageModifier = 0f;
    [SerializeField] Transform AttackArea;
    [SerializeField] float RangeX;
    [SerializeField] float RangeY;
    [SerializeField] LayerMask EnnemiesMask;

    private Player player;
    private Rigidbody2D rb;
    private Animator anim;

    private float nextAttackIn = 0;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!player.IsAlive || !rb.simulated)
        {
            return; //Avoid attacking while the game is paused or while player is dead
        }

        if (nextAttackIn > 0f)
        {
            nextAttackIn -= Time.deltaTime;
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.J))
        {
            anim.SetTrigger("Attack1");
            nextAttackIn = Attack1Cooldown;
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.L))
        {
            anim.SetTrigger("Attack2");
            nextAttackIn = Attack2Cooldown;
        }
    }

    public void Attack1()
    {
        Attack(Damage);
    }

    public void Attack2()
    {
        // This attack has bigger cooldown but deals more base damage
        Attack(Damage * 1.25f);
    }

    private void Attack(float damage)
    {
        damage += damage * DamageModifier;
        bool hit = false;
        foreach (Collider2D ennemy in Physics2D.OverlapBoxAll(AttackArea.position, new Vector2(RangeX, RangeY), 0, EnnemiesMask))
        {
            Ennemy e = ennemy.transform.GetComponent<Ennemy>();
            if(e.isAlive)
            {
                e.TakeDamage(damage);
                hit = true;
            }         
        }
        if(hit)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.sfxPlayerHit);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(AttackArea.position, new Vector3(RangeX, RangeY, 1));
    }

}
