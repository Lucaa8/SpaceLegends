using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{

    [SerializeField] GameObject ShopObject;
    [SerializeField] Button ShopButton;
    [SerializeField] GameObject HelpObject;
    [SerializeField] Button HelpButton;

    [SerializeField] Menu _menu;

    [SerializeField] CaseSroll earthScroll;
    [SerializeField] CaseSroll marsScroll;

    [SerializeField] UserInfo userInfo; //Needed to get relics counts and probabilities

    private bool isHelpDisplay = false;
    public bool isShopAnimated = false;

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

        if ((show && _menu.openedWindow != -1) || (!show && _menu.openedWindow != 2))
        {
            yield break;
        }

        isShopAnimated = true;

        if (show) //Cannot do ShopObject.SetActive(show); because in the false case, the gameobject would be deactivated before the canvas opacity animation played.
        {
            _menu.openedWindow = 2;
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
            _menu.openedWindow = -1;
            ShopObject.SetActive(false);
        }

        isShopAnimated = false;
    }

    public void UpdateOpenRelicButton(int collec)
    {
        Button button = transform.Find("Shop/Scroll View/Viewport/Content/Relics/CREL/CREL" + UserInfo.Collections[collec] + "/Open").GetComponent<Button>();
        button.interactable = userInfo.CountRelic(collec) > 0;
    }

    private void OpenRelic(int collec, CaseSroll scroll)
    {
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("open-relic/"+collec.ToString()), UnityWebRequest.kHttpVerbPOST, null, Auth.AuthType.ACCESS, (isOk, status, jrep) =>
        {
            if (isOk)
            {
                JArray proba = jrep.Value<JArray>("probabilities");
                if (proba.Count > 0)
                {
                    userInfo.RemoveRelic(collec);
                    UpdateOpenRelicButton(collec);
                    CaseCell.chances = new System.Collections.Generic.Dictionary<CaseCell.Rarity, float>
                        {
                            {
                                CaseCell.Rarity.COMMON, proba[0].Value<float>()
                            },
                            {
                                CaseCell.Rarity.RARE, proba[1].Value<float>()
                            },
                            {
                                CaseCell.Rarity.EPIC, proba[2].Value<float>()
                            },
                            {
                                CaseCell.Rarity.LEGENDARY, proba[3].Value<float>()
                            }
                        };
                }
                scroll.nftResult = new CaseCell.NFT();
                scroll.nftResult.name = jrep.Value<string>("name");
                scroll.nftResult.rarity = (CaseCell.Rarity)(jrep.Value<int>("rarity") - 1);
                scroll.nftResult.sprite = Resources.Load<Sprite>(jrep.Value<string>("image"));
                scroll.Scroll();
                CaseCell.chances = null;
                long type = jrep.Value<long>("type");
                userInfo.SetNFTCount(type, userInfo.GetNFTCount(type) + 1);
            }
        }));
    }

    public void WantToOpenEarthRelic()
    {
        if (userInfo.NFTCountEarth > 0 && CaseSroll.canScroll)
        {
            OpenRelic(0, earthScroll);
        }
    }

    public void WantToOpenMarsRelic()
    {
        if (userInfo.NFTCountMars > 0 && CaseSroll.canScroll)
        {
            OpenRelic(1, marsScroll);
        }
    }

}
