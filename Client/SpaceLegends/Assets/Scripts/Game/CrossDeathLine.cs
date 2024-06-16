using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossDeathLine : MonoBehaviour
{

    [SerializeField] GameObject player;
    [SerializeField] GameObject check;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger Enter");
        if (collision.gameObject.CompareTag("Player"))
        {
            player.transform.position = check.transform.position;
        }
    }

}
