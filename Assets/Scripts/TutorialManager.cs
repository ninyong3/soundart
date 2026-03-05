using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject guidePanel;
    [Header("¸Å´ÏÀú")]
    public RhythmManager rhythmManager;
    private void Start()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);
        if(rhythmManager != null)
            rhythmManager.GameStart();
    }
}
