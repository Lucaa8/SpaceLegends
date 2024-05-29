using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeInput : MonoBehaviour
{

    EventSystem System;
    [SerializeField] Selectable firstInput;

    // Start is called before the first frame update
    void Start()
    {
        System = EventSystem.current;
        firstInput.Select();
    }

    private bool tabPressed = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            tabPressed = true;
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            tabPressed = false;
        }
        if (tabPressed)
        {
            Selectable next = System.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            Debug.Log(next.name);
            if (next != null)
            {
                next.Select();
            }
            tabPressed = false;
        } 
    }
}
