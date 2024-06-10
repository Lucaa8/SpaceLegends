using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{

    [SerializeField] GameObject ProfileObject;
    [SerializeField] Button ProfileButton;

    [SerializeField] Menu _menu;

    public bool isProfileAnimated = false;

    // Start is called before the first frame update
    void Start()
    {
        ProfileButton.onClick.AddListener(() =>
        {
            if (!isProfileAnimated)
            {
                StartCoroutine(ShowProfile(!ProfileObject.activeInHierarchy));
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EditProfile()
    {
        Application.OpenURL(Auth.BaseUrl+"/profile");
    }

    public void CloseProfile()
    {
        StartCoroutine(ShowProfile(false)); //For the cross inside the settings window
    }

    public IEnumerator ShowProfile(bool show)
    {

        if ((show && _menu.openedWindow != -1) || (!show && _menu.openedWindow != 1))
        {
            yield break;
        }

        isProfileAnimated = true;

        if (show) //Cannot do ProfileObject.SetActive(show); because in the false case, the gameobject would be deactivated before the canvas opacity animation played.
        {
            ProfileObject.SetActive(true);
            _menu.openedWindow = 1;
        }

        float duration = 0.3f; // seconds
        float elapsedTime = 0f;
        CanvasGroup group = ProfileObject.GetComponent<CanvasGroup>();
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / duration;
            group.alpha = show ? alpha : 1f - alpha;
            yield return null;
        }
        group.alpha = show ? 1f : 0f;

        if (!show)
        {
            ProfileObject.SetActive(false);
            _menu.openedWindow = -1;
        }

        isProfileAnimated = false;
    }

}
