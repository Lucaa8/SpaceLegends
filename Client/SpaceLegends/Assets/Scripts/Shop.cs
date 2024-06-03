using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{

    [SerializeField] GameObject ShopObject;
    [SerializeField] Button ShopButton;
    [SerializeField] GameObject HelpObject;
    [SerializeField] Button HelpButton;


    private bool isHelpDisplay = false;
    private bool isShopAnimated = false;

    // Start is called before the first frame update
    void Start()
    {
        ShopButton.onClick.AddListener(() =>
        {
            if (!isShopAnimated)
            {
                if(isHelpDisplay)
                {
                    Help();
                }
                StartCoroutine(ShowShop(!ShopObject.activeInHierarchy));
            }
        });
        HelpButton.onClick.AddListener(Help);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Help()
    {
        isHelpDisplay = !isHelpDisplay;
        HelpObject.SetActive(isHelpDisplay);
    }

    public void CloseShop()
    {
        if (isHelpDisplay)
        {
            Help();
        }
        StartCoroutine(ShowShop(false)); //For the cross inside the shop window
    }

    public IEnumerator ShowShop(bool show)
    {
        isShopAnimated = true;

        if (show) //Cannot do ShopObject.SetActive(show); because in the false case, the gameobject would be deactivated before the canvas opacity animation played.
        {
            ShopObject.SetActive(true);
        }

        float duration = 0.3f; // seconds
        float elapsedTime = 0f;
        CanvasGroup group = ShopObject.GetComponent<CanvasGroup>();
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
            ShopObject.SetActive(false);
        }

        isShopAnimated = false;
    }

}
