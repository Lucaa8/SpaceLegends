using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeInput : MonoBehaviour
{

    EventSystem System;
    [SerializeField] Selectable firstInput;

    private void OnEnable() //Called each time a gameobject associated with this script is setActive(true)
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
            if(System.currentSelectedGameObject == null)
            {
                firstInput.Select();
            }
            else
            {
                Selectable next = System.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                if (next != null)
                {
                    next.Select();
                }
            }
            tabPressed = false;
        } 
    }
}
