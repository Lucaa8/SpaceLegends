using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CaseCell : MonoBehaviour
{

    public enum Rarity
    {
        COMMON, RARE, EPIC, LEGENDARY
    }

    [System.Serializable]
    public enum RelicType
    {
        EARTH, MARS
    }

    [System.Serializable]
    public class NFT
    {
        public Sprite sprite;
        public Rarity rarity;
        public string name;
    }

    [SerializeField] List<NFT> sprites = new List<NFT>();

    private Dictionary<Rarity, float> chances = new Dictionary<Rarity, float>();

    [SerializeField] RelicType type;

    void Awake()
    {
        if(type == RelicType.EARTH)
        {
            chances = new Dictionary<Rarity, float> { { Rarity.COMMON, .7f }, { Rarity.RARE, .25f }, { Rarity.EPIC, .05f }, { Rarity.LEGENDARY, 0f } };
        }
        else
        {
            chances = new Dictionary<Rarity, float> { { Rarity.COMMON, .7f }, { Rarity.RARE, .25f }, { Rarity.EPIC, .04f }, { Rarity.LEGENDARY, 0.01f } };
        }
    }

    // Start is called before the first frame update
    public void Setup(Sprite result, Color color)
    {

        if (result != null)
        {
            GetComponent<Image>().sprite = result;
            transform.parent.GetComponent<Image>().color = color;
            return;
        }

        NFT nft = GetRandomDrop();

        transform.parent.GetComponent<Image>().color = CaseSroll.colors[nft.rarity];
        GetComponent<Image>().sprite = nft.sprite;

    }

    private NFT GetRandomDrop()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        float cumulativeProbability = 0f;

        foreach (var rarity in chances)
        {
            cumulativeProbability += rarity.Value;

            if (randomValue <= cumulativeProbability)
            {
                return GetRandomByRarity(rarity.Key);
            }
        }

        return null; // Not going to happen if everything is configured nicely
    }

    private NFT GetRandomByRarity(Rarity rarity)
    {
        List<NFT> skinsByRarity = sprites.Where(s => s.rarity == rarity).ToList();
        int randomIndex = UnityEngine.Random.Range(0, skinsByRarity.Count);
        return skinsByRarity[randomIndex];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
