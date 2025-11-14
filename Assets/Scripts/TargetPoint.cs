using UnityEngine;
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
    private enum NoteState
    {
        Idle, // 대기중(비활성화)
        Active, // 활성화
        Hit, // 선이 닿음(성공)
        Missed // 유효 시간이 아님(실패) 
    }
    private NoteState currentState=NoteState.Idle;
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.gray; // 시작 시 대기 중 색깔로 지정
    }
    /// <summary>
    /// RhythmManager에서 현재 시간 받아 상태 갱신
    /// </summary>
    public void UpdateTiming(float currentSongPosition)
    {
        if (currentState == NoteState.Hit || currentState == NoteState.Missed)
        {
            return;
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(currentState != NoteState.Active | !other.CompareTag("DrawingLine"))
        {
            return;
        }
        Debug.Log("타겟 포인트 적중! (시간: " + activationTime + ")");
        currentState = NoteState.Hit;
        spriteRenderer.color = Color.green;
    }
    public void ResetTarget()
    {
        currentState = NoteState.Idle;
        spriteRenderer.color = Color.gray;
    }
}
