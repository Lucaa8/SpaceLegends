using System.Collections;
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
    public AudioClip sfxPlayerDie;
    public AudioClip sfxPlayerHit;
    public AudioClip sfxEnemyDie;
    public AudioClip sfxHurt;
    public AudioClip sfxCheckpoint;
    public AudioClip sfxPickStar;
    public AudioClip sfxPickItem;

    [Header("Other")]
    [SerializeField] float transitionTime;

    private void Start()
    {
        music.volume = PlayerPrefs.GetFloat("VolumeMusic", 0.5f);
        music.loop = true;
        sfx.volume = PlayerPrefs.GetFloat("SoundVolume", 0.5f);
        sfx.loop = false;
        PlayLoginMusic();
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
        float volume = music.volume;
        float current = 0f;
        while(music.volume > 0f)
        {
            music.volume = Mathf.Lerp(volume, 0f, current);
            current += Time.deltaTime / transitionTime;
            yield return null;
        }
        music.Stop();
        music.clip = next;
        music.Play();
        current = 0f;
        while(music.volume < volume)
        {
            music.volume = Mathf.Lerp(0f, volume, current);
            current += Time.deltaTime / transitionTime;
            yield return null;
        }
    }
    
    public void PlayLoginMusic()
    {
        StartCoroutine(MixMusics(musicLogin));
    }

    public void PlayMenuMusic()
    {
        StartCoroutine(MixMusics(musicMenu));
    }

    public void PlayEarthMusic()
    {
        StartCoroutine(MixMusics(musicLevelEarth));
    }

    public void PlayWinMusic()
    {

    }

    public void PlaySound(AudioClip clip)
    {
        sfx.clip = clip;
        sfx.Play();
    }

}
