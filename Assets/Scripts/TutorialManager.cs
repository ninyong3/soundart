using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject guidePanel;
    [Header("매니저")]
    public RhythmManager rhythmManager;
    private void Start()
    {
        if (RhythmManager.isRetry)
        {
            Debug.Log("재시작입니다! 튜토리얼 스킵!");
            if (guidePanel != null)
            {
                guidePanel.SetActive(false);
            }
            if (rhythmManager != null)
                rhythmManager.GameStart();
        }
        else
        {
            if (guidePanel != null)
            {
                guidePanel.SetActive(true);
            }
        }
    }
    public void OnClickGameStart()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
        if (rhythmManager != null)
        {
            rhythmManager.GameStart();
        }
    }
}
