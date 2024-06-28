using UnityEngine;
using Discord;
using System;

public class DiscordManager : MonoBehaviour
{

    public static DiscordManager Instance { get; private set; }

    private Discord.Discord discord;

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            try
            {
                discord = new Discord.Discord(1256303445523038369, (ulong)CreateFlags.NoRequireDiscord);
            }
            catch(Exception)
            {
                Debug.Log("Discord not found.");
                discord = null;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        if (discord != null)
        {
            discord.Dispose();
        }
        
    }

    public void ChangeActivity(string where, string details)
    {
        if (discord == null)
            return;
        try
        {
            var activityManager = discord.GetActivityManager();
            var activity = new Activity
            {
                State = where,
                Details = details,
                Timestamps =
                {
                    Start = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };
            activityManager.UpdateActivity(activity, (res) => { });
        }
        catch (Exception)
        {
            Debug.Log("Discord closed");
            discord = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (discord == null)
            return;
        try
        {
            discord.RunCallbacks();
        }
        catch (Exception)
        {
            Debug.Log("Discord closed");
            discord = null;
        }
    }
}
