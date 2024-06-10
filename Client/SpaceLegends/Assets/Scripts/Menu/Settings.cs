using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{

    [SerializeField] GameObject SettingsObject;
    [SerializeField] Button SettingsButton;
    [SerializeField] TMP_Dropdown Settings_CbxResolution;
    [SerializeField] TMP_Dropdown Settings_CbxScreenMode;
    [SerializeField] Button Settings_BtnLogout;
    [SerializeField] Button Settings_BtnQuit;

    [SerializeField] Menu _menu;

    public bool areSettingsAnimated = false;

    private void InitResolutionSettings()
    {
        int screenWidth = Screen.mainWindowDisplayInfo.width;
        int screenHeight = Screen.mainWindowDisplayInfo.height;
        int currentWidth = PlayerPrefs.GetInt("screenw", Screen.currentResolution.width);
        int currentHeight = PlayerPrefs.GetInt("screenh", Screen.currentResolution.height);
        int current = 0;

        List<string> added = new List<string>();

        foreach (Resolution r in Screen.resolutions.Reverse())
        {
            if (r.width >= 1280) //Under 1280px is too small, some text are so tiny they become unreadable
            {
                string res = r.width + "x" + r.height;
                if (added.Contains(res)) //Because this Screen.resolutions can contains multiple times "1920x1080" resolutions, one for 144hz, one for 60hz, etc...
                    continue;
                Settings_CbxResolution.options.Add(new TMP_Dropdown.OptionData(res));
                added.Add(res);
                if (r.width == currentWidth && r.height == currentHeight)
                {
                    Settings_CbxResolution.value = current;
                }
                current++;
            }
        }

        Settings_CbxResolution.RefreshShownValue();
        Settings_CbxResolution.onValueChanged.AddListener(delegate
        {
            int selectedIndex = Settings_CbxResolution.value;
            string selectedOption = Settings_CbxResolution.options[selectedIndex].text;
            string[] dimensions = selectedOption.Split('x');

            int width = int.Parse(dimensions[0]);
            int height = int.Parse(dimensions[1]);
            PlayerPrefs.SetInt("screenw", width);
            PlayerPrefs.SetInt("screenh", height);

            Screen.SetResolution(width, height, Screen.fullScreen);
        });
    }

    private void InitScreenModeSettings()
    {
        Settings_CbxScreenMode.options.Add(new TMP_Dropdown.OptionData("Fullscreen"));
        Settings_CbxScreenMode.options.Add(new TMP_Dropdown.OptionData("Windowed"));
        if (!Screen.fullScreen)
        {
            Settings_CbxScreenMode.value = 1;
        }

        Settings_CbxScreenMode.onValueChanged.AddListener(delegate
        {
            int screenWidth = PlayerPrefs.GetInt("screenw", Screen.width);
            int screenHeight = PlayerPrefs.GetInt("screenh", Screen.height);
            int selectedIndex = Settings_CbxScreenMode.value;
            string selectedOption = Settings_CbxScreenMode.options[selectedIndex].text;
            Screen.SetResolution(screenWidth, screenHeight, selectedOption == "Fullscreen");
        });

    }

    void Start()
    {

        InitResolutionSettings();
        InitScreenModeSettings();

        SettingsButton.onClick.AddListener(() =>
        {
            if (!areSettingsAnimated)
            {
                StartCoroutine(ShowSettings(!SettingsObject.activeInHierarchy));
            }
        });
        Settings_BtnQuit.onClick.AddListener(() => Application.Quit());

        if (Auth.Instance != null)
        {
            Settings_BtnLogout.onClick.AddListener(() => StartCoroutine(Auth.Instance.Logout()));        
        }
    }

    public void CloseSettings()
    {
        StartCoroutine(ShowSettings(false)); //For the cross inside the settings window
    }

    public IEnumerator ShowSettings(bool show)
    {

        if((show && _menu.openedWindow != -1) || (!show && _menu.openedWindow != 0))
        {
            yield break;
        }

        areSettingsAnimated = true;

        if (show) //Cannot do SettingsObject.SetActive(show); because in the false case, the gameobject would be deactivated before the canvas opacity animation played.
        {
            _menu.openedWindow = 0;
            _menu.PlayMenu.SetActive(false);
            SettingsObject.SetActive(true);
        }

        float duration = 0.3f; // seconds
        float elapsedTime = 0f;
        CanvasGroup group = SettingsObject.GetComponent<CanvasGroup>();
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / duration;
            group.alpha = show ? alpha : 1f - alpha;
            yield return null;
        }
        group.alpha = show ? 1f : 0f;

        if (!show)
        {
            SettingsObject.SetActive(false);
            _menu.PlayMenu.SetActive(true);
            _menu.openedWindow = -1;
        }

        areSettingsAnimated = false;
    }

}
