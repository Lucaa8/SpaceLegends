using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CaseCell;

public class CaseSroll : MonoBehaviour
{

    [SerializeField] GameObject Prefab;

    [SerializeField] int Max;

    [SerializeField] float speed;
    private float currentSpeed;
    private Vector3 initialPosition;
    private bool isScrolling;
    public static bool canScroll { get; private set; } = true;

    private List<CaseCell> cells = new List<CaseCell>();

    private float widthImg;
    private float widthScroll;
    private int spacing;

    private int DropLocation;
    private float ToDropLocation;

    public NFT nftResult;
    [SerializeField] GameObject InterrogationPointsFake;
    [SerializeField] Image WinWindow;
    [SerializeField] TMP_Text WinTitle;
    [SerializeField] TMP_Text WinRarity;

    public static Dictionary<Rarity, Color> colors = new Dictionary<Rarity, Color>
    {
        { Rarity.COMMON,    new Color(0f / 255f, 128f / 255f, 0f / 255f, 1f) },
        { Rarity.RARE,      new Color(16f / 255f, 35f / 255f, 174f / 255f, 1f) },
        { Rarity.EPIC,      new Color(133f / 255f, 8f / 255f, 255f / 255f, 1f) },
        { Rarity.LEGENDARY, new Color(255f / 255f, 255f / 255f, 0f / 255f, 1f) }
    };

    public void Scroll()
    {
        if (isScrolling || !canScroll)
        {
            return;
        }

        canScroll = false;

        InterrogationPointsFake.SetActive(false);

        if (cells.Count != 0)
        {
            cells.ForEach(c=>Destroy(c.transform.parent.gameObject));
            cells.Clear();
        }

        currentSpeed = speed;
        isScrolling = true;
        transform.localPosition = new Vector3 (initialPosition.x, initialPosition.y); //reset x position

        for(int i = 0; i < Max; i++)
        {
            CaseCell cell = Instantiate(Prefab, transform).GetComponentInChildren<CaseCell>();
            cells.Add(cell);
            if(i == DropLocation)
            {
                cell.Setup(nftResult.sprite, colors[nftResult.rarity]);
                continue;
            }
            cell.Setup(null, Color.black /* unused */);
        }

        widthImg = GetComponentInChildren<CaseCell>().GetComponent<RectTransform>().rect.width;
        widthScroll = GetComponent<RectTransform>().rect.width;
        spacing = (int)GetComponent<HorizontalLayoutGroup>().spacing;

        ToDropLocation = -1 * ((DropLocation * widthImg) + (DropLocation * spacing) + Random.Range(-widthImg/2, widthImg/2));

    }


    public void CloseWin()
    {
        WinWindow.transform.parent.gameObject.SetActive(false);
        InterrogationPointsFake.SetActive(true);
        cells.ForEach(c => Destroy(c.transform.parent.gameObject));
        cells.Clear();
        canScroll = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = new Vector3(transform.localPosition.x, transform.localPosition.y);
        DropLocation = Max - 10;
    }


    // Update is called once per frame
    void Update()
    {

        if(!isScrolling)
        {
            return;
        }

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition + Vector3.left * 100, currentSpeed * Time.deltaTime * 700);

        if (transform.localPosition.x <= ToDropLocation)
        {
            isScrolling = false;
            WinWindow.sprite = nftResult.sprite;
            WinTitle.text = nftResult.name;
            WinRarity.text = nftResult.rarity.ToString();
            WinRarity.color = colors[nftResult.rarity];
            WinWindow.transform.parent.gameObject.SetActive(true);
            return;
        }

        float distanceToDrop = Mathf.Abs(transform.localPosition.x - ToDropLocation);
        float t = Mathf.Clamp01(distanceToDrop / 2200f);
        currentSpeed = Mathf.Max(Mathf.Lerp(0, speed, t), 0.1f);

    }
}
