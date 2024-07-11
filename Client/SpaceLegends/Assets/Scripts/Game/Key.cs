using UnityEngine;
using UnityEngine.Events;

public class Key : MonoBehaviour
{

    public bool Picked = false;
    public bool Saved = false;

    [SerializeField] UnityEvent<string> OnKeyCollected;

    public void ExecuteEvent()
    {
        if(OnKeyCollected != null)
        {
            OnKeyCollected.Invoke("pick");
        }
    }

    public void ResetState()
    {
        GetComponent<Collider2D>().enabled = true;
        transform.gameObject.SetActive(true);
        Picked = false;
        if (OnKeyCollected != null)
        {
            OnKeyCollected.Invoke("reset");
        }       
    }

    public void Remove()
    {
        if(Picked)
        {
            transform.gameObject.SetActive(false);
        }
    }

}
