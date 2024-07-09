using Newtonsoft.Json.Linq;
using System.Collections;
using TMPro;
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

        if(Auth.Instance == null)
        {
            return;
        }

        // Add click event on combat perk to buy them
        for(int i = 1; i <= 3; i++)
        {
            foreach (Button buy in transform.Find("Shop/Scroll View/Viewport/Content/Transactions/Combat/Offer"+i+"/Buy").GetComponentsInChildren<Button>())
            {
                int id = i;
                buy.onClick.AddListener(() =>
                {
                    int price = 0;
                    int.TryParse(buy.GetComponentInChildren<TMP_Text>().text.Split(" ")[0], out price);
                    // This one is also checked server-side, but this avoid the server being spammed if the player spams the button without enough money
                    if (userInfo.SDT < price)
                    {
                        return;
                    }
                    Auth.OnResponse r = (ok, status, perk) =>
                    {
                        if(ok)
                        {
                            userInfo.SDT -= price;
                            string type = perk.Value<string>("type");
                            int value = perk.Value<int>("value");
                            long end_time = perk.Value<long>("end_time");
                            if (type == "damage")
                            {
                                userInfo.PerkDamage = value;
                                userInfo.TotalDamage += value;
                                userInfo.PerkDamageEnd = end_time;
                            }
                            if (type == "armor")
                            {
                                userInfo.PerkArmor = value;
                                userInfo.TotalArmor += value;
                                userInfo.PerkArmorEnd = end_time;
                            }
                            if (type == "speed")
                            {
                                userInfo.PerkSpeed = value;
                                userInfo.TotalSpeed += value;
                                userInfo.PerkSpeedEnd = end_time;
                            }
                        }
                    };
                    JObject body = new JObject { { "id",  id }, { "duration", buy.transform.parent.name } };
                    StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("buy-perk"), "POST", body, Auth.AuthType.ACCESS, r));
                });
            }
        }

        // Add click event on shop lives offers to buy them
        for (int i = 1; i <= 4; i++)
        {
            foreach (Button buy in transform.Find("Shop/Scroll View/Viewport/Content/Transactions/Lives/Offer" + i + "/Buy").GetComponentsInChildren<Button>())
            {
                // +4 because id 1 -> 4 are for ETH to SDT offers in the server's db. So lives are 5 -> 8
                int id = i+4;
                buy.onClick.AddListener(() =>
                {
                    int price = 0;
                    int.TryParse(buy.GetComponentInChildren<TMP_Text>().text.Split(" ")[0], out price);
                    // This one is also checked server-side, but this avoid the server being spammed if the player spams the button without enough money
                    if (userInfo.SDT < price)
                    {
                        return;
                    }
                    Auth.OnResponse r = (ok, status, lives) =>
                    {
                        if (ok)
                        {
                            userInfo.SDT -= price;
                            userInfo.Hearts = lives.Value<int>("lives");
                        }
                    };
                    StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("buy-lives")+"/"+id.ToString(), "POST", null, Auth.AuthType.ACCESS, r));
                });
            }
        }

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
                userInfo.RemoveRelic(collec);
                UpdateOpenRelicButton(collec);
                scroll.nftResult = new CaseCell.NFT();
                scroll.nftResult.name = jrep.Value<string>("name");
                scroll.nftResult.rarity = (CaseCell.Rarity)(jrep.Value<int>("rarity") - 1);
                scroll.nftResult.sprite = Resources.Load<Sprite>(jrep.Value<string>("image"));
                scroll.Scroll();
                CaseCell.chances = null;
                long type = jrep.Value<long>("type");
                userInfo.SetNFTCount(type, userInfo.GetNFTCount(type) + 1);
                userInfo.UpdateTotalNFTCount(collec);
            }
        }));
    }

    public void WantToOpenEarthRelic()
    {
        if (userInfo.CountRelic(0) > 0 && CaseSroll.canScroll)
        {
            OpenRelic(0, earthScroll);
        }
    }

    public void WantToOpenMarsRelic()
    {
        if (userInfo.CountRelic(1) > 0 && CaseSroll.canScroll)
        {
            OpenRelic(1, marsScroll);
        }
    }

}
