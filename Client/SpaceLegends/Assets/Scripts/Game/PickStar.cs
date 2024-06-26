using UnityEngine;

public class PickStar : MonoBehaviour
{

    public int StarNumber;
    public bool Enabled = true;
    public bool Saved = false;

    private Animator anim;
    private Collider2D starCollider;

    private void Start()
    {
        anim = GetComponent<Animator>();
        starCollider = GetComponent<Collider2D>();
    }

    public void PickAnimationEnd()
    {
        transform.gameObject.SetActive(false);
    }

    public void Pick()
    {
        starCollider.enabled = false;
        anim.SetTrigger("Pick");
    }

    public void ResetStar()
    {
        transform.gameObject.SetActive(true);
        starCollider.enabled = true;
    }

}
