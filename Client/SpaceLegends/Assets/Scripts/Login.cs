using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using Discord;

public class Login : MonoBehaviour
{

    private delegate void OnVersionChecked(bool ok);

    [SerializeField] TMP_InputField username;
    [SerializeField] TMP_InputField password;
    [SerializeField] TMP_Text error;
    [SerializeField] Button BtnLogin;
    [SerializeField] Button BtnQuit;
    [SerializeField] Button BtnContinue;
    [SerializeField] Button BtnLogout;
    [SerializeField] CanvasGroup LoginForm;
    [SerializeField] CanvasGroup LoggedInForm;
    [SerializeField] TMP_Text LoggedInAs;
    [SerializeField] TMP_Text AppCredits;


    public void Start()
    {
        CheckVersion((ok) =>
        {
            if(ok)
            {
                BtnLogin.onClick.AddListener(LoginClick);
                BtnQuit.onClick.AddListener(QuitClick);
                BtnContinue.onClick.AddListener(ContinueClick);
                BtnLogout.onClick.AddListener(LogoutClick);
                GetUser(false);
            }
            else
            {
                //Display an error message
                //Maybe check if error is internet not available ? (I think code == -1 in this case?)
            }
        });

        foreach (Button button in FindObjectsOfType<Button>(true))
        {
            button.onClick.AddListener(() => AudioManager.Instance.PlaySound(AudioManager.Instance.sfxButtonClick));
        }

        StartCoroutine(DisplayDiscord());

    }

    private IEnumerator DisplayDiscord()
    {
        yield return new WaitForSeconds(1f);
        DiscordManager.Instance.ChangeActivity("Chilling on the login page", "Trying to remember his password...", "");
    }

    public void QuitClick()
    {
        Application.Quit();
    }

    public void LoginClick()
    {
        StartCoroutine(LoginCoroutine());
    }

    public void ContinueClick()
    {
        FindObjectOfType<LevelChanger>().FadeToLevel("Menu");
        AudioManager.Instance.PlayMenuMusic();
    }

    public void LogoutClick()
    {
        StartCoroutine(Auth.Instance.Logout());
        StartCoroutine(FadeForm(true));
    }

    public IEnumerator FadeForm(bool showLoginForm)
    {
        float duration = 0.5f; // seconds
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / duration;
            LoggedInForm.alpha = showLoginForm ? 1f - alpha : alpha;
            LoginForm.alpha = showLoginForm ? alpha : 1f - alpha;
            yield return null;
        }
        LoggedInForm.alpha = showLoginForm ? 0f : 1f;
        LoginForm.alpha = showLoginForm ? 1f : 0f;
        LoggedInForm.gameObject.SetActive(!showLoginForm);
        LoginForm.gameObject.SetActive(showLoginForm);
        Navigation navigation = BtnQuit.navigation;
        navigation.selectOnDown = showLoginForm ? username : BtnContinue;
        BtnQuit.navigation = navigation;
    }

    private IEnumerator LoginCoroutine()
    {
        error.gameObject.SetActive(false);

        WWWForm form = new WWWForm();
        form.AddField("username", username.text);
        form.AddField("password", password.text);

        using (UnityWebRequest www = UnityWebRequest.Post("https://space-legends.luca-dc.ch/auth/login", form))
        {
            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.ConnectionError)
            {          
                error.text = "Cannot reach remote server";
                error.gameObject.SetActive(true);
            }
            else if(www.result != UnityWebRequest.Result.Success)
            {              
                if(www.GetResponseHeader("Content-Type") == "application/json")
                {
                    error.text = JObject.Parse(www.downloadHandler.text).Value<string>("message");
                }
                else
                {
                    error.text = "An unexcepted error occured. Please try again later.";
                }
                error.gameObject.SetActive(true);
            }
            else
            {
                JObject jrep = JObject.Parse(www.downloadHandler.text);
                if(jrep != null)
                {
                    Auth.Instance.UpdateTokens(jrep);
                }
                GetUser(true);
            }
        }
    }

    private void CheckVersion(OnVersionChecked versionCheckOver)
    {
        AppCredits.text = "Space Legends v" + Auth.CLIENT_VERSION;
        Auth.OnResponse Callback = (ok, status, jrep) =>
        {
            if(status == 200 && jrep.Value<string>("version") == Auth.CLIENT_VERSION)
            {
                versionCheckOver(true);
            }
            else
            {
                versionCheckOver(false);
            }
        };
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("version"), UnityWebRequest.kHttpVerbGET, null, Auth.AuthType.NONE, Callback));
    }

    private void GetUser(bool Direct)
    {
        Auth.OnResponse Callback = (ok, status, jrep) =>
        {
            if (ok)
            {
                Auth.Instance.SetUser(jrep);
                if(!Direct)
                {
                    LoggedInAs.text = "You are logged in as " + Auth.Instance.GetUsername();
                    StartCoroutine(FadeForm(false));
                }
                else
                {
                    ContinueClick();
                }
            }
        };
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetAuthURL("user"), UnityWebRequest.kHttpVerbGET, null, Auth.AuthType.ACCESS, Callback));
    }

}
