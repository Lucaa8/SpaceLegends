using UnityEngine;

public class Keys_6 : MonoBehaviour
{

    [SerializeField] GameObject DoorStar2;

    public void SetStar2State(string state)
    {
        bool picked = state == "pick";

        if (picked)
        {
            DoorStar2.transform.localPosition = new Vector3(83f, -11.702f, 0);
        }
        else
        {
            DoorStar2.transform.localPosition = new Vector3(44.825f, -10.66f, 0);
        }

    }
}
