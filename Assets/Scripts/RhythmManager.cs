using UnityEngine;
/// <summary>
/// 게임의 현재 시간(음악 재생 시간) 관리 및 포인트들에게 전달
/// </summary>
public class RhythmManager : MonoBehaviour
{
    [Header("타이밍")]
    [Tooltip("현재 음악 재생 시간 (초)")]
    public float songPosition;
    [Tooltip("음악 재생 속도 (1.0 = 100%)")]
    public float playbackSpeed = 1.0f;
    [Header("참조")]
    [Tooltip("모든 포인트(노트)")]
    private TargetPoint[] allTargetPoints;
    private void Start()
    {
        allTargetPoints = FindObjectsByType<TargetPoint>(FindObjectsSortMode.None);
        Debug.Log("RhythmManager: TargetPoint " + allTargetPoints.Length + "개를 찾았습니다.");
    }
    private void Update()
    {
        songPosition += Time.deltaTime * playbackSpeed; //  차후 노래 생기면 audio.time으로 교체
        foreach (TargetPoint target in allTargetPoints)
        {
            target.UpdateTiming(songPosition);
        }
    }
    ///<summary>
    ///모든 노트 초기화(다시하기용)
    ///</summary>
    public void ResetAllTargets()
    {
        songPosition = 0f; // 시간 초기화
        foreach(TargetPoint target in allTargetPoints)
        {
            target.ResetTarget();
        }
    }
}
