using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyPatrol : MonoBehaviour
{

    [SerializeField] Transform Left;
    [SerializeField] Transform Right;

    [SerializeField] Transform Ennemy;

    private Animator anim;

    [SerializeField] float Speed;

    private bool isFacingRight = true;
    private bool movingLeft = false;

    [SerializeField] float idleTime;
    private float idleTimer;

    // Start is called before the first frame update
    void Start()
    {
        anim = Ennemy.GetComponent<Animator>();
    }

    void OnDisable()
    {
        anim.SetBool("Moving", false);
    }

    // Update is called once per frame
    void Update()
    {
        if (movingLeft)
        {
            if(Ennemy.position.x >= Left.position.x)
            {
                MoveInDirection(-1);
            }
            else
                DirectionChange();
            
        }
        else
        {
            if (Ennemy.position.x <= Right.position.x)
            {
                MoveInDirection(1);
            }
            else
                DirectionChange();
                
        }
        
    }

    private void DirectionChange()
    {
        anim.SetBool("Moving", false);

        idleTimer += Time.deltaTime;

        if (idleTimer > idleTime)
        {
            movingLeft = !movingLeft;
        }
    }

    private void MoveInDirection(int _direction)
    {
        anim.SetBool("Moving", true);
        idleTimer = 0;
        if (_direction > 0 && !isFacingRight)
        {
            isFacingRight = true;
            Ennemy.rotation = Quaternion.Euler(new Vector3(Ennemy.rotation.x, 0f, Ennemy.rotation.z));
        }
        else if (_direction < 0 && isFacingRight)
        {
            isFacingRight = false;
            Ennemy.rotation = Quaternion.Euler(new Vector3(Ennemy.rotation.x, 180f, Ennemy.rotation.z));
        }

        Ennemy.position = new Vector3(Ennemy.position.x + Time.deltaTime * _direction * Speed, Ennemy.position.y, Ennemy.position.z);
    }

}
