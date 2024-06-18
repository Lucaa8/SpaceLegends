using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Auth : MonoBehaviour
{

    public static Auth Instance { get; private set; }

    public void Awake()
    {
        if(Instance == null)
        {
            BaseUrl = "https://space-legends.luca-dc.ch";
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static string BaseUrl { get; private set; }

    private static Func<string, string, string> BuildURL = (sub, endpoint) =>
    {
        return $"{BaseUrl}/{sub}/{endpoint}";
    };

    public static Func<string, string> GetAuthURL = (endpoint) =>
    {
        return BuildURL("auth", endpoint);
    };

    public static Func<string, string> GetApiURL = (endpoint) =>
    {
        return BuildURL("api", endpoint);
    };

    public delegate void OnResponse(bool ok, long status, JObject jresponse);
    private delegate void OnRefresh(string new_access);

    public enum AuthType
    { 
        NONE, ACCESS, REFRESH
    }

    public void SetUser(JObject data)
    {
        PlayerPrefs.SetString("c", data.Value<string>("name"));
        PlayerPrefs.SetString("d", data.Value<string>("displayname"));
        PlayerPrefs.Save();
    }

    public String GetUsername()
    {
        return PlayerPrefs.GetString("c", "");
    }

    public String GetDisplayname()
    {
        return PlayerPrefs.GetString("d", "");
    }

    private string getAccessToken()
    {
        return PlayerPrefs.GetString("a", "");
    }

    private void setAccessToken(string token)
    {
        PlayerPrefs.SetString("a", token);
        PlayerPrefs.Save();
    }

    private string getRefreshToken()
    {
        return PlayerPrefs.GetString("b", "");
    }

    private void setRefreshToken(string token)
    {
        PlayerPrefs.SetString("b", token);
        PlayerPrefs.Save();
    }

    public void UpdateTokens(JObject data)
    {
        if (data.ContainsKey("access_token"))
        {
            setAccessToken(data.Value<string>("access_token"));
        }

        if (data.ContainsKey("refresh_token"))
        {
            setRefreshToken(data.Value<string>("refresh_token"));
        }
    }

    public void ClearTokens()
    {
        PlayerPrefs.DeleteKey("a");
        PlayerPrefs.DeleteKey("b");
        PlayerPrefs.DeleteKey("c");
        PlayerPrefs.DeleteKey("d");
    }

    private void _logout()
    {
        ClearTokens();
        if(SceneManager.GetActiveScene().name != "Login")
        {
            LevelChanger manager = FindObjectOfType<LevelChanger>();
            if(manager != null)
            {
                manager.FadeToLevel("Login");
            }
        }
    }

    public IEnumerator Logout()
    {
        yield return MakeRequest(GetAuthURL("logout"), UnityWebRequest.kHttpVerbDELETE, new JObject { { "refresh", getRefreshToken() } }, AuthType.ACCESS, (ok, status, jrep) =>
        {
            _logout();
        });
    }

    private IEnumerator RefreshToken(OnRefresh RefreshCallback)
    {
        yield return _MakeRequest(GetAuthURL("refresh"), UnityWebRequest.kHttpVerbPOST, null, AuthType.REFRESH, (ok, status, jrep) =>
        {
            string tok = null;
            if (ok && jrep != null && jrep.ContainsKey("access_token"))
            {
                tok = jrep.Value<string>("access_token");
                setAccessToken(tok);
            }
            RefreshCallback(tok);
        });
    }

    public IEnumerator MakeRequest(string url, string method, JObject body, AuthType authentification, OnResponse onResponse)
    {
        yield return _MakeRequest(url, method, body, authentification, (ok, status, jrep) =>
        {
            if(authentification == AuthType.ACCESS && status == 401)
            {
                OnRefresh onRefreshed = (new_access) =>
                {
                    if (!string.IsNullOrEmpty(new_access))
                    {
                        StartCoroutine(_MakeRequest(url, method, body, authentification, onResponse));
                    }
                    else
                    {
                        //The refresh failed, therefore the player needs to log him in again on the login page.
                        _logout();
                    }
                };
                StartCoroutine(RefreshToken(onRefreshed));
            }
            else
            {
                onResponse(ok, status, jrep);
            }
        });
    }

    private IEnumerator _MakeRequest(string url, string method, JObject body, AuthType authentification, OnResponse onResponse)
    {

        //If one of those method returns an empty string then It means the player is not logged in, avoid doing useless requests.
        if((authentification == AuthType.ACCESS && getAccessToken() == "") || (authentification == AuthType.REFRESH && getRefreshToken() == ""))
        {
            _logout(); //The player is already logged out but nevermind, the redirection to login scene is done inside _logout();
            yield break;
        }

        UnityWebRequest request = null;
        switch (method) //Pretty useless as I can just do "new UnityWebRequest(url, method);", but its just a verification if the method is wrong the method will quit.
        {
            case UnityWebRequest.kHttpVerbGET:
                request = UnityWebRequest.Get(url);
                break;
            case UnityWebRequest.kHttpVerbPOST:
                request = new UnityWebRequest(url, "POST");
                break;
            case UnityWebRequest.kHttpVerbDELETE:
                request = UnityWebRequest.Delete(url);
                break;
            case UnityWebRequest.kHttpVerbPUT:
                request = new UnityWebRequest(url, "PUT");
                break;
        }

        if(request == null || string.IsNullOrEmpty(request.url))
        {
            yield break;
        }

        if (body != null)
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(body.ToString());
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
        }

        request.downloadHandler = new DownloadHandlerBuffer();

        switch(authentification)
        {
            case AuthType.ACCESS:
                request.SetRequestHeader("Authorization", "Bearer " + getAccessToken());
                break;
            case AuthType.REFRESH:
                request.SetRequestHeader("Authorization", "Bearer " + getRefreshToken());
                break;
        }

        yield return request.SendWebRequest();

        bool isOk = false;
        long status = -1;
        JObject jrep = null;

        //If the user is not connected to the network or the remote server is not reachable, the callback will get -1 as status
        //Otherwise I do more work (in this condition)
        if (request.result != UnityWebRequest.Result.ConnectionError)
        {
            isOk = UnityWebRequest.Result.Success == request.result;
            status = request.responseCode;
            if (request.GetResponseHeader("Content-Type").Equals("application/json"))
            {
                try
                {
                    jrep = JObject.Parse(request.downloadHandler.text);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while parsing MakeRequest response. Error: " + e.Message);
                    jrep = new JObject { { "message", "An unexepted error occurred while parsing the JSON response." } };
                }
            }
        }

        try
        {
            onResponse(isOk, status, jrep);
        }
        catch(Exception e)
        {
            Debug.LogError("Something went wrong with a callback (From request at " + url + "). The status code was " + status.ToString() + ". Error;");
            Debug.LogException(e);
        }

    }

}
