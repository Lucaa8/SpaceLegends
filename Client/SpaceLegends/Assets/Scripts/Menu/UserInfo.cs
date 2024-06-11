using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private int _stars;
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
    private int[] _statsGames;
    private int[] _statsKills;
    private int[] _statsDeaths;
    private Dictionary<long, int> _nft = new Dictionary<long, int>();
    private int _nftCountMars;
    private int _nftCountEarth;
    private Dictionary<int, GameLevel> _levels = new Dictionary<int, GameLevel>();

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
            Profile.transform.Find("Resources/List/SDTCurrencyDisplay/TextSection/SDT").GetComponent<TMP_Text>().text = _SDT.ToString("F2");
            Shop.transform.Find("Scroll View/Viewport/Content/Transactions/SDT/SDTCount/CurrencySDT/TextSection/Currency").GetComponent<TMP_Text>().text = _SDT.ToString("F2");
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
        return _nft[type];
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
        for(int i=0;i<=3;i++)
        {
            GameObject starObj = levelObj.Find("Stars"+i.ToString()).gameObject;
            starObj.SetActive(i == level.Stars);
        }
    }


    // Start is called before the first frame update
    void Start()
    {

        Username = "lucaa_8";
        Displayname = "luluuu";
        SETH = 18.892891F;
        SDT = 1421.238291F;
        Stars = 14;
        Hearts = 332;
        TotalDamage = 13;
        PerkDamage = 9;
        PerkDamageEnd = 1718128200L;
        TotalArmor = 14;
        PerkArmor = 8;
        PerkArmorEnd = 1718128215L;
        TotalSpeed = 12;
        PerkSpeed = 82;
        PerkSpeedEnd = 1718128230L;
        Level = new UserLevel(new int[] { 35, 28765, 126000 });
        Games = new int[] { 872, 4 };
        Kills = new int[] { 1372, 1 };
        Deaths = new int[] { 262, 400 };
        NFTCountEarth = 1;
        NFTCountMars = 2;
        SetNFTCount(4106, 2);
        SetNFTCount(134219801, 4);
        SetNFTCount(134221833, 5);
        SetNFTCount(134219801, 5);

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
