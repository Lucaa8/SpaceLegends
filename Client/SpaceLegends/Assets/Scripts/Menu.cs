using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    [SerializeField] Button BtnLogout;
    [SerializeField] TMP_Text TxtUser;

    // Start is called before the first frame update
    void Start()
    {
        BtnLogout.onClick.AddListener(() => StartCoroutine(Auth.Instance.Logout()));
        TxtUser.text = "Welcome back " + Auth.Instance.GetDisplayname();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
