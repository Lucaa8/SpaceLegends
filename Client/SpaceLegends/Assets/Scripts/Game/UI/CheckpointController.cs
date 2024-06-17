using UnityEngine;
using UnityEngine.UI;

public class CheckpointController : MonoBehaviour
{
    private Slider slider;
    private Image checkpointImage;

    void Start()
    {
        slider = transform.GetComponent<Slider>();
        checkpointImage = transform.Find("Checkpoint").GetComponent<Image>();
    }

    public void PositionCheckpoint()
    {
        float sliderWidth = slider.GetComponent<RectTransform>().rect.width;
        Vector3 checkpointPosition = checkpointImage.rectTransform.localPosition;
        checkpointPosition.x = (slider.value - 0.5f) * sliderWidth;
        checkpointImage.rectTransform.localPosition = checkpointPosition;
    }

    public void PositionPlayer(float currentProgress)
    {
        if (currentProgress < 0f)
        {
            currentProgress = 0f;
        }
        slider.value = currentProgress;
    }

}
