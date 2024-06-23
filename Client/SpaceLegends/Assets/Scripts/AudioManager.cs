using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Audio sources")]
    [SerializeField] AudioSource music;
    [SerializeField] AudioSource sfx;

    [Header("Audio musics")]
    [SerializeField] AudioClip musicLogin;
    [SerializeField] AudioClip musicMenu;
    [SerializeField] AudioClip musicLevelEarth;
    [SerializeField] AudioClip musicLevelMars;

    [Header("Audio sounds")]
    public AudioClip sfxButtonClick;
    public AudioClip sfxButtonClickLevel;
    public AudioClip sfxPlayerJump;   
    public AudioClip sfxPlayerHit;
    public AudioClip sfxPlayerHurt;
    public AudioClip sfxPlayerDie;
    public AudioClip sfxCheckpoint;
    public AudioClip sfxPickStar;
    public AudioClip sfxPickItem;
    public AudioClip sfxWin;
    public AudioClip sfxLose;

    [Header("Other")]
    [SerializeField] float TransitionTime;
    [SerializeField] AudioClip DefaultClipOnRun;

    private List<AudioClip> mixMusicQueue = new List<AudioClip>();
    private bool isChangingMusic = false;

    private string musicKey = "MusicVolume";
    private string sfxKey = "SoundVolume";
    private float defaultVolume = 0.5f;

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(musicKey, defaultVolume);
    }

    public float GetSfxVolume()
    {
        return PlayerPrefs.GetFloat(sfxKey, defaultVolume);
    }

    private void Start()
    {
        music.volume = GetMusicVolume();
        music.loop = true;
        sfx.volume = GetSfxVolume();
        sfx.loop = false;
        if(DefaultClipOnRun != null)
        {
            mixMusicQueue.Add(DefaultClipOnRun);
        }
    }

    private void Update()
    {
        if (mixMusicQueue.Count > 0 && !isChangingMusic)
        {
            AudioClip play = mixMusicQueue[0];
            mixMusicQueue.RemoveAt(0);
            StartCoroutine(MixMusics(play));
        }
    }

    public void ChangeVolumeMusic(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        music.volume = volume;
    }

    public void ChangeVolumeSounds(float volume)
    {
        PlayerPrefs.SetFloat("SoundVolume", volume);
        sfx.volume = volume;
    }

    private IEnumerator MixMusics(AudioClip next)
    {
        if(music.volume == 0f)
        {
            music.Stop();
            music.clip = next;
            music.Play();
            yield break;
        }
        isChangingMusic = true;
        float volume = music.volume;
        float current = 0f;
        while(music.volume > 0f)
        {
            music.volume = Mathf.Lerp(volume, 0f, current);
            current += Time.deltaTime / TransitionTime;
            yield return null;
        }
        music.Stop();
        if(next == null)
        {
            music.volume = volume; //Reset the volume but do not play anything
            isChangingMusic = false;
            yield break;
        }
        music.clip = next;
        music.Play();
        float targetVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        current = 0f;
        while (music.volume < targetVolume)
        {
            music.volume = Mathf.Lerp(0f, targetVolume, current);
            current += Time.deltaTime / TransitionTime;
            yield return null;
        }
        isChangingMusic = false;
    }
    
    public void PlayLoginMusic()
    {
        mixMusicQueue.Add(musicLogin);
    }

    public void PlayMenuMusic()
    {
        mixMusicQueue.Add(musicMenu);
    }

    public void PlayLevelMusic(string level)
    {
        if(level == "Earth")
        {
            mixMusicQueue.Add(musicLevelEarth);
        }
        else
        {
            mixMusicQueue.Add(musicLevelMars);
        }
    }

    public void PlayGameState(bool isWin)
    {
        mixMusicQueue.Add(null); //Slowly stop the game level's music
        sfx.Stop(); //Stop any sound currently played
        PlaySound(isWin ? sfxWin : sfxLose); //Start the win/lose short music
    }

    public void PlaySound(AudioClip clip)
    {
        sfx.clip = clip;
        sfx.Play();
    }

}
