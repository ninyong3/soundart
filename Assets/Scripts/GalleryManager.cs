using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;
using System.Data;
public class GalleryManager : MonoBehaviour
{
    [Header("구성 요소")]
    public Transform contentPanel;
    public GameObject galleryItemPrefab;
    [Header("사운드")]
    public AudioClip galleryBGM;
    public AudioClip gallerySelectSound;
    public AudioClip backToTitleSound;
    private void Start()
    {
        LoadGalleryItems();
        SoundManager.Instance.PlayBGM(galleryBGM, 0f);
    }
    private void LoadGalleryItems()
    {
        string path = Application.persistentDataPath;
        string[] imageFiles=Directory.GetFiles(path, "*.png"); // 위치 안 png 형식 전부 가져오기
        foreach (string imagePath in imageFiles)
        {
            byte[] bytes = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            GameObject newFrame = Instantiate(galleryItemPrefab, contentPanel);
            newFrame.transform.localScale=Vector3.one;
            newFrame.transform.localPosition = new Vector3(newFrame.transform.localPosition.x, newFrame.transform.localPosition.y, 0f);
            Image frameImage = newFrame.GetComponent<Image>();
            if (frameImage != null)
            {
                frameImage.sprite = sprite;
            }
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string[] nameParts =fileName.Split('_');
            string stageID = "Unknown";
            string dateString = "";
            if(nameParts.Length >= 3)
            {
                stageID=nameParts[0];
                string rawTime = nameParts[1]+"_"+nameParts[2];
                if(DateTime.TryParseExact(rawTime, "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    dateString = parsedDate.ToString("yyyy. MM. dd");
                }
            }
            string songTitle = GetSongTitle(stageID);
            TextMeshProUGUI frameText = newFrame.GetComponentInChildren<TextMeshProUGUI>();
            if(frameText != null)
            {
                frameText.text = $"{songTitle}\n{dateString}";
            }
            Button button = newFrame.GetComponent<Button>();
            if (button != null)
            {
                string jsonPath = imagePath.Replace(".png", ".json");
                button.onClick.AddListener(() => OnFrameClicked(jsonPath, stageID));
            }
        }
    }
    private void OnFrameClicked(string jsonPath, string stageID)
    {
        Debug.Log("액자 클릭, 열어볼 데이터: "+jsonPath);
        GalleryDataKeeper.targetJsonPath=jsonPath;
        GalleryDataKeeper.targetStageID = stageID;
        SoundManager.Instance.PlaySFX(gallerySelectSound, 0f);
        SceneManager.LoadScene("GalleryDetail");
    }
    private string GetSongTitle(string stageID)
    {
        switch(stageID)
        {
            case "test": return "Pumped";
            case "Stage1": return "사람";
            default: return "무제";
        }
    }

    public void GoToTitle()
    {
        SoundManager.Instance.PlaySFX(backToTitleSound, 0f);
        SceneManager.LoadScene("title");
    }
}
