using Newtonsoft.Json.Linq;
using UnityEngine;

public class Connection : MonoBehaviour
{

    [SerializeField] int LevelID;

    private Rigidbody2D player;

    public delegate void Get(JObject jresponse);
    private delegate void Run();

    private string code;

    void Start()
    {
        player = transform.GetComponent<Rigidbody2D>();
        Run next = () =>
        {
            player.simulated = true;
        };
        if (Auth.Instance != null)
        {
            FetchLevel(next);
        }
        //Play offline
        else
        {
            next();
        }
    }

    //Executed when player clicks on the home button of death screen or Next of win screen
    public void End()
    {
        Run next = () =>
        {
            LevelChanger manager = FindObjectOfType<LevelChanger>();
            if (manager != null)
            {
                manager.FadeToLevel("Menu");
            }
        };
        if (Auth.Instance != null && code != null)
        {
            EndLevel(next);
        }
        else
        {
            next();
        }
    }

    private void FetchLevel(Run next)
    {
        string URL = Auth.GetApiURL("start-level/"+LevelID);
        Auth.OnResponse onResponse = (ok, status, jrep) =>
        {
            if(ok)
            {
                code = jrep.Value<string>("code");
                next();
            }
            else
            {
                // The level is not unlocked
                LevelChanger manager = FindObjectOfType<LevelChanger>();
                if (manager != null)
                {
                    manager.FadeToLevel("Menu");
                }
            }
        };
        StartCoroutine(Auth.Instance.MakeRequest(URL, "PUT", null, Auth.AuthType.ACCESS, onResponse));
    }

    private void EndLevel(Run next)
    {
        string URL = Auth.GetApiURL("stop-level/");
        Auth.OnResponse onResponse = (ok, status, jrep) =>
        {
            if(ok)
            {
                //Reward
            }
            next();
        };
        JObject body = new JObject();
        //Needs to be encrypted
        body["code"] = code;
        body["completed"] = true;
        StartCoroutine(Auth.Instance.MakeRequest(URL, "DELETE", body, Auth.AuthType.ACCESS, onResponse));
    }

    public void GetLives(Get result)
    {
        JObject jempty = new JObject();
        if (Auth.Instance == null)
        {
            jempty.Add("count", 999); //Allow respawning in offline mode
            result(jempty);
            return;
        }
        string URL = Auth.GetApiURL("lives");
        Auth.OnResponse onResponse = (ok, status, jrep) =>
        {
            if(!ok)
            {
                jempty.Add("count", 0);
                result(jempty);
            }
            else
            {
                result(jrep);
            }
            
        };
        StartCoroutine(Auth.Instance.MakeRequest(URL, "GET", null, Auth.AuthType.ACCESS, onResponse));
    }

    public void DecreaseLives(Get result)
    {
        JObject jobj = new JObject();
        if (Auth.Instance == null)
        {
            jobj["status"] = true; //Allow respawning in offline mode;
            result(jobj);
            return;
        }
        string URL = Auth.GetApiURL("lives");

        Auth.OnResponse onResponse = (ok, status, jrep) =>
        {
            jobj["status"] = ok;
            result(jobj);
        };

        JObject body = new JObject();
        body["count"] = 1;

        StartCoroutine(Auth.Instance.MakeRequest(URL, "POST", body, Auth.AuthType.ACCESS, onResponse));
    }

    public void AddKill()
    {
        if (Auth.Instance == null)
        {
            return;
        }
        JObject jobj = new JObject();
        jobj["code"] = code;
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("kills"), "POST", jobj, Auth.AuthType.ACCESS, (o, s, j) => { }));
    }

    public void AddDeath()
    {
        if (Auth.Instance == null)
        {
            return;
        }
        JObject jobj = new JObject();
        jobj["code"] = code;
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("deaths"), "POST", jobj, Auth.AuthType.ACCESS, (o, s, j) => { }));
    }

    public void AddStar()
    {
        if (Auth.Instance == null)
        {
            return;
        }
        JObject jobj = new JObject();
        jobj["code"] = code;
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("stars"), "POST", jobj, Auth.AuthType.ACCESS, (o, s, j) => { }));
    }

}
