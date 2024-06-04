using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CaseSroll : MonoBehaviour
{

    [SerializeField] GameObject Prefab;

    [SerializeField] int Max = 150;
    [SerializeField] int DropLocation = 120;

    [SerializeField] float speed;
    private float currentSpeed;
    private Vector3 initialPosition;
    private bool isScrolling;

    private List<CaseCell> cells = new List<CaseCell>();

    [SerializeField] Sprite resultSprite;
    [SerializeField] Color resultColor;

    private float widthImg;
    private float widthScroll;
    private int spacing;

    public void Scroll()
    {
        if (isScrolling)
        {
            return;
        }

        if(cells.Count != 0)
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
                cell.Setup(resultSprite, resultColor);
                continue;
            }
            cell.Setup(null, Color.black /* unused */);
        }

        widthImg = GetComponentInChildren<CaseCell>().GetComponent<RectTransform>().rect.width;
        widthScroll = GetComponent<RectTransform>().rect.width;
        spacing = (int)GetComponent<HorizontalLayoutGroup>().spacing;

    }

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = new Vector3(transform.localPosition.x, transform.localPosition.y);
    }


    // Update is called once per frame
    void Update()
    {

        if(!isScrolling)
        {
            return;
        }

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition + Vector3.left * 100, currentSpeed * Time.deltaTime * 700);

        float drop = -1 * ((DropLocation * widthImg) + (DropLocation * spacing));

        if (transform.localPosition.x <= drop)
        {
            isScrolling = false;
            return;
        }

        float distanceToDrop = Mathf.Abs(transform.localPosition.x - drop);

        // Utiliser une interpolation linéaire pour réduire la vitesse en fonction de la distance restante
        float t = Mathf.Clamp01(distanceToDrop / 10000f); // Diviseur ajustable pour contrôler la vitesse de ralentissement
        Debug.Log(t);
        currentSpeed = Mathf.Lerp(0, speed, t); // Réduire progressivement la vitesse

        Debug.Log("Distance to drop: " + distanceToDrop + ", Current Speed: " + currentSpeed);

    }
}
