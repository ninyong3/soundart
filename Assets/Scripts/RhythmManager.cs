using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
/// <summary>
/// 개인 악보 시스템
/// </summary>
public class RhythmManager : MonoBehaviour
{
    [Header("타이밍")]
    [Tooltip("현재 음악 재생 시간 (초)")]
    public float songPosition;
    [Tooltip("음악 재생 속도 (1.0 = 100%)")]
    public float playbackSpeed = 1.0f;
    [Header("UI 설정")]
    public GameObject clearPanel;
    public GameObject overPanel;
    [Tooltip("실패 개수를 표시할 텍스트")]
    public TextMeshProUGUI missCountText;
    private int totalEventCount = 0; // 전체 노트 개수
    private int processedEventCount = 0; // 지금까지 처리한 개수
    private int missCount = 0; // 실패 횟수
    private TargetPoint lastProcessedNote = null;
    private TargetPoint[] allTargetPoints;
    private bool isGameEnded = false;
    private void Start()
    {
        allTargetPoints = FindObjectsByType<TargetPoint>(FindObjectsSortMode.None);
        totalEventCount = 0;
        foreach (TargetPoint note in allTargetPoints)
        {
            if (note.myEvents != null)
            {
                totalEventCount += note.myEvents.Count;
            }
        }
        Debug.Log($"총 이벤트 개수: {totalEventCount}개");
        ResetAllTargets();
    }
    private void Update()
    {
        if (isGameEnded)
        {
            return;
        }
        songPosition += Time.deltaTime * playbackSpeed; //  차후 노래 생기면 audio.time으로 교체
        foreach (TargetPoint note in allTargetPoints)
        {
            note.UpdateTiming(songPosition);
        }
    }
    ///<summary>
    ///모든 노트 초기화(다시하기용)
    ///</summary>
    public void ResetAllTargets()
    {
        songPosition = 0f; // 시간 초기화
        lastProcessedNote = null;
        processedEventCount = 0;
        missCount = 0;
        isGameEnded = false;
        if(clearPanel != null)
            clearPanel.SetActive(false);
        if(overPanel != null) 
            overPanel.SetActive(false);
        if(missCountText != null)
            missCountText.text="";
        if (allTargetPoints != null)
        {
            foreach (TargetPoint target in allTargetPoints)
            {
                target.ResetTarget();
            }
        }
        if (clearPanel != null)
            clearPanel.SetActive(false);
        if (overPanel != null)
            overPanel.SetActive(false);
    }
    /// <summary>
    /// 노트가 성공했을 때 자신을 마지막 성공 노트로 등록
    /// </summary>
    public void ReportNoteFinished(TargetPoint note, bool isMissed)
    {
        lastProcessedNote = note;
        processedEventCount++;
        if (isMissed)
        {
            missCount++;
        }
        if (processedEventCount >= totalEventCount)
        {
            FinishGame();
        }
    }
    public TargetPoint GetLastProcessedNote()
    {
        return lastProcessedNote;
    }
    private void FinishGame()
    {
        isGameEnded = true;
        Debug.Log($"게임 종료! (실패: {missCount}개)");
        if (missCount > 0)
        {
            Debug.Log(" Game Over...");
            if (overPanel != null)
                overPanel.SetActive(true);
            if(missCountText != null)
            {
                missCountText.text = "Missed: " + missCount;
            }
        }
        else
        {
            Debug.Log(" Game Clear! ");
            if (clearPanel != null)
                clearPanel.SetActive(true);
        }
    }
    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
