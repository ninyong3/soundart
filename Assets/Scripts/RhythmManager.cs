using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using NUnit.Framework;
using System.Collections;
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
    public GameObject pausePanel;
    private bool isPaused=false;
    [Tooltip("실패 개수를 표시할 텍스트")]
    public TextMeshProUGUI missCountText;
    [Tooltip("등급을 표시할 텍스트")]
    public TextMeshProUGUI gradeText;
    [Header("저장 시스템 연결")]
    public RecordManager recordManager;
    public DrawManager drawManager;
    [Header("카운트다운 UI")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;
    [Header("스테이지 정보")]
    public StageData stageData;
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
        isGameStarted = false;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
        }
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
        StartCoroutine(StartCountdownRoutine());
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
        if(ScoreManager.Instance !=  null)
            ScoreManager.Instance.ResetScoreManager();
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
        int finalScore = 0;
        if (ScoreManager.Instance != null)
            finalScore = ScoreManager.Instance.GetScore();
        string finalGrade = CalculateGrade();
        if (stageData != null)
        {
            int previousBest = 0;
            int.TryParse(stageData.bestScore, out previousBest);
            if (finalScore > previousBest)
            {
                stageData.bestScore = finalScore.ToString();
                stageData.rank = finalGrade;
                PlayerPrefs.SetInt(stageData.stageID + "_BestScore", finalScore);
                PlayerPrefs.SetString(stageData.stageID + "_BestGrade", finalGrade);
                PlayerPrefs.Save();
            }
        }
        Debug.Log($"게임 종료! (실패: {missCount}개)");
        gradeText.text = "등급: " + finalGrade;
        missCountText.text = "놓친 개수: " + missCount;
        if (clearPanel != null)
            clearPanel.SetActive(true);
        if (recordManager != null && drawManager != null)
        {
            System.Collections.Generic.List<LineRenderer> finalLines = drawManager.GetActiveDrawnLines();
            recordManager.SavePlayData(stageData.stageID, finalLines, missCount);
        }
        if(audioSource != null)
            audioSource.Stop();
    }
    public void RetryGame()
    {
        Time.timeScale = 1f;
        isRetry = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GoToStageSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StageSelect");
    }
    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("title");
    }
    public void TogglePause()
    {
        isPaused = !isPaused; // 상태 뒤집기
        if(pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
        Time.timeScale = isPaused ? 0f : 1f;
        if (isPaused)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }
    public void GameStart()
    {
        isGameStarted = true;
        Debug.Log("게임 시작!");
        if (audioSource != null && audioSource.clip != null)
        {
            if(GameManager.Instance != null)
                audioSource.volume = GameManager.Instance.bgmVolume;
            audioSource.Play();
            isPlaying = true;
        }
    }
    private System.Collections.IEnumerator StartCountdownRoutine()
    {
        if (countdownPanel != null)
            countdownPanel.SetActive(true);
        int count = 3;
        while (count > 0)
        {
            if (countdownText != null)
                countdownText.text = count.ToString() + "!";
            yield return new WaitForSeconds(1f);
            count--;
        }
        if (countdownText != null)
            countdownText.text = "시작!";
        yield return new WaitForSeconds(0.5f);
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
        GameStart();
    }
    private string CalculateGrade()
    {
        if (totalEventCount == 0)
            return "F";
        if (missCount == 0)
            return "S";
        float missRate=(float)missCount/totalEventCount;
        if (missRate <= 0.02f)
            return "A+";
        else if (missRate <= 0.05f)
            return "A";
        else if (missRate <= 0.1f)
            return "B+";
        else if (missRate <= 0.2f)
            return "B";
        else if(missRate <= 0.35f)
            return "C+";
        else if(missRate <= 0.5f)
            return "C";
        return "F";
    }
}
