using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseCell : MonoBehaviour
{

    [System.Serializable]
    private class ListOfSprites
    {
        public List<Sprite> _sprites;
    }

    [SerializeField] List<ListOfSprites> sprites;

    [SerializeField] int[] chances;

    [SerializeField] Color[] _colors;

    // Start is called before the first frame update
    public void Setup(Sprite result, Color color)
    {

        if (result != null)
        {
            GetComponent<Image>().sprite = result;
            transform.parent.GetComponent<Image>().color = color;
            return;
        }

        int index = Randomize();

        List<Sprite> slist = sprites[index]._sprites;
        GetComponent<Image>().sprite = slist[Random.Range(0, slist.Count)];
        transform.parent.GetComponent<Image>().color = _colors[index];

    }

    private int Randomize()
    {
        int index = 0;
        for(int i = 0; i < chances.Length; i++)
        {
            int rand = Random.Range(0, 100);
            if(rand > chances[i])
            {
                return i;
            }
            index++;
        }
        return index;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
