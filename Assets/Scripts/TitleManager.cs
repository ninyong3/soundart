using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class TitleManager : MonoBehaviour
{
    public GameObject optionPanel;
    public void GoToStageSelect()
    {
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
        optionPanel.SetActive(true);
    }
    public void CloseOption()
    {
        optionPanel.SetActive(false);
    }
    public void GoToGallery()
    {
        SceneManager.LoadScene("Gallery");
    }
}

