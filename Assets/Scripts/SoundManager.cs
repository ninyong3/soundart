using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [Header("스피커")]
    public AudioSource bgmPlayer;
    public AudioSource sfxPlayer;
    [Header("사운드 볼륨 설정")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        bgmVolume = PlayerPrefs.GetFloat("SavedBGM", 1f); // 저장된 환경 불러오기, 없으면 기본값 1
        sfxVolume = PlayerPrefs.GetFloat("SavedSFX", 1f);
    }
    public void PlaySFX(AudioClip clip, float skipTime)
    {
        if (sfxPlayer != null && clip != null)
        {
            sfxPlayer.clip=clip;
            sfxPlayer.time=skipTime;
            sfxPlayer.volume = sfxVolume;
            sfxPlayer.Play();
        }
    }
    public void PlayBGM(AudioClip clip, float skipTime)
    {
        if (bgmPlayer.clip == clip)
        {
            return;
        }
        if(bgmPlayer != null && clip != null)
        {
            bgmPlayer.clip = clip;
            bgmPlayer.time=skipTime;
            bgmPlayer.volume = bgmVolume;
            bgmPlayer.Play();
        }
    }
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        bgmPlayer.volume=volume;
        PlayerPrefs.SetFloat("SavedBGM", volume);
        PlayerPrefs.Save();
    }
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        sfxPlayer.volume=volume;
        PlayerPrefs.SetFloat("SavedSFX", volume);
        PlayerPrefs.Save();
    }
    public void StopBGM()
    {
        bgmPlayer.Stop();
    }
}
