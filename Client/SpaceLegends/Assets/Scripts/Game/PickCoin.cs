using UnityEngine;

public class PickCoin : MonoBehaviour
{

    private Animator anim;

    public float Value = 0.03f;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void OnAnimStart()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void OnAnimMid()
    {
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    public void Pick()
    {
        transform.GetComponent<Collider2D>().enabled = false;
        anim.SetTrigger("Pick");
    }

    public void Delete()
    {
        gameObject.SetActive(false);
    }

}
