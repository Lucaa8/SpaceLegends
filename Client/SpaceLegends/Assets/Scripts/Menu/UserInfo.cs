using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class UserInfo : MonoBehaviour
{

    public static Dictionary<int, string> Collections = new Dictionary<int, string> { { 0, "Earth" }, { 1, "Mars" } };

    public static (int, int, int, int) DecodeTokenType(long encodedType)
    {
        int collec = (int)((encodedType >> 27) & 0xFFF);
        int row = (int)((encodedType >> 11) & 0xFF);
        int col = (int)((encodedType >> 3) & 0xFF);
        int rarity = (int)(encodedType & 0x7);

        return (collec, row, col, rarity);
    }

    private string ToDate(long unix)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unix);
        DateTime dateTime = dateTimeOffset.UtcDateTime;
        return dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", System.Globalization.CultureInfo.InvariantCulture);
    }

    [SerializeField] private GameObject GameMenu;
    [SerializeField] private GameObject Profile;
    [SerializeField] private GameObject Shop;

    private string _username;
    private string _displayname;
    private float _sETH;
    private float _SDT;
    private int _stars = 0;
    private int _hearts;
    private int _totalDamage = 0;
    private int _perkDamage = 0;
    private long _perkDamageEnd = -1;
    private int _totalArmor = 0;
    private int _perkArmor = 0;
    private long _perkArmorEnd = -1;
    private int _totalSpeed = 0;
    private int _perkSpeed = 0;
    private long _perkSpeedEnd = -1;
    private UserLevel _level;
    // Misleading name as this attribute holds the number of levels COMPLETED (Total completions) and not total games ran
    private int[] _statsGames = new int[] {0, 0};
    private int[] _statsKills = new int[] { 0, 0 };
    private int[] _statsDeaths = new int[] { 0, 0 };
    private Dictionary<long, int> _nft = new Dictionary<long, int>();
    private int _nftCountMars;
    private int _nftCountEarth;
    private Dictionary<int, GameLevel> _levels = new Dictionary<int, GameLevel>();
    private Dictionary<int, int> _relics = new Dictionary<int, int>();

    public string Username
    {
        get { return _username; }
        set
        {
            _username = value;
            Profile.transform.Find("General/UserName").GetComponent<TMP_Text>().text = "@" + _username;
        }
    }

    public string Displayname
    {
        get { return _displayname; }
        set
        {
            _displayname = value;
            Profile.transform.Find("General/DisplayName").GetComponent<TMP_Text>().text = _displayname;
        }
    }

    public float SETH
    {
        get { return _sETH; }
        set
        {
            _sETH = value;
            Shop.transform.Find("SETHBg/TxtSETH").GetComponent<TMP_Text>().text = _sETH.ToString("F2") + " SETH";
        }
    }

    public float SDT
    {
        get { return _SDT; }
        set
        {
            _SDT = value;
            string sdt = _SDT.ToString("F2");
            Profile.transform.Find("Resources/List/SDTCurrencyDisplay/TextSection/SDT").GetComponent<TMP_Text>().text = sdt;
            Shop.transform.Find("Scroll View/Viewport/Content/Transactions/SDT/SDTCount/CurrencySDT/TextSection/Currency").GetComponent<TMP_Text>().text = sdt;
            Shop.transform.Find("Scroll View/Viewport/Content/Relics/SDTInfo").GetComponent<TMP_Text>().text = "Opening a Cosmic Relic costs 0.5 SDT. (You have: "+sdt+" SDT)";
        }
    }

    public int Stars
    {
        get { return _stars; }
        set
        {
            _stars = value;
            Profile.transform.Find("Resources/List/StarsCurrencyDisplay/TextSection/Stars").GetComponent<TMP_Text>().text = _stars.ToString();
            GameMenu.transform.Find("Image/StarsCurrencyDisplay/TextSection/Stars").GetComponent<TMP_Text>().text = _stars.ToString();
        }
    }

    public int Hearts
    {
        get { return _hearts; }
        set
        {
            _hearts = value;
            Profile.transform.Find("Resources/List/HeartCurrencyDisplay/TextSection/Hearts").GetComponent<TMP_Text>().text = _hearts.ToString();
            Shop.transform.Find("Scroll View/Viewport/Content/Transactions/Lives/HeartsCount/CurrencyHearts/TextSection/Currency").GetComponent<TMP_Text>().text = _hearts.ToString();
        }
    }

    public UserLevel Level
    {
        get { return _level; }
        set
        {
            _level = value;
            string baseuri = "GameStatistics/List/AccountLevel/LevelBar/";
            Profile.transform.Find(baseuri + "Image/TxtLevel").GetComponent<TMP_Text>().text = _level.Level.ToString();
            Profile.transform.Find(baseuri + "TxtLevelExp").GetComponent<TMP_Text>().text = _level.Current + "/" + _level.Max + " exp";
            float ratio = _level.getRatio();
            Profile.transform.Find(baseuri + "Slider").GetComponent<Slider>().value = ratio;
            Profile.transform.Find(baseuri + "TxtLvlPercent").GetComponent<TMP_Text>().text = (ratio*100).ToString("F1") + "%";
        }
    }

    public int TotalDamage
    {
        get { return _totalDamage; }
        set 
        {
            _totalDamage = value;
            Profile.transform.Find("CombatStatistics/List/Damage/CombatDamage").GetComponent<TMP_Text>().text = "+" + _totalDamage.ToString() + "%";
        }
    }

    public int PerkDamage
    {
        get { return _perkDamage; }
        set
        {
            _perkDamage = value;
            Shop.transform.Find("Scroll View/Viewport/Content/Transactions/Combat/ActiveStats/List/Damage/CombatDamage").GetComponent<TMP_Text>().text = "+" + _perkDamage.ToString() + "%";
        }
    }

    public long PerkDamageEnd
    {
        get { return _perkDamageEnd; }
        set
        {
            _perkDamageEnd = value;
            string baseuri = "Scroll View/Viewport/Content/Transactions/Combat/Offer1/";
            Shop.transform.Find(baseuri + "Buy").gameObject.SetActive(_perkDamageEnd <= 0);
            Shop.transform.Find(baseuri + "Bought").gameObject.SetActive(_perkDamageEnd > 0);
            if(_perkDamageEnd > 0)
            {
                Shop.transform.Find(baseuri + "Bought/GameObject/RemainingTime").GetComponent<TMP_Text>().text = ToDate(_perkDamageEnd);
            }
            else
            {
                TotalDamage -= PerkDamage;
                PerkDamage = 0;
            }
        }
    }

    public int TotalArmor
    {
        get { return _totalArmor; }
        set
        {
            _totalArmor = value;
            Profile.transform.Find("CombatStatistics/List/Armor/CombatArmor").GetComponent<TMP_Text>().text = "+" + _totalArmor.ToString() + "%";
        }
    }

    public int PerkArmor
    {
        get { return _perkArmor; }
        set
        {
            _perkArmor = value;
            Shop.transform.Find("Scroll View/Viewport/Content/Transactions/Combat/ActiveStats/List/Armor/CombatArmor").GetComponent<TMP_Text>().text = "+" + _perkArmor.ToString() + "%";
        }
    }

    public long PerkArmorEnd
    {
        get { return _perkArmorEnd; }
        set
        {
            _perkArmorEnd = value;
            string baseuri = "Scroll View/Viewport/Content/Transactions/Combat/Offer2/";
            Shop.transform.Find(baseuri + "Buy").gameObject.SetActive(_perkArmorEnd <= 0);
            Shop.transform.Find(baseuri + "Bought").gameObject.SetActive(_perkArmorEnd > 0);
            if (_perkArmorEnd > 0)
            {
                Shop.transform.Find(baseuri + "Bought/GameObject/RemainingTime").GetComponent<TMP_Text>().text = ToDate(_perkArmorEnd);
            }
            else
            {
                TotalArmor -= PerkArmor;
                PerkArmor = 0;
            }
        }
    }

    public int TotalSpeed
    {
        get { return _totalSpeed; }
        set
        {
            _totalSpeed = value;
            Profile.transform.Find("CombatStatistics/List/Speed/CombatSpeed").GetComponent<TMP_Text>().text = "+" + _totalSpeed.ToString() + "%";
        }
    }

    public int PerkSpeed
    {
        get { return _perkSpeed; }
        set
        {
            _perkSpeed = value;
            Shop.transform.Find("Scroll View/Viewport/Content/Transactions/Combat/ActiveStats/List/Speed/CombatSpeed").GetComponent<TMP_Text>().text = "+" + _perkSpeed.ToString() + "%";
        }
    }

    public long PerkSpeedEnd
    {
        get { return _perkSpeedEnd; }
        set
        {
            _perkSpeedEnd = value;
            string baseuri = "Scroll View/Viewport/Content/Transactions/Combat/Offer3/";
            Shop.transform.Find(baseuri + "Buy").gameObject.SetActive(_perkSpeedEnd <= 0);
            Shop.transform.Find(baseuri + "Bought").gameObject.SetActive(_perkSpeedEnd > 0);
            if (_perkSpeedEnd > 0)
            {
                Shop.transform.Find(baseuri + "Bought/GameObject/RemainingTime").GetComponent<TMP_Text>().text = ToDate(_perkSpeedEnd);
            }
            else
            {
                TotalSpeed -= PerkSpeed;
                PerkSpeed = 0;
            }
        }
    }

    public int[] Games
    {
        get { return _statsGames; }
        set
        {
            _statsGames = value;
            Profile.transform.Find("GameStatistics/List/Game/Completed/TxtGames").GetComponent<TMP_Text>().text = _statsGames[0].ToString() + " (Top " + _statsGames[1] + ")";
        }
    }

    public int[] Kills
    { 
        get { return _statsKills; }
        set
        {
            _statsKills = value;
            Profile.transform.Find("GameStatistics/List/Game/Kills/TxtKills").GetComponent<TMP_Text>().text = _statsKills[0].ToString() + " (Top " + _statsKills[1] + ")";
        }
    }

    public int[] Deaths
    {
        get { return _statsDeaths; }
        set
        {
            _statsDeaths = value;
            Profile.transform.Find("GameStatistics/List/Game/Deaths/TxtDeaths").GetComponent<TMP_Text>().text = _statsDeaths[0].ToString() + " (Top " + _statsDeaths[1] + ")";
        }
    }

    public int NFTCountMars
    {
        get { return _nftCountMars; }
        set
        {
            _nftCountMars = value;
            FindInActiveObjectByName("TxtMarsRelic").GetComponent<TMP_Text>().text = "Mars (" + _nftCountMars + "/9)";
        }
    }

    public int NFTCountEarth
    {
        get { return _nftCountEarth; }
        set
        {
            _nftCountEarth = value;
            FindInActiveObjectByName("TxtEarthRelic").GetComponent<TMP_Text>().text = "Earth (" + _nftCountEarth + "/9)";
        }
    }

    public int GetNFTCount(long type)
    {
        return _nft.GetValueOrDefault(type, 0);
    }

    public void SetNFTCount(long type, int count)
    {
        _nft[type] = count;
        FindInActiveObjectByName("Profile"+type.ToString()).GetComponent<TMP_Text>().text = "Count: " + count.ToString();
        if (count > 0)
        {
            var decoded = DecodeTokenType(type);
            GameMenu.transform.Find("Image/" + Collections[decoded.Item1] + "/Reward/Image/R" + decoded.Item2 + "C" + decoded.Item3).GetComponent<Image>().enabled = true;
        }
    }

    public void AddGameLevel(GameLevel level)
    {
        _levels[level.LevelID] = level;
        Transform levelObj = GameMenu.transform.Find("Image/" + level.CollectionName + "/Levels/Level (" + level.Level.ToString() + ")");
        bool unlocked = Stars >= level.UnlockRequirements;
        levelObj.Find("BgLocked").gameObject.SetActive(!unlocked);
        levelObj.Find("Locked").gameObject.SetActive(!unlocked);
        levelObj.Find("Locked/Image/UnlockRequirement").GetComponent<TMP_Text>().text = level.UnlockRequirements.ToString();
        GameObject starsObj = levelObj.Find("Stars").gameObject;
        for (int i=0;i<=2;i++)
        {
            starsObj.transform.Find("Star" + i.ToString()).Find("On").gameObject.SetActive(level.Stars[i]);
        }
        if (!unlocked)
            return;
        levelObj.Find("BgUnlocked" + level.CollectionName).GetComponent<Button>().onClick.AddListener(() =>
        {
            FindObjectOfType<LevelChanger>().FadeToLevel(level.CollectionName + "_" + level.LevelID);
            // /100 because in this script, modifiers are at 100% scale like 18% speed bonus. But in the below scripts, it needs to be 0.18, etc..
            PlayerController.SpeedModifier = this.TotalSpeed * 1.0f / 100f;
            PlayerAttack.DamageModifier = this.TotalDamage * 1.0f / 100f;
            Player.ArmorModifier = this.TotalArmor * 1.0f / 100f;
        });
    }

    public void SetRelicsCount(int collec, int count)
    {
        _relics[collec] = count;
        Shop.transform.Find("Scroll View/Viewport/Content/Relics/CREL/CREL" + Collections[collec] + "/Value").GetComponent<TMP_Text>().text = "You have " + count.ToString() + " relics to open";
    }

    public void AddRelic(int collec)
    {
        SetRelicsCount(collec, CountRelic(collec) + 1);
    }

    public void RemoveRelic(int collec)
    {
        SetRelicsCount(collec, CountRelic(collec) - 1);
    }

    public int CountRelic(int collec)
    {
        return _relics.GetValueOrDefault(collec, 0);
    }

    public void UpdateTotalNFTCount(int collec)
    {
        ISet<long> set = new HashSet<long>();
        foreach(long item in _nft.Keys)
        {
            var decoded = DecodeTokenType(item);
            if(decoded.Item1 == collec)
            {
                set.Add(item);
            }
        }
        if(collec == 0)
        {
            if(set.Count == 9 && NFTCountEarth != 9)
            {
                TotalDamage += 18;
            }
            NFTCountEarth = set.Count;
        }
        else if (collec == 1)
        {
            if (set.Count == 9 && NFTCountMars != 9)
            {
                TotalArmor += 21;
            }
            NFTCountMars = set.Count;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        Auth.OnResponse UpdateLeaderboard = (ok, status, j) =>
        {
            if(ok)
            {
                Kills = new int[] { Kills[0], j.Value<JArray>("kills")[0].Value<int>() };
                Deaths = new int[] { Deaths[0], j.Value<JArray>("deaths")[0].Value<int>() };
                Games = new int[] { Games[0], j.Value<JArray>("completed")[0].Value<int>() };
            }
        };

        Auth.OnResponse Update = (ok, status, j) =>
        {
            if(ok)
            {
                Username = j.Value<string>("username");
                Displayname = j.Value<string>("display_name");
                Level = new UserLevel(j.Value<JArray>("experience"));
                JObject resources = j.Value<JObject>("resources");
                SETH = resources.Value<float>("eth");
                SDT = resources.Value<float>("sdt");
                Hearts = resources.Value<int>("heart");              
                NFTCountEarth = j.Value<JObject>("completed_collections").Value<int>("0");
                if(NFTCountEarth == 9)
                {
                    TotalDamage += 18;
                }
                NFTCountMars = j.Value<JObject>("completed_collections").Value<int>("1");
                if (NFTCountMars == 9)
                {
                    TotalArmor += 21;
                }
                foreach (var property in j.Value<JObject>("nft").Properties())
                {
                    long.TryParse(property.Name, out long type);
                    int value = (int)property.Value;
                    SetNFTCount(type, value);
                }
                foreach(var property in resources.Value<JArray>("perks"))
                {
                    JObject perk = (JObject)property;
                    string type = perk.Value<string>("type");
                    int value = perk.Value<int>("value");
                    long end_time = perk.Value<long>("end_time");
                    if (type == "damage")
                    {
                        PerkDamage = value;
                        TotalDamage += value;
                        PerkDamageEnd = end_time;
                    }
                    if (type == "armor")
                    {
                        PerkArmor = value;
                        TotalArmor += value;
                        PerkArmorEnd = end_time;
                    }
                    if (type == "speed")
                    {
                        PerkSpeed = value;
                        TotalSpeed += value;
                        PerkSpeedEnd = end_time;
                    }
                }
                JObject jLevels = j.Value<JObject>("levels");
                GameLevel[] tmpLevels = new GameLevel[jLevels.Count];
                int i = 0;
                foreach (var property in j.Value<JObject>("levels").Properties())
                {
                    GameLevel level = new GameLevel((JObject)property.Value);
                    Stars += level.StarsCount; //Need to count every star before adding game levels to UI because of the Lock requirement images.
                    tmpLevels[i++] = level;
                }
                foreach(GameLevel level in tmpLevels)
                {
                    AddGameLevel(level);
                    Kills = new int[] { Kills[0] + level.Kills, 0 };
                    Deaths = new int[] { Deaths[0] + level.Deaths, 0 };
                    Games = new int[] { Games[0] + level.Completions, 0 };
                }
                foreach (var property in j.Value<JObject>("relics").Properties())
                {
                    int.TryParse(property.Name, out int collec);
                    SetRelicsCount(collec, (int)property.Value);
                    transform.GetComponent<Shop>().UpdateOpenRelicButton(collec);
                }
                StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("leaderboard")+"/"+_username, UnityWebRequest.kHttpVerbGET, null, Auth.AuthType.ACCESS, UpdateLeaderboard));
            }
        };

        if(Auth.Instance != null)
        {
            StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("user"), UnityWebRequest.kHttpVerbGET, null, Auth.AuthType.ACCESS, Update));
        }

    }

    // Update is called once per frame
    void Update()
    {
        long ms = GetCurrentUnixTimestamp();

        if (PerkDamageEnd != -1)
        {
            if(ms > PerkDamageEnd)
            {
                PerkDamageEnd = -1;
            }
        }

        if (PerkArmorEnd != -1)
        {
            if (ms > PerkArmorEnd)
            {
                PerkArmorEnd = -1;
            }
        }

        if (PerkSpeedEnd != -1)
        {
            if (ms > PerkSpeedEnd)
            {
                PerkSpeedEnd = -1;
            }
        }

    }

    private long GetCurrentUnixTimestamp()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
    }

    GameObject FindInActiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }
}
