using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    public int openedWindow = -1;

    [SerializeField] Settings _settings; //could inerhit common interface and have a Show(bool show) to call on button click but dont have time to do
    [SerializeField] Profile _profile;
    [SerializeField] Shop _shop;

    [SerializeField] TMP_Text TxtUser;
    [SerializeField] public GameObject PlayMenu;

    private void Start()
    {
        if(Auth.Instance != null)
        {
            TxtUser.text = "Welcome back " + Auth.Instance.GetDisplayname();
        }

        foreach (Button button in FindObjectsOfType<Button>(true))
        {
            AudioClip clip = AudioManager.Instance.sfxButtonClick;
            // Set a custom click sound for level icons buttons (why not?)
            if (button.gameObject.name.Contains("BgUnlocked"))
            {
                clip = AudioManager.Instance.sfxButtonClickLevel;                
            }
            button.onClick.AddListener(() => AudioManager.Instance.PlaySound(clip));
        }


        StartCoroutine(DisplayDiscord());

    }

    private IEnumerator DisplayDiscord()
    {
        yield return new WaitForSeconds(1f);
        DiscordManager.Instance.ChangeActivity("In the menu", "", "");
    }

    private bool isESCPressed = false;

    // Close current opened window
    void Update()
    {
        if(openedWindow < 0)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isESCPressed = true;
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            isESCPressed = false;
        }
        if(!isESCPressed)
        {
            return;
        }
        if (openedWindow == 0 && !_settings.areSettingsAnimated)
        {
            _settings.CloseSettings();
        }
        if (openedWindow == 1 && !_profile.isProfileAnimated)
        {
            _profile.CloseProfile();
        }
        if (openedWindow == 2 && !_shop.isShopAnimated)
        {
            _shop.CloseShop();
        }
        isESCPressed = false;
    }

}
