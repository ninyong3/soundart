using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("사운드 볼륨 설정")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Header("프리셋 색상들")]
    public Color[] presetColors = new Color[4] { Color.black, Color.black, Color.black, Color.black };
    [Header("현재 선택한 색상")]
    public int selectedColorIndex = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            bgmVolume = PlayerPrefs.GetFloat("SavedBGM", 1f); // 저장된 환경 불러오기, 없으면 기본값 1
            sfxVolume = PlayerPrefs.GetFloat("SavedSFX", 1f);     
            for(int i=0;i<4;i++)
            {
                string hex = PlayerPrefs.GetString("SavedColor" + i, "#000000");
                if(ColorUtility.TryParseHtmlString(hex, out Color loadedColor))
                {
                    presetColors[i] = loadedColor;
                }
            }
            selectedColorIndex = PlayerPrefs.GetInt("SelectedColorIndex", 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        PlayerPrefs.SetFloat("SavedBGM", volume);
        PlayerPrefs.Save();
    }
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        PlayerPrefs.SetFloat("SavedSFX", volume);
        PlayerPrefs.Save();
    }
    public void SaveColor(int index, Color color)
    {
        presetColors[index] = color;
        PlayerPrefs.SetString("SavedColor" + index, "#" + ColorUtility.ToHtmlStringRGB(color));
        PlayerPrefs.Save();
    }
    public void SetSelectedColor(int index)
    {
        selectedColorIndex = index;
        PlayerPrefs.SetInt("SelectedColorIndex", index);
        PlayerPrefs.Save();
    }
    public Color GetCurrentDrawColor()
    {
        return presetColors[selectedColorIndex];
    }
}
