using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
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
