using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{

    [SerializeField] GameObject ShopObject;
    [SerializeField] Button ShopButton;
    [SerializeField] GameObject HelpObject;
    [SerializeField] Button HelpButton;

    [SerializeField] CaseSroll earthScroll;
    [SerializeField] CaseSroll marsScroll;

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

    public void WantToOpenEarthRelic()
    {
        int count = 3;
        if (count > 0 && CaseSroll.canScroll)
        {
            earthScroll.nftResult = new CaseCell.NFT();
            earthScroll.nftResult.name = "Earth 3 (Row 1 | Col 3)";
            earthScroll.nftResult.rarity = CaseCell.Rarity.EPIC;
            earthScroll.nftResult.sprite = Resources.Load<Sprite>("00_Earth_r01c03");
            earthScroll.Scroll();
        }
    }

    public void WantToOpenMarsRelic()
    {
        int count = 1;
        if (count > 0 && CaseSroll.canScroll)
        {
            marsScroll.nftResult = new CaseCell.NFT();
            marsScroll.nftResult.name = "Mars 8 (Row 3 | Col 2)";
            marsScroll.nftResult.rarity = CaseCell.Rarity.RARE;
            marsScroll.nftResult.sprite = Resources.Load<Sprite>("01_Mars_r03c02");
            marsScroll.Scroll();
        }
    }

}
