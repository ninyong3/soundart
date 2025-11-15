using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    [Header("루프 설정")]
    [Tooltip("체크하면, 마지막 노트가 첫 노트로 이어짐")]
    public bool isLooping = true;
    [Header("참조")]
    [Tooltip("모든 포인트(노트)")]
    private TargetPoint[] allTargetPoints;
    private int currentNoteIndex = 0; // 현재 활성화되어야 할 노트(순서)
    private TargetPoint lastHitNote = null;
    private float songLoopDuration;
    private void Start()
    {
        TargetPoint[] foundTargets = FindObjectsByType<TargetPoint>(FindObjectsSortMode.None);
        allTargetPoints = foundTargets.OrderBy(target => target.activationTime).ToArray(); // 노트 순서대로 정렬
        Debug.Log("RhythmManager: TargetPoint " + allTargetPoints.Length + "개를 찾았습니다.");
        if(allTargetPoints.Length > 0)
        {
            TargetPoint lastNote = allTargetPoints[allTargetPoints.Length - 1];
            songLoopDuration = lastNote.activationTime + (lastNote.timeWindow / 2f); // 마지막 노트 0.25초 시점
        }
        else
        {
            songLoopDuration = 10f;
        }
        ResetAllTargets();
    }
    private void Update()
    {
        songPosition += Time.deltaTime * playbackSpeed; //  차후 노래 생기면 audio.time으로 교체
        if (!this.enabled)
            return;
        for (int i = 0; i < allTargetPoints.Length; i++)
        {
            bool isMyTurn = (i == currentNoteIndex); //현재 노트가 i와 같을 때만 true
            allTargetPoints[i].UpdateTiming(songPosition, isMyTurn);
        }
        if (currentNoteIndex >= allTargetPoints.Length)
            return;
        TargetPoint currentNote = allTargetPoints[currentNoteIndex]; // 현재 차례인 노트 상태 확인
        NoteState state=currentNote.GetState();
        if (state == NoteState.Hit || state == NoteState.Missed)
        { 
            if(state == NoteState.Hit)
            {
                lastHitNote = currentNote;
            }
            else
            {
                lastHitNote = null;
            }
            currentNoteIndex++;
            if(isLooping && currentNoteIndex >= allTargetPoints.Length)
            {
                currentNoteIndex = 0;
                foreach(TargetPoint note in allTargetPoints)
                {
                    note.AdvanceLoop(songLoopDuration);
                }
            }
            else if(!isLooping && currentNoteIndex == allTargetPoints.Length)
            {
                Debug.Log("게임 클리어");
                this.enabled = false;
            }
        }
    }
    ///<summary>
    ///모든 노트 초기화(다시하기용)
    ///</summary>
    public void ResetAllTargets()
    {
        songPosition = 0f; // 시간 초기화
        currentNoteIndex = 0;
        foreach(TargetPoint target in allTargetPoints)
        {
            target.ResetTarget();
        }
        if (isLooping && allTargetPoints != null && allTargetPoints.Length > 0)
        {
            lastHitNote = allTargetPoints[allTargetPoints.Length - 1];
        }
        else
        {
            lastHitNote = null; 
        }
    }
    public TargetPoint GetLastHitNote()
    {
        return lastHitNote;
    }
}
