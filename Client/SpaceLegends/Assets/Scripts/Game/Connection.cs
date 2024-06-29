using Newtonsoft.Json.Linq;
using UnityEngine;

public class Connection : MonoBehaviour
{

    [SerializeField] int LevelID;

    private Rigidbody2D player;

    public delegate void Get(JObject jresponse);
    public delegate void Run();

    private string code;
    public string Level { get; private set; }

    public float SDT { get; private set; }
    public int Kills { get; private set; }
    public int Deaths { get; private set; }
    public int Lives { get; private set; }
    private bool[] stars = new bool[] { false, false, false };
    public bool Completed = false;

    public bool getStar(int star)
    {
        return stars[star - 1];
    }

    public void setStar(int star)
    {
        stars[star - 1] = true;
    }

    public void unsetStar(int star)
    {
        stars[star - 1] = false;
    }

    public void OnStart(Run next)
    {
        player = transform.GetComponent<Rigidbody2D>();
        if (Auth.Instance != null)
        {
            FetchLevel(next);
        }
        //Play offline
        else
        {
            Lives = 999;
            next();
        }
    }

    //Executed when player clicks on the home button of death screen or Next of win screen
    public void OnEnd(Get next)
    {
        if (Auth.Instance != null && code != null)
        {
            EndLevel(next);
        }
        else
        {
            JObject fake_rew = new JObject { { "type", "None" } };
            next(new JObject { { "reward", fake_rew }, { "time", 1 } });
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
                Lives = jrep.Value<int>("lives");
                JObject stars = jrep.Value<JObject>("stars");
                this.stars[0] = stars.Value<bool>("star_1");
                this.stars[1] = stars.Value<bool>("star_2");
                this.stars[2] = stars.Value<bool>("star_3");
                Level = jrep.Value<string>("level");
                if(Level == "Level 0")
                {
                    Level = "Tutorial";
                }
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

    private void EndLevel(Get next)
    {
        string URL = Auth.GetApiURL("stop-level");
        Auth.OnResponse onResponse = (ok, status, jrep) =>
        {
            if(ok)
            {
                next(jrep);
            }
            else
                Player.Next(); //Something went wrong...
        };
        JObject body = new JObject();
        //Needs to be encrypted
        body["code"] = code;
        body["completed"] = Completed;
        if(Completed)
        {
            body["stars"] = new JObject { { "star_1", stars[0] }, { "star_2", stars[1] }, { "star_3", stars[2] } };
            body["SDT"] = SDT;
        }
        StartCoroutine(Auth.Instance.MakeRequest(URL, "DELETE", body, Auth.AuthType.ACCESS, onResponse));
    }

    public void DecreaseLives(Get result)
    {
        JObject jobj = new JObject();
        if (Auth.Instance == null)
        {
            Lives--;
            jobj["status"] = true; //Allow respawning in offline mode;
            result(jobj);
            return;
        }
        string URL = Auth.GetApiURL("lives");

        Auth.OnResponse onResponse = (ok, status, jrep) =>
        {
            jobj["status"] = ok;
            if (ok) Lives--;
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
            Kills++;
            return;
        }
        JObject jobj = new JObject();
        jobj["code"] = code;
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("kills"), "POST", jobj, Auth.AuthType.ACCESS, (o, s, j) => { if(o) Kills++; }));
    }

    public void AddDeath()
    {
        if (Auth.Instance == null)
        {
            Deaths++;
            return;
        }
        JObject jobj = new JObject();
        jobj["code"] = code;
        StartCoroutine(Auth.Instance.MakeRequest(Auth.GetApiURL("deaths"), "POST", jobj, Auth.AuthType.ACCESS, (o, s, j) => { if (o) Deaths++; }));
    }

    public void AddSDT(float amount)
    {
        SDT += amount;
    }

}
