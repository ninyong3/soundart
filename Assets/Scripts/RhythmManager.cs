using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using NUnit.Framework;
/// <summary>
/// 개인 악보 시스템
/// </summary>
public class RhythmManager : MonoBehaviour
{
    public bool isGameStarted=false;
    public static bool isRetry = false;
    [Header("타이밍")]
    [Tooltip("현재 음악 재생 시간 (초)")]
    public float songPosition;
    [Tooltip("음악 재생 속도 (1.0 = 100%)")]
    public float playbackSpeed = 1.0f;
    [Header("UI 설정")]
    public GameObject clearPanel;
    public GameObject overPanel;
    public GameObject pausePanel;
    private bool isPaused=false;
    [Tooltip("실패 개수를 표시할 텍스트")]
    public TextMeshProUGUI missCountText;
    [Header("저장 시스템 연결")]
    public RecordManager recordManager;
    [Tooltip("현재 스테이지의 고유 ID")]
    public string currentStageID;
    public DrawManager drawManager;
    private int totalEventCount = 0; // 전체 노트 개수
    private int processedEventCount = 0; // 지금까지 처리한 개수
    private int missCount = 0; // 실패 횟수
    private TargetPoint lastProcessedNote = null;
    private TargetPoint[] allTargetPoints;
    private bool isGameEnded = false;
    public AudioSource audioSource;
    private bool isPlaying = false;
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
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && audioSource.isPlaying)
            Debug.Log($"[채보 기록] 노트 시간: {audioSource.time}");
#endif
        if (isGameEnded || isPaused || !isGameStarted || audioSource == null)
        {
            return;
        }
        songPosition = audioSource.time;
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
            if (recordManager != null && drawManager != null)
            {
                System.Collections.Generic.List<LineRenderer> finalLines = drawManager.GetActiveDrawnLines();
                recordManager.SavePlayData(currentStageID, finalLines, missCount);
            }
        }
    }
    public void RetryGame()
    {
        Time.timeScale = 1f;
        isRetry = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void TogglePause()
    {
        isPaused = !isPaused; // 상태 뒤집기
        if(pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
        Time.timeScale = isPaused ? 0f : 1f;
    }
    public void GameStart()
    {
        isGameStarted = true;
        Debug.Log("게임 시작!");
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            isPlaying = true;
        }
    }
}
