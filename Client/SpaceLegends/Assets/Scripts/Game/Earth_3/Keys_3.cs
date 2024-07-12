using UnityEngine;

public class Keys_3 : MonoBehaviour
{

    [SerializeField] GameObject DoorStar2;

    public void SetStar2State(string state)
    {
        bool picked = state == "pick";

        if (picked)
        {
            DoorStar2.transform.localPosition = new Vector3(83f, -6.701f, 0);
        }
        else
        {
            DoorStar2.transform.localPosition = new Vector3(83f, -5.657f, 0);
        }

    }

}
