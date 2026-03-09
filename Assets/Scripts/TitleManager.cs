using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public GameObject optionPanel;
    public AudioClip buttonSound;
    public AudioClip closeSound;
    public AudioClip titleBGM;
    [Header("소리 시작 시간(초)")]
    public float skipTime = 0.1f;
    public float soundDelay = 0.3f;
    private void Start()
    {
        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(titleBGM, 0f);
        }
    }
    public void GoToStageSelect()
    {
        ButtonSoundPlay();
        SceneManager.LoadScene("StageSelect");
    }
    public void Shutdown()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void ShowOption()
    {
        ButtonSoundPlay();
        optionPanel.SetActive(true);
    }
    public void CloseOption()
    {
        SoundManager.Instance.PlaySFX(closeSound, 0f);
        optionPanel.SetActive(false);
    }
    public void GoToGallery()
    {
        ButtonSoundPlay();
        SceneManager.LoadScene("Gallery");
    }
    public void ButtonSoundPlay()
    {
        SoundManager.Instance.PlaySFX(buttonSound, skipTime);
    }
}

