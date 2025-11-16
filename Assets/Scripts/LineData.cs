using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
/// <summary>
/// 선 1개의 모든 좌표 데이터 저장
/// </summary>
public class LineData : MonoBehaviour
{
    public List<Vector3> positions=new List<Vector3>();
    [HideInInspector]
    public SplineContainer drawnOnSpline; // 선의 출생지
}
