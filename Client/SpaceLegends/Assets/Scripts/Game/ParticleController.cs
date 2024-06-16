using UnityEngine;

public class ParticleController : MonoBehaviour
{

    [SerializeField] ParticleSystem movementParticle;
    [SerializeField] ParticleSystem fallParticle;

    [Range(0, 10)]
    [SerializeField] int occurAfterVelocity = 3;

    [Range(0f, .2f)]
    [SerializeField] float dustFormationPeriod = 0.035f;

    private Rigidbody2D playerRb;
    private PlayerController playerController;

    private float counter;

    private void Start()
    {
        playerRb = transform.parent.transform.GetComponent<Rigidbody2D>();
        playerController = transform.parent.transform.GetComponent<PlayerController>();
    }

    void Update()
    {
        counter += Time.deltaTime;

        if (playerController.IsOnGround && Mathf.Abs(playerRb.velocity.x) > occurAfterVelocity)
        {
            if (counter > dustFormationPeriod)
            {
                movementParticle.Play();
                counter = 0;
            }
        }
    }

    public void PlayFall()
    {
        fallParticle.Play();
    }

}
