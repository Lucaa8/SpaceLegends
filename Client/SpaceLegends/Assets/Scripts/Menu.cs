using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    [SerializeField] TMP_Text TxtUser;
    [SerializeField] GameObject SettingsObject;
    [SerializeField] Button SettingsButton;
    [SerializeField] TMP_Dropdown Settings_CbxResolution;
    [SerializeField] TMP_Dropdown Settings_CbxScreenMode;
    [SerializeField] Button Settings_BtnLogout;
    [SerializeField] Button Settings_BtnQuit;

    private bool areSettingsAnimated = false;

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
        if(!Screen.fullScreen)
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

    // Start is called before the first frame update
    void Start()
    {

        InitResolutionSettings();
        InitScreenModeSettings();

        SettingsButton.onClick.AddListener(() =>
        {
            if(!areSettingsAnimated)
            {
                StartCoroutine(Settings(!SettingsObject.activeInHierarchy));
            }
        });
        Settings_BtnQuit.onClick.AddListener(() => Application.Quit());

        if (Auth.Instance != null)
        {
            Settings_BtnLogout.onClick.AddListener(() => StartCoroutine(Auth.Instance.Logout()));
            TxtUser.text = "Welcome back " + Auth.Instance.GetDisplayname();
        }
    }

    private bool isESCPressed = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isESCPressed = true;
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            isESCPressed = false;
        }
        if(isESCPressed && SettingsObject.activeInHierarchy && !areSettingsAnimated)
        {
            CloseSettings();
            isESCPressed = false;
        }
    }

    public void CloseSettings()
    {
        StartCoroutine(Settings(false)); //For the cross inside the settings window
    }

    public IEnumerator Settings(bool show)
    {
        areSettingsAnimated = true;

        if (show) //Cannot do SettingsObject.SetActive(show); because in the false case, the gameobject would be deactivated before the canvas opacity animation played.
        {
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

        if(!show) 
        {
            SettingsObject.SetActive(false);
        }

        areSettingsAnimated = false;
    }

}
