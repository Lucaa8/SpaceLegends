using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class Login : MonoBehaviour
{

    [SerializeField] TMP_InputField username;
    [SerializeField] TMP_InputField password;
    [SerializeField] TMP_Text error;


    public void Start()
    {
        Auth.OnResponse Callback = (ok, status, jrep) =>
        {
            Debug.Log("UWUWUWUWUWUWUWUWUW " + status.ToString());
            if (ok)
            {
                Debug.Log("Hello " + jrep.Value<string>("displayname") + "!");
                //TODO Menu Scene
            }
        };
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetAuthURL("user"), UnityWebRequest.kHttpVerbGET, null, Auth.AuthType.ACCESS, Callback));
    }

    public void quit()
    {
        Application.Quit();
    }

    public void login()
    {
        StartCoroutine(LoginCoroutine());
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
                    //todo fonctionne pas si le mot-de-passe est invalide (le backend retourne jarray au lieu de string
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
                HandleLoginResponse(JObject.Parse(www.downloadHandler.text));
            }
        }
    }

    private void HandleLoginResponse(JObject response)
    {
        Auth.Instance.UpdateTokens(response);
        Debug.Log(PlayerPrefs.GetString("a"));
        //TODO Menu Scene
    }

}
