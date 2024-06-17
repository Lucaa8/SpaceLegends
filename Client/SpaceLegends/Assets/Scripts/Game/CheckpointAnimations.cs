using UnityEngine;

public class CheckpointAnimations : MonoBehaviour
{

    private Animator animator;

    void Start()
    {
        animator = transform.GetComponent<Animator>();
    }

    public void Touch()
    {
        animator.SetTrigger("TouchCheckpoint");
    }

    public void Idle()
    {
        animator.SetTrigger("AnimationEnded");
    }

}
