using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{

    [SerializeField] Animator p;
    [SerializeField] Animator a1;
    [SerializeField] Animator a2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            p.SetTrigger("Attack1");
            a1.SetTrigger("Attack1");
            a2.SetTrigger("Attack1");
        }
    }
}
