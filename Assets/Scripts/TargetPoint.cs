using UnityEngine;
using System.Collections.Generic;
public enum NoteState
{
    Idle, // 대기중(비활성화)
    Active, // 활성화
    Hit, // 선이 닿음(성공)
    Missed // 유효 시간이 아님(실패) 
}
/// <summary>
/// 노트 역할 타겟 포인트
/// </summary>
public class TargetPoint : MonoBehaviour
{
    [Header("개인 악보")]
    [Tooltip("이 노트가 활성화 될 모든 시간과 조건 목록")]
    public List<RhythmEvent> myEvents;
    [Header("판정 설정")]
    public float timeWindow = 0.5f;
    [Tooltip("이전 노트에서 값만큼 가까이에서 시작해야 인정")]
    public float startThreshold = 0.6f;
    [Header("시각화 설정")]
    [Tooltip("줄어드는 원 오브젝트(타이밍 알려주는 용도)")]
    public Transform approachCircle;
    [Tooltip("원이 나타나기 시작하는 시간(초)")]
    public float approachTime = 1.0f; // 1초 전부터 줄어듦
    [Tooltip("원의 시작 크기 배율")]
    public float startScale = 2.0f;
    [Tooltip("이번 차례에 카메라 이동을 이미 명령했는지 확인하는 플래그")]
    private bool hasMovedCameraForThisEvent=false;
    private NoteState currentState=NoteState.Idle;
    private SpriteRenderer spriteRenderer;
    private RhythmManager rhythmManager;
    private int currentEventIndex = 0;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        rhythmManager=FindFirstObjectByType<RhythmManager>();
        ResetTarget();
    }
    /// <summary>
    /// RhythmManager에서 현재 시간 받아 개인 악보 보고 상태 갱신
    /// </summary>
    public void UpdateTiming(float currentSongPosition)
    {
        if (currentEventIndex >= myEvents.Count)
        {
            if (approachCircle != null)
                approachCircle.gameObject.SetActive(false);
            return;
        }
        RhythmEvent currentEvent = myEvents[currentEventIndex];
        float myActivationTime = currentEvent.activationTime;
        if (approachCircle != null)
        {
            float timeRemaining = myActivationTime - currentSongPosition;
            float progress = 1f - (timeRemaining / approachTime); // 0부터 1까지, 1이 쳐야하는 시점
            if (currentState == NoteState.Idle || currentState == NoteState.Active)
            {
                if (progress < 0f || progress > 1.1f) // 너무 멀면 숨긴다
                {
                    approachCircle.gameObject.SetActive(false);
                }
                else
                {
                    approachCircle.gameObject.SetActive(true);
                    float currentScale = Mathf.Lerp(startScale, 1.0f, progress);
                    approachCircle.localScale = Vector3.one * currentScale;
                }
            }
            else
            {
                approachCircle.gameObject.SetActive(false);
            }
        }
        TargetPoint lastNote=rhythmManager.GetLastProcessedNote();
        TargetPoint requiredNote = currentEvent.requiredPreviousNote;
        bool isMyTurn= (requiredNote == null) || (requiredNote == lastNote);
        if (!isMyTurn)
        {
            if (currentState == NoteState.Active)
            {
                currentState = NoteState.Idle;
                spriteRenderer.color = Color.gray * 0.5f;
            }
            return;
        }
        float windowStart = myActivationTime - (timeWindow / 2); // 유효 시간 계산
        float windowEnd = myActivationTime + (timeWindow / 2);
        if (currentSongPosition >= windowStart && currentSongPosition <= windowEnd)
        {
            if (currentState == NoteState.Idle)
            {
                currentState = NoteState.Active; // 활성화
                spriteRenderer.color = Color.white;
                TriggerCameraMoveOnTurn();
            }
        }
        else if (currentSongPosition > windowEnd)
        {
            if (currentState == NoteState.Active || currentState == NoteState.Idle)
            {
                Debug.Log("Missed! (시간 초과)");
                currentState = NoteState.Missed; // 지나침(실패)
                spriteRenderer.color = Color.red;
                ScoreManager.Instance.ResetCombo();
                rhythmManager.ReportNoteFinished(this, true);
                currentEventIndex++;
                hasMovedCameraForThisEvent = false;
            }
        }
    }
    /// <summary>
    /// 현재 노트의 상태를 RhythmManager에게 보고
    /// </summary>
    public NoteState GetState()
    {
        return currentState;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState != NoteState.Active | !other.CompareTag("DrawingLine"))
        {
            return;
        }
        LineData thisLine = other.GetComponent<LineData>(); // 지금 날 친 선
        RhythmEvent currentEvent=myEvents[currentEventIndex];
        TargetPoint requiredNote = currentEvent.requiredPreviousNote;
        TargetPoint lastNote = rhythmManager.GetLastProcessedNote();
        if (requiredNote != lastNote && requiredNote != null)
        {
            Debug.LogWarning("[판정 실패] 순서 불일치!");
            return;
        }
      /*  if (currentEvent.requiredSpline != null && thisLine.drawnOnSpline != currentEvent.requiredSpline)
        {
            Debug.LogWarning($"[판정 실패] '길' 불일치! (필요: {currentEvent.requiredSpline.name}, 실제: {thisLine.drawnOnSpline?.name})");
            return;
        }*/
        if (requiredNote == null)
        {
            SetHit();
        }
        else
        {
            Vector3 lineStartPosition = thisLine.positions[0];
            Vector3 lastNotePosition = requiredNote.transform.position;
            float distance = Vector3.Distance(lineStartPosition, lastNotePosition);
            if (distance <= startThreshold)
            {
                SetHit();
            }
            else
            {
                 Debug.LogWarning("선이 이전 노트에서 시작하지 않았어요! (무시)");
            }
        }
    }
    private void SetHit()
    {
        Debug.Log("타겟 포인트 적중!");
        currentState = NoteState.Hit;
        float currentSongPosition = rhythmManager.songPosition;
        float myActivationTime = myEvents[currentEventIndex].activationTime;
        float timeDifference = Mathf.Abs(myActivationTime - currentSongPosition);
        if (timeDifference <= 0.1f)
            ScoreManager.Instance.AddHit(ScoreManager.HitAccuracy.Perfect);
        else
            ScoreManager.Instance.AddHit(ScoreManager.HitAccuracy.Good);
        spriteRenderer.color = Color.green;
        rhythmManager.ReportNoteFinished(this, false);
        currentEventIndex++;
        hasMovedCameraForThisEvent = false;
    }
    public void ResetTarget()
    {
        currentEventIndex = 0;
        currentState = NoteState.Idle;
        spriteRenderer.color = Color.gray * 0.5f;
        if (approachCircle != null)
        {
            approachCircle.gameObject.SetActive(false);
        }
    }
    private void TriggerCameraMoveOnTurn()
    {
        if (hasMovedCameraForThisEvent)
            return;
        RhythmEvent currentEvent = myEvents[currentEventIndex];
        if (currentEvent.triggerCameraMove)
        {
            CameraController.Instance.MoveToView(currentEvent.moveCameraToIndex);
        }
        hasMovedCameraForThisEvent = true;
    }
}
