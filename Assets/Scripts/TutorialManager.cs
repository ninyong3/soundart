using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        [Header("[{i}] 단계 설정")]
        [TextArea(3, 5)]
        public string instructionText;
        public RectTransform targetTouchArea;
        public bool blockUntilAction = true;
    }
    [Header("연결할 것들")]
    public GameObject tutorialCanvas;
    public GameObject fullScreenBlocker;
    public TextMeshProUGUI instructionTMPro;
    public RhythmManager rhythmManager;
    [Header("단계별 데이터 설정")]
    public TutorialStep[] tutorialSteps;
    private int currentStepIndex = -1;
    private bool isActionCompleted=false;
    [Header("테스트 설정")]
    public bool isTestMode = true;
    public void CheckAndStartTutorial()
    {
        Debug.Log(" 튜토리얼 매니저 호출됨! 테스트 모드: " + isTestMode);
        if (!isTestMode)
        {
            int isTutorialDone = PlayerPrefs.GetInt("TutorialDone_Stage1", 0);
            if (isTutorialDone == 1)
            {
                tutorialCanvas.SetActive(false);
                Destroy(this.gameObject);
                return;
            }
        }
        StartCoroutine(StartTutorialRoutine());
    }
    private IEnumerator StartTutorialRoutine()
    {
        tutorialCanvas.SetActive(true);
        PauseGame(true);
        for (int i = 0; i < tutorialSteps.Length; i++)
        {
            currentStepIndex = i;
            TutorialStep step = tutorialSteps[i];
            instructionTMPro.text = step.instructionText;
            if (step.targetTouchArea != null)
            {
                step.targetTouchArea.SetParent(fullScreenBlocker.transform, true);
                step.targetTouchArea.SetAsLastSibling();
                SetRaycastable(step.targetTouchArea.gameObject, true);
            }
            if (step.blockUntilAction)
            {
                yield return new WaitUntil(() => isActionCompleted);
                isActionCompleted = false;
            }
            else
            {
                yield return new WaitForSecondsRealtime(2f);
            }
            if (step.targetTouchArea != null)
            {
                step.targetTouchArea.SetParent(tutorialCanvas.transform, true);
                SetRaycastable(step.targetTouchArea.gameObject, false);
            }
        }
        FinishTutorial();
    }
    public void CompleteCurrentStepAction()
    {
        isActionCompleted = true;
        Debug.Log($"[{currentStepIndex}] 단계 행동 완료!");
    }
    private void PauseGame(bool isPause)
    {
        Time.timeScale=isPause ? 0f : 1f;
        if(rhythmManager.audioSource != null)
        {
            if (isPause)
                rhythmManager.audioSource.Pause();
            else
                rhythmManager.audioSource.UnPause();
        }
    }
    private void SetRaycastable(GameObject obj, bool isRaycastable)
    {
        Image img = obj.GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = isRaycastable;
        }
    }
    private void FinishTutorial()
    {
        PauseGame(false);
        tutorialCanvas.SetActive(false);
        if (!isTestMode)
        {
            PlayerPrefs.SetInt("TutorialDone_Stage1", 1);
            PlayerPrefs.Save();
        }
        Destroy(this.gameObject);
    }
}
