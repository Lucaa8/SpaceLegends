using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Play : MonoBehaviour
{

    [SerializeField] List<GameObject> Sections = new List<GameObject>();
    [SerializeField] List<GameObject> Backgrounds = new List<GameObject>();

    [SerializeField] Button Prev;
    [SerializeField] Button Next;

    [SerializeField] GameObject LevelBullets;

    private int current = 0;

    private void Start()
    {
        Prev.onClick.AddListener(PrevSection);
        Next.onClick.AddListener(NextSection);
    }

    private void ShowIndex()
    {
        int i = 0;
        foreach(Transform bullet in LevelBullets.transform)
        {
            bullet.GetChild(1).gameObject.SetActive(i++ != current);
        }
    }

    private void ShowSection()
    {
        Sections.ForEach(s => s.SetActive(false));
        Backgrounds.ForEach(s => s.SetActive(false));
        Sections[current].SetActive(true);
        Backgrounds[current].SetActive(true);
    }

    public void NextSection()
    {
        if (Sections.Count - 1 <= current)
        {
            return;
        }
        current++;
        ShowIndex();
        ShowSection();
    }

    public void PrevSection()
    {
        if (current <= 0)
        {
            return;
        }
        current--;
        ShowIndex();
        ShowSection();
    }

}
