using System.Runtime.InteropServices;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
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
    [Header("효과음")]
    public AudioClip pauseSound;
    public AudioClip backSound;
    public AudioClip restartSound;
    public AudioClip countSound;
    public AudioClip startSound;
    [Header("유도 점 설정")]
    public GameObject gudieDotPrefab;
    public int dotJumpSteps = 5;
    public float approachTime = 1.0f;
    [Tooltip("최상위 스플라인 컨테이너")]
    public UnityEngine.Splines.SplineContainer mainSplineContainer;
    [Tooltip("노트와 길이 연결됐다고 인정할 거리")]
    public float pathConnectionThreshold = 1.5f;
    private GameObject instantiatedDot;
    private TargetPoint currentGuideNote = null;
    private int currentGuideEventIndex = -1;
    private float dynamicApproachTime = 1f;
    private UnityEngine.Splines.Spline activeSpline = null;
    private float guideStartT = 0f;
    private float guideEndT = 1f;
    [Header("테스트용")]
    public bool isAutoMode=false;
    [Header("튜토리얼 연결")]
    public TutorialManager tutorialManager;
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
        if (gudieDotPrefab != null && instantiatedDot == null)
        {
            instantiatedDot = Instantiate(gudieDotPrefab);
            instantiatedDot.SetActive(false);
        }
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
        if (!audioSource.isPlaying)
        {
            FinishGame();
            return;
        }
        songPosition = audioSource.time;
        foreach (TargetPoint note in allTargetPoints)
        {
            note.UpdateTiming(songPosition);
        }
        UpdateGuideDot();
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
        currentGuideNote = null;
        activeSpline = null;
        if (instantiatedDot != null)
        {
            instantiatedDot.SetActive(false);
        }
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
        SoundManager.Instance.PlaySFX(restartSound, 0f);
        Time.timeScale = 1f;
        isRetry = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GoToStageSelect()
    {
        SoundManager.Instance.PlaySFX(backSound, 0f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("StageSelect");
    }
    public void GoToTitle()
    {
        SoundManager.Instance.PlaySFX(backSound, 0f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("title");
    }
    public void TogglePause()
    {
        SoundManager.Instance.PlaySFX(pauseSound, 0f);
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
            if(SoundManager.Instance != null)
                audioSource.volume = SoundManager.Instance.bgmVolume;
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
            SoundManager.Instance.PlaySFX(countSound, 0f);
            if (countdownText != null)
                countdownText.text = count.ToString() + "!";
            yield return new WaitForSeconds(1f);
            count--;
        }
        SoundManager.Instance.PlaySFX(startSound, 0f);
        if (countdownText != null)
            countdownText.text = "시작!";
        yield return new WaitForSeconds(0.5f);
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
        if(tutorialManager != null)
            tutorialManager.CheckAndStartTutorial();
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
        else if(missRate <= 0.4f)
            return "C";
        else if(missRate <= 0.45f)
            return "D+";
        else if(missRate <= 0.5f)
            return "D";
        return "F";
    }
    private void UpdateGuideDot()
    {
        if(instantiatedDot == null)
            return;
        if(currentGuideNote  == null || currentGuideNote.GetState() == NoteState.Missed || currentGuideNote.GetState() == NoteState.Hit || currentGuideNote.GetCurrentEventIndex() != currentGuideEventIndex)
        {
            currentGuideNote = null;
            activeSpline = null;
            TargetPoint nextNoteCandidate = null;
            float earliestTIme =float.MaxValue;
            foreach (TargetPoint note in allTargetPoints)
            {
                if (note.myEvents.Count > 0 && note.GetState() != NoteState.Hit && note.GetState() != NoteState.Missed)
                {
                    int idx=note.GetCurrentEventIndex();
                    if(idx >= note.myEvents.Count)
                        continue;
                   float noteTime = note.myEvents[idx].activationTime;
                   if(noteTime < earliestTIme)
                   {
                        earliestTIme = noteTime;
                        nextNoteCandidate = note;
                   }
                }
            }
            if (nextNoteCandidate != null)
            {
                currentGuideNote = nextNoteCandidate;
                currentGuideEventIndex = currentGuideNote.GetCurrentEventIndex();
                float targetTime = currentGuideNote.myEvents[currentGuideEventIndex].activationTime;
                dynamicApproachTime = targetTime - songPosition;
                if(dynamicApproachTime < approachTime)
                    dynamicApproachTime = approachTime;
                TargetPoint requiredPrev = currentGuideNote.myEvents[currentGuideEventIndex].requiredPreviousNote;
                FindBestSplinePath(requiredPrev, currentGuideNote);
            }
        }
        if (currentGuideNote != null)
        {
            int currentIdx=currentGuideNote.GetCurrentEventIndex();
            TargetPoint reqPrev=currentGuideNote.myEvents[currentIdx].requiredPreviousNote;
            if (reqPrev != null && activeSpline == null)
            {
                instantiatedDot.SetActive(false);
                return;
            }
            float activationTime = currentGuideNote.myEvents[currentIdx].activationTime;
            float timeRemaining = activationTime - songPosition;
            float progress = 1f - (timeRemaining / dynamicApproachTime);
            if (progress < 0f || progress > 1.2f)
            {
                instantiatedDot.SetActive(false);
            }
            else
            {
                float clampedProgress = Mathf.Clamp01(progress);
                int currentStep = Mathf.FloorToInt(clampedProgress* dotJumpSteps);
                Vector3 targetPos;
                if (reqPrev == null)
                    targetPos = currentGuideNote.transform.position;
                else if (currentStep <= 0)
                    targetPos = reqPrev.transform.position;
                else if (currentStep >= dotJumpSteps)
                    targetPos = currentGuideNote.transform.position;
                else
                {
                    float t = (float)currentStep / dotJumpSteps;
                    if (activeSpline == null)
                    {
                        targetPos = Vector3.Lerp(reqPrev.transform.position, currentGuideNote.transform.position, t);
                    }
                    else
                    {
                        float currentT = Mathf.Lerp(guideStartT, guideEndT, t);
                        currentT = currentT % 1f;
                        if (currentT < 0f)
                            currentT += 1f;
                        float3 localPos = SplineUtility.EvaluatePosition(activeSpline, currentT);
                        targetPos = mainSplineContainer.transform.TransformPoint(localPos);
                    }
                }
                float popScale = 1f + Mathf.Abs(Mathf.Sin(progress * Mathf.PI * dotJumpSteps)) * 0.5f;
                instantiatedDot.transform.localScale= Vector3.one*popScale;
                instantiatedDot.transform.position = targetPos;
                instantiatedDot.SetActive(true);
            }
        }
        else
        {
            instantiatedDot.SetActive(false);
        }
    }
    private void FindBestSplinePath(TargetPoint startNote, TargetPoint endNote)
    {
        activeSpline = null;
        if (mainSplineContainer == null && endNote == null)
        {
            return;
        }
        Vector3 endWorldPos = endNote.transform.position;
        Vector3 endLocal = mainSplineContainer.transform.InverseTransformPoint(endWorldPos);
        float bestScore = float.MaxValue;
        foreach (Spline spline in mainSplineContainer.Splines)
        {
            if (spline == null || spline.Count < 2) continue;
            try
            {
                UnityEngine.Splines.SplineUtility.GetNearestPoint(spline, endLocal, out float3 nearestEnd, out float endT);
                float distToEnd = math.distance(endLocal, nearestEnd);
                float score = distToEnd;
                float startT = endT;
                if (startNote != null)
                {
                    Vector3 startWorldPos = startNote.transform.position;
                    float3 startLocal = mainSplineContainer.transform.InverseTransformPoint(startWorldPos);
                    SplineUtility.GetNearestPoint(spline, startLocal, out float3 nearestStart, out startT);
                    float distToStart = math.distance(startLocal, nearestStart);
                    score = distToStart + distToEnd;
                }
                if (score < bestScore)
                {
                    bestScore = score;
                    activeSpline = spline;
                    guideStartT = startT;
                    guideEndT = endT;
                }
            }
            catch(System.Exception e)
            {
                Debug.LogWarning($"스플라인 길 찾다가 에러 발생!: {e.Message}");
                continue;
            }
        }
        if(activeSpline != null && activeSpline.Closed)
        {
            if (guideStartT - guideEndT > 0.5f)
                guideEndT += 1f;
            else if(guideEndT - guideStartT > 0.5f)
                guideStartT += 1f;
        }
    }
}
