using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialController : MonoBehaviour
{

    [TextArea] public string Text;

    [SerializeField] GameObject Panel;
    private TMP_Text tmpText;

    private static bool isShowing = false;

    [SerializeField] GameObject BarrierBottom;

    private void Start()
    {
        tmpText = Panel.GetComponentInChildren<TMP_Text>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(transform.name == "Checkpoints")
        {
            BarrierBottom.gameObject.SetActive(false);
        }
        if(collision.gameObject.CompareTag("Player"))
        {
            tmpText.text = Text;
            StartCoroutine(SetAndShowOrHide(true));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(SetAndShowOrHide(false));
            GetComponent<Collider2D>().enabled = false;
        }
    }

    private IEnumerator SetAndShowOrHide(bool show)
    {

        if (isShowing)
        {
            yield break;
        }

        isShowing = show;

        float duration = 0.3f; // seconds
        float elapsedTime = 0f;
        CanvasGroup group = Panel.GetComponent<CanvasGroup>();
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / duration;
            group.alpha = show ? alpha : 1f - alpha;
            yield return null;
        }
        group.alpha = show ? 1f : 0f;

        isShowing = false;

    }

}
