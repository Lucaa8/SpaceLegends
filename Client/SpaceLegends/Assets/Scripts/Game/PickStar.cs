using UnityEngine;

public class PickStar : MonoBehaviour
{

    public void PickAnimationEnd()
    {
        transform.gameObject.SetActive(false);
    }

}
