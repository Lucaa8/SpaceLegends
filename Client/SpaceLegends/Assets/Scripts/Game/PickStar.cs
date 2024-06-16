using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickStar : MonoBehaviour
{

    public void PickAnimationEnd()
    {
        transform.gameObject.SetActive(false);
    }

}
