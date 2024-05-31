using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    [SerializeField] Button BtnLogout;
    [SerializeField] TMP_Text TxtUser;
    [SerializeField] TMP_Dropdown resolutionDropdown;

    // Start is called before the first frame update
    void Start()
    {
        int screenWidth = Screen.mainWindowDisplayInfo.width;
        int screenHeight = Screen.mainWindowDisplayInfo.height;

        resolutionDropdown.options.Clear();

        foreach (Resolution r in Screen.resolutions.Reverse())
        {
            if(r.width >= 1000)
            {
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(r.width + "x" + r.height));
            }
        }
        resolutionDropdown.RefreshShownValue();


        /*if (screenWidth > 1920)
        {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData("2560x1440"));
            resolutionDropdown.RefreshShownValue();
        }*/

        resolutionDropdown.onValueChanged.AddListener(delegate { ChangeResolution(); });
        if (Auth.Instance != null)
        {
            BtnLogout.onClick.AddListener(() => StartCoroutine(Auth.Instance.Logout()));
            TxtUser.text = "Welcome back " + Auth.Instance.GetDisplayname();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeResolution()
    {
        int selectedIndex = resolutionDropdown.value;
        string selectedOption = resolutionDropdown.options[selectedIndex].text;
        string[] dimensions = selectedOption.Split('x');

        int width = int.Parse(dimensions[0]);
        int height = int.Parse(dimensions[1]);

        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
    }

}
