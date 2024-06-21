using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    [SerializeField] float damage;
    [SerializeField] Transform attackArea;
    [SerializeField] float RangeX;
    [SerializeField] float RangeY;
    [SerializeField] LayerMask EnnemiesMask;

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Currently the trigger is queued and plays after the current animation ended. not good
            anim.SetTrigger("Attack1");
        }
    }

    private void Attack()
    {
        foreach (Collider2D ennemy in Physics2D.OverlapBoxAll(attackArea.position, new Vector2(RangeX, RangeY), 0, EnnemiesMask))
        {
            Ennemy e = ennemy.transform.GetComponent<Ennemy>();
            if(e.isAlive)
            {
                e.TakeDamage(damage);
            }         
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(attackArea.position, new Vector3(RangeX, RangeY, 1));
    }

}
