using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
    [Header("타이밍 설정")]
    [Tooltip("노트가 활성화되는 정확한 시간 (초)")]
    public float activationTime = 3.0f;
    [Tooltip("판정 유효 시간(반만큼 앞 뒤 시간)")]
    public float timeWindow = 0.5f;
    [Header("판정 설정")]
    [Tooltip("이전 노트에서 값만큼 가까이에서 시작해야 인정")]
    public float startThreshold = 0.5f;
    [Header("시각화 설정")]
    [Tooltip("줄어드는 원 오브젝트(타이밍 알려주는 용도)")]
    public Transform approachCircle;
    [Tooltip("원이 나타나기 시작하는 시간(초)")]
    public float approachTime = 1.0f; // 1초 전부터 줄어듦
    [Tooltip("원의 시작 크기 배율")]
    public float startScale = 2.0f;
    private NoteState currentState=NoteState.Idle;
    private SpriteRenderer spriteRenderer;
    private RhythmManager rhythmManager;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.gray*0.5f; // 시작 시 대기 중 색깔로 지정
    }
    private void Start()
    {
        rhythmManager=FindFirstObjectByType<RhythmManager>();  
    }
    /// <summary>
    /// RhythmManager에서 현재 시간 받아 상태 갱신
    /// </summary>
    public void UpdateTiming(float currentSongPosition, bool isMyTurn)
    {
        if (currentState == NoteState.Hit || currentState == NoteState.Missed)
        {
            if (approachCircle != null)
            {
                approachCircle.gameObject.SetActive(false);
            }
            return;
        }
        if (!isMyTurn)
        {
            currentState = NoteState.Idle;
            spriteRenderer.color = Color.gray * 0.5f;
            if (approachCircle != null)
            {
                approachCircle.gameObject.SetActive(false);
            }
            return;
        }
        if (approachCircle != null)
        {
            float timeRemaining = activationTime - currentSongPosition;
            float progress = 1f - (timeRemaining / approachTime); // 0부터 1까지, 1이 쳐야하는 시점
            if (progress < 0f)
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
        float windowStart = activationTime - (timeWindow / 2); // 유효 시간 계산
        float windowEnd = activationTime + (timeWindow / 2);
        if (currentSongPosition >= windowStart && currentSongPosition <= windowEnd)
        {
            if (currentState == NoteState.Idle)
            {
                currentState = NoteState.Active; // 활성화
                spriteRenderer.color = Color.white;
            }
        }
        else if (currentSongPosition > windowEnd)
        {
            if (currentState == NoteState.Active || currentState == NoteState.Idle)
            {
                currentState = NoteState.Missed; // 지나침(실패)
                spriteRenderer.color = Color.red;
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
        TargetPoint lastHitNote = rhythmManager.GetLastHitNote();
        if (lastHitNote == null)
        {
            SetHit(thisLine);
        }
        else
        {
            Vector3 lineStartPosition = thisLine.positions[0];
            Vector3 lastNotePosition = lastHitNote.transform.position;
            float distance = Vector3.Distance(lineStartPosition, lastNotePosition);
            if (distance <= startThreshold)
            {
                SetHit(thisLine);
            }
            else
            {
                Debug.LogWarning("선이 이전 노트에서 시작하지 않았어요! (무시)");
            }
        }
    }
    private void SetHit(LineData line)
    {
        Debug.Log("타겟 포인트 적중! (시간: " + activationTime + ")");
        currentState = NoteState.Hit;
        spriteRenderer.color = Color.green;
    }
    public void ResetTarget()
    {
        currentState = NoteState.Idle;
        spriteRenderer.color = Color.gray * 0.5f;
        if (approachCircle != null)
        {
            approachCircle.gameObject.SetActive(false);
            approachCircle.localScale = Vector3.one * startScale;
        }
    }
    public void AdvanceLoop(float loopDuration)
    {
        this.activationTime += loopDuration;
        ResetTarget();
    }
}
