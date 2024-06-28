using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    private Connection connection;
    private Rigidbody2D player;
    private SpriteRenderer sprite;
    private Animator animator;
    private Vector3 initialScale;

    [SerializeField] GameObject StartPoint;
    [SerializeField] GameObject EndPoint;
    private float levelLength;

    [SerializeField] CheckpointController checkpointController;
    private Vector3 lastCheckpoint;

    [SerializeField] GameObject WinScreen;
    [SerializeField] GameObject QuitScreen;

    [SerializeField] GameObject DeathScreen;
    [SerializeField] TMP_Text DeathScreenRemaining;
    [SerializeField] CanvasGroup RespawnButton;

    [SerializeField] GameObject Stars;

    public bool IsAlive { get; private set; } = true;
    [SerializeField] float MaxHealth;
    private float currentHealth;
    [SerializeField] Image ImageHealth;

    public static float ArmorModifier = 0f;

    [SerializeField] float PassiveDamageAmount = .25f;
    private float damageInterval = .5f; // Time interval between each passive damage tick
    private float timeSinceLastDamage;
    private bool isTakingPassiveDamage = false;

    private void Start()
    {
        // Because scene names are following the pattern: CollectionName_LevelID, e.g. Earth_0, Mars_1
        AudioManager.Instance.PlayLevelMusic(SceneManager.GetActiveScene().name.Split('_')[0]);
        player = transform.GetComponent<Rigidbody2D>();
        connection = transform.GetComponent<Connection>();
        connection.OnStart(() => {
            // Decrease opacity of already picked stars
            GetStars().Where(s => connection.getStar(s.StarNumber)).ToList()
            .ForEach(s => {
                s.Enabled = false;
                s.Saved = true;
                s.transform.GetComponent<SpriteRenderer>().color = new Color(89f / 255f, 87f / 255f, 69f / 255f, 0.5f);
            });
            player.simulated = true;
        });
        transform.position = StartPoint.transform.position;
        sprite = transform.GetComponent<SpriteRenderer>();
        animator = transform.GetComponent<Animator>();    
        initialScale = transform.localScale;
        currentHealth = MaxHealth;
        lastCheckpoint = transform.position;
        levelLength = Mathf.Abs(EndPoint.transform.position.x - StartPoint.transform.position.x);
        UpdateHealth();

        StartCoroutine(DisplayDiscord());
    }

    private IEnumerator DisplayDiscord()
    {
        yield return new WaitForSeconds(1f);
        string collec = SceneManager.GetActiveScene().name.Split("_")[0];
        DiscordManager.Instance.ChangeActivity("On " + collec, "Level 1", collec.ToLower());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UpdateQuitScreen();
        }
        if (!IsAlive || !player.simulated)
        {
            return;
        }
        if (currentHealth <= 0f)
        {
            Die(true);
        }
        TakeDamage();
        float playerPosition = player.position.x - StartPoint.transform.position.x;
        checkpointController.PositionPlayer(playerPosition / levelLength);
    }

    public Connection GetConnection()
    {
        return connection;
    }

    public void UpdateQuitScreen()
    {
        // Disable player movement and hitboxes before updating the active status
        player.simulated = QuitScreen.activeInHierarchy;
        StartCoroutine(ShowScreen(!QuitScreen.activeInHierarchy, QuitScreen));
    }

    public void Die(bool animate)
    {
        IsAlive = false;
        connection.AddDeath();
        AudioManager.Instance.PlaySound(AudioManager.Instance.sfxPlayerDie);
        GetStars().Where(s => !s.Saved).ToList()
        .ForEach(s =>
        {
            s.ResetStar();
            connection.unsetStar(s.StarNumber);
        });
        if (animate)
        {
            animator.SetBool("IsDead", true);
        }
        else
        {
            player.velocity = Vector3.zero;
            transform.position = lastCheckpoint; //Reset position of the player to avoid him to see unwanted background transitions
            ShowDeathScreen();
        }
    }

    public void ShowDeathScreen()
    {
        DeathScreenRemaining.text = "You have " + connection.Lives.ToString();
        RespawnButton.alpha = connection.Lives <= 0 ? 0.5f : 1f;
        RespawnButton.interactable = connection.Lives > 0;
        StartCoroutine(ShowScreen(true, DeathScreen));
    }

    //Called by the end of death sprite animation
    public void TryRespawn()
    {
        connection.DecreaseLives((j) =>
        {
            if(j.Value<bool>("status"))
            {
                Respawn();
            }
        });
    }

    private void Respawn()
    {
        StartCoroutine(ShowScreen(false, DeathScreen));
        currentHealth = MaxHealth;
        UpdateHealth();
        player.velocity = Vector3.zero;
        transform.position = lastCheckpoint;
        IsAlive = true;
        animator.SetBool("IsDead", false);
    }

    private void TakeDamage()
    {
        if (isTakingPassiveDamage)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= damageInterval)
            {
                TakeDamage(PassiveDamageAmount);
                timeSinceLastDamage = 0f;
            }
        }
        else
        {
            timeSinceLastDamage = damageInterval;
        }
    }

    public void TakeDamage(float damage)
    {
        damage -= damage * ArmorModifier;
        AudioManager.Instance.PlaySound(AudioManager.Instance.sfxPlayerHurt);
        currentHealth -= damage;
        UpdateHealth();
        StartCoroutine(ShowDamage(damageInterval / 2));
    }

    private void UpdateHealth()
    {
        float ratio = currentHealth / MaxHealth;
        ImageHealth.transform.localScale = new Vector2(ratio < 0f ? 0f : ratio, ImageHealth.transform.localScale.y);
    }

    private IEnumerator ShowDamage(float seconds)
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(seconds);
        sprite.color = Color.white;
    }

    private bool isPassiveDamage(GameObject go)
    {
        return go.CompareTag("Spikes") || go.CompareTag("Campfire");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject g = collision.gameObject;
        if(isPassiveDamage(g) && !isTakingPassiveDamage)
        {
            isTakingPassiveDamage = true;
        }
        else if(g.CompareTag("Star"))
        {
            PickStar state = g.GetComponent<PickStar>();
            state.Pick();
            if(state.Enabled) // Avoid adding already picked star
            {
                AudioManager.Instance.PlaySound(AudioManager.Instance.sfxPickStar);
                connection.setStar(state.StarNumber);
            }     
        }
        else if(g.CompareTag("Potion"))
        {
            if(currentHealth == MaxHealth)
            {
                return;
            }
            g.GetComponent<Animator>().SetTrigger("Pick");
            currentHealth = MaxHealth;
            UpdateHealth();
            AudioManager.Instance.PlaySound(AudioManager.Instance.sfxPickItem);
        }
        else if(g.CompareTag("Checkpoint"))
        {
            CheckpointAnimations anim = g.GetComponent<CheckpointAnimations>();
            anim.Touch();
            collision.enabled = false;
            lastCheckpoint = collision.transform.position;
            checkpointController.PositionCheckpoint();
            AudioManager.Instance.PlaySound(AudioManager.Instance.sfxCheckpoint);
            // Save new stars so the player do not lost them anymore when dying
            GetStars().Where(s => s.Enabled).ToList()
            .ForEach(s =>
            {
                if(connection.getStar(s.StarNumber))
                {
                    s.Saved = true;
                }
            });
        }
        else if(g.CompareTag("DeadLine"))
        {
            Die(false);
        }
        else if(g.CompareTag("Finish"))
        {
            connection.Completed = true;
            player.velocity = Vector2.zero;
            player.simulated = false;
            StartCoroutine(ShowScreen(true, WinScreen));
            SetupWinScreen();
            AudioManager.Instance.PlayGameState(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isPassiveDamage(collision.gameObject) && isTakingPassiveDamage)
        {
            isTakingPassiveDamage = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isPassiveDamage(collision.gameObject) && !isTakingPassiveDamage)
        {
            isTakingPassiveDamage = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isPassiveDamage(collision.gameObject) && isTakingPassiveDamage)
        {
            isTakingPassiveDamage = false;
        }
    }

    private IEnumerator ShowScreen(bool show, GameObject toShow)
    {

        // If the player is dead (I show death screen), wait a little that the dead sound ends and then show death screen
        if (show && toShow == DeathScreen)
        {
            yield return new WaitForSeconds(0.2f);        
        }

        if (show)
        {
            toShow.SetActive(true);
        }

        float duration = 0.3f; // seconds
        float elapsedTime = 0f;
        CanvasGroup group = toShow.GetComponent<CanvasGroup>();
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
            toShow.SetActive(false);
        }
    }

    private void SetupWinScreen()
    {
        WinScreen.transform.Find("Image/Stats/Kills/Kills").GetComponent<TMP_Text>().text = connection.Kills.ToString();
        WinScreen.transform.Find("Image/Stats/Deaths/Deaths").GetComponent<TMP_Text>().text = connection.Deaths.ToString();
        for (int i = 1; i <= 3; i++)
        {
            if (connection.getStar(i))
            {
                WinScreen.transform.Find("Image/Stars/Star" + i.ToString() + "/Y").GetComponent<Image>().gameObject.SetActive(true);
            }
        }
        connection.OnEnd((jrep) =>
        {

            WinScreen.transform.Find("Image/Stats/TimeSpent/Time").GetComponent<TMP_Text>().text = FormatTime(jrep.Value<int>("time"));

            JObject reward = jrep.Value<JObject>("reward");
            string type = reward.Value<string>("type");
            if(type == "SDT")
            {
                Transform sdt = WinScreen.transform.Find("Image/Reward/SDT");
                sdt.Find("Text").transform.GetComponent<TMP_Text>().text = reward.Value<float>("value").ToString();
                sdt.gameObject.SetActive(true);
            }
            else if (type == "HEART")
            {
                Transform hearts = WinScreen.transform.Find("Image/Reward/Hearts");
                hearts.Find("Text").transform.GetComponent<TMP_Text>().text = reward.Value<int>("value").ToString();
                hearts.gameObject.SetActive(true);
            }
            else if (type == "RELIC")
            {
                Transform rel = WinScreen.transform.Find("Image/Reward/Relic");
                rel.Find("Text").transform.GetComponent<TMP_Text>().text = reward.Value<string>("value");
                rel.gameObject.SetActive(true);
            }
            else
            {
                WinScreen.transform.Find("Image/Reward/None").gameObject.SetActive(true);
            }

            WinScreen.transform.Find("Image/Fetching").gameObject.SetActive(false);
            WinScreen.transform.Find("Image/Stats").gameObject.SetActive(true);
            WinScreen.transform.Find("Image/Stars").gameObject.SetActive(true);
            WinScreen.transform.Find("Image/Next").gameObject.SetActive(true);
            WinScreen.transform.Find("Image/Reward").gameObject.SetActive(true);

        });
    }

    public void EndAndQuit()
    {
        connection.Completed = false;
        AudioManager.Instance.PlayGameState(false); // Initially the death music but too long. So It will play like a "loose" sounds when the player quits
        // Waits that the communication ends before unloading the scene
        connection.OnEnd((jrep) =>
        {
            Next(); 
        });
    }

    public static void Next()
    {
        LevelChanger manager = FindObjectOfType<LevelChanger>();
        if (manager != null)
        {
            manager.FadeToLevel("Menu");
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    private string FormatTime(int seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return string.Format("{0}h {1}m {2}s", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
    }

    public List<PickStar> GetStars()
    {
        List<PickStar> stars = new List<PickStar>();
        foreach (Transform starTransform in Stars.GetComponentsInChildren<Transform>(true))
        {
            if (starTransform == Stars.transform) continue;
            PickStar state = starTransform.GetComponent<PickStar>();
            stars.Add(state);
        }
        return stars;
    }

}
