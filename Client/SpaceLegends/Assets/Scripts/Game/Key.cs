using UnityEngine;

public class Key : MonoBehaviour
{

    [SerializeField] GameObject ToHide;

    public void Open()
    {
        ToHide.gameObject.SetActive(false);
    }

    public void Remove()
    {
        transform.gameObject.SetActive(false);
    }

}
