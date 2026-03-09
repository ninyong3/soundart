using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SettingUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public TMP_InputField bgmInput;
    public TMP_InputField sfxInput;
    public GameObject colorPickerWindow;
    [Header("컬러 프리셋 연결")]
    public Image[] presetImages;
    [Header("선택한 색 표시")]
    public RectTransform pickTextRect;
    private void Awake()
    {
        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 100f;
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 100f;
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        bgmInput.onValueChanged.AddListener(OnBGMInputChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        sfxInput.onValueChanged.AddListener(OnSFXInputChanged);
    }
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            bgmSlider.value = SoundManager.Instance.bgmVolume * 100f;
            bgmInput.text = Mathf.RoundToInt(bgmSlider.value).ToString();
            sfxSlider.value = SoundManager.Instance.sfxVolume * 100f;
            sfxInput.text = Mathf.RoundToInt(sfxSlider.value).ToString();
        }
        if(colorPickerWindow != null)
            colorPickerWindow.SetActive(false);
        if (GameManager.Instance != null && presetImages != null)
        {
            for (int i = 0; i < presetImages.Length; i++)
            {
                presetImages[i].color = GameManager.Instance.presetColors[i];
            }
        }
        if (GameManager.Instance != null && presetImages.Length > 0)
        {
            MovePickText(GameManager.Instance.selectedColorIndex);
        }
    }
    //슬라이더 드래그 시
    public void OnBGMSliderChanged(float value)
    {
        bgmInput.text=Mathf.RoundToInt(value).ToString();
        SoundManager.Instance.SetBGMVolume(value / 100f);
    }
    public void OnBGMInputChanged(string text)
    {
        if(float.TryParse(text, out float result))
        {
            result = Mathf.Clamp(result, 0f, 100f); // 숫자가 0~100으로만 유지되게
            bgmInput.text=result.ToString();
            bgmSlider.value=result;
            SoundManager.Instance.SetBGMVolume(result/100f);
        }
    }
    public void OnSFXSliderChanged(float value)
    {
        sfxInput.text = Mathf.RoundToInt(value).ToString();
        SoundManager.Instance.SetSFXVolume(value / 100f);
    }
    public void OnSFXInputChanged(string text)
    {
        if (float.TryParse(text, out float result))
        {
            result = Mathf.Clamp(result, 0f, 100f); // 숫자가 0~100으로만 유지되게
            sfxInput.text = result.ToString();
            sfxSlider.value = result;
            SoundManager.Instance.SetSFXVolume(result / 100f);
        }
    }
    public void MovePickText(int index)
    {
        GameManager.Instance.SetSelectedColor(index);
        pickTextRect.position = presetImages[index].rectTransform.position;
        pickTextRect.localPosition += new Vector3(0f, -80f, 0f);
    }
}
