using UnityEngine;
using UnityEngine.Splines;
/// <summary>
/// 몇 초에 어떤 노트를 칠건지 이벤트 하나 정의
/// </summary>
[System.Serializable]
public class RhythmEvent
{
    [Tooltip("활성화될 시간(초)")]
    public float activationTime;
    [Tooltip("치기 직전에 쳤어야 하는 노트")]
    public TargetPoint requiredPreviousNote;
    [Tooltip("성공 시키기 위해 반드시 그려야 하는 길")]
    public SplineContainer requiredSpline;
}
