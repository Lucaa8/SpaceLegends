using UnityEngine;

public class Keys : MonoBehaviour
{

    [SerializeField] GameObject Star2Barrier;
    [SerializeField] MovingPlatform Star3Platform;
    [SerializeField] GameObject Star3Lock;

    public void SetStar2Barrier(string state)
    {
        Star2Barrier.SetActive(state == "reset");
    }

    public void SetStar3Platform(string state)
    {
        bool picked = state == "pick";
        if (picked)
        {
            Star3Platform.Speed = 2.5f;
        }
        else
        {
            Star3Platform.Speed = 0f;
            Star3Platform.Platform.localPosition = Vector3.zero;
        }
        Star3Lock.SetActive(!picked);
    }

}
