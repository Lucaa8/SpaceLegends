using UnityEngine;

public class PickStar : MonoBehaviour
{

    public int StarNumber;
    public bool Enabled = true;

    public void PickAnimationEnd()
    {
        transform.gameObject.SetActive(false);
    }

}
