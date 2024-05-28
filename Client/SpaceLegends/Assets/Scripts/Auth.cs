using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Auth : MonoBehaviour
{

    public static Auth Instance { get; private set; }

    static Auth()
    {
        Instance = new Auth();
    }

    private static string BaseUrl = "https://space-legends.luca-dc.ch/";

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
    }

    public void Logout()
    {
        MakeRequest(GetAuthURL("logout"), UnityWebRequest.kHttpVerbDELETE, new JObject { { "refresh", getRefreshToken() } }, AuthType.ACCESS, (ok, status, jrep) =>
        {
            ClearTokens();
            //TODO Login Scene
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
            else
            {
                ClearTokens();
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
                StartCoroutine(RefreshToken((new_access) =>
                {
                    if (!string.IsNullOrEmpty(new_access))
                    {
                        StartCoroutine(_MakeRequest(url, method, body, authentification, onResponse));
                    }
                    else
                    {
                        Debug.Log("Logged out");
                        //TODO Login Scene
                    }
                }));
            }
            else
            {
                onResponse(ok, status, jrep);
            }
        });
    }

    private IEnumerator _MakeRequest(string url, string method, JObject body, AuthType authentification, OnResponse onResponse)
    {

        UnityWebRequest request = null;
        switch (method)
        {
            case UnityWebRequest.kHttpVerbGET:
                request = UnityWebRequest.Get(url);
                break;
            case UnityWebRequest.kHttpVerbPOST:
                request = new UnityWebRequest(url, "POST");
                if(body != null)
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(body.ToString());
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }
                break;
            case UnityWebRequest.kHttpVerbDELETE:
                request = UnityWebRequest.Delete(url);
                break;

        }

        if(request == null || string.IsNullOrEmpty(request.url))
        {
            yield break;
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

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            onResponse(false, -1, null);
        }
        else
        {
            bool isOk = UnityWebRequest.Result.Success == request.result;
            long status = request.responseCode;
            if (request.GetResponseHeader("Content-Type").Equals("application/json"))
            {
                try
                {
                    JObject jrep = JObject.Parse(request.downloadHandler.text);
                    onResponse(isOk, status, jrep);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while parsing MakeRequest response. Error: " + e.Message);
                    onResponse(isOk, status, new JObject{{ "message", "An unexepted error occurred while parsing the response." }});
                }
            }
            else
            {
                onResponse(isOk, status, null);
            }
        }

    }

}
