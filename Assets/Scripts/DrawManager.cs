using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine.EventSystems;
public class DrawManager : MonoBehaviour
{
    [Header("에셋")]
    public GameObject linePrefab;
    [Header("오브젝트 풀링")]
    [Tooltip("미리 생성해 둘 선의 개수")]
    [SerializeField] private int initialPoolSize = 20;
    private List<GameObject> linePool;
    [Header("그리기 설정")]
    [Tooltip("점을 확정하기 위한 최소 이동 거리")]
    [SerializeField] private float minDistance = 0.1f;
    [Header("최적화 설정")]
    [Tooltip("그리기 끝난 선을 단순화하는 허용 오차(작을수록 원본에 가까움)")]
    [SerializeField] private float simplificationTolerance = 0.02f;
    [Header("영역 설정")]
    [Tooltip("따라 그려야 할 모든 스플라인 길 목록")]
    public List<SplineContainer> allTrackPaths;
    [Tooltip("경로에서 벗어나도 되는 최대 허용 거리")]
    public float maxTraceDistance = 0.4f;
    private bool isDrawing=false;
    [Header("런타임 중 참조")]
    private Camera mainCamera;
    private LineRenderer currentLine;
    private PlayerControls inputActions;
    private LineData currentLineData;
    private Vector3 lastCommittedPosition;
    private GameObject currentLineObject;
    private EdgeCollider2D currentEdgeCollider;
    private void Awake()
    {
        mainCamera=Camera.main;
        inputActions = new PlayerControls();
    }
    private void Start()
    {
        InitializePool();
        if(allTrackPaths == null || allTrackPaths.Count == 0)
        {
            Debug.LogWarning("!!! DrawManager에 trackPath 스플라인이 연결되지 않았어요!!!");
        }
    }
    private void OnEnable()
    {
        inputActions.Drawing.Enable();
    }
    private void OnDisable()
    {
        inputActions.Drawing.Disable();
    }
    private void Update()
    {
        if (Time.timeScale == 0f)
            return;
        if(inputActions.Drawing.PrimaryContact.WasPressedThisFrame())
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            Vector2 screenPosition = inputActions.Drawing.PointerPosition.ReadValue<Vector2>();
            Vector3 startPosition = GetWorldPosition(screenPosition);
            if (!IsPositionInDrawingZone(startPosition, out SplineContainer _))
            {
                return;
            }
            CreateNewLine(startPosition);
            isDrawing = true;
        }
        if(inputActions.Drawing.PrimaryContact.WasReleasedThisFrame())
        {
            if (!isDrawing)
                return;
            EndCurrentLine();
            return;
        }
        if (!isDrawing)
            return;
        Vector2 pointerPos = inputActions.Drawing.PointerPosition.ReadValue<Vector2>();
        Vector3 currentWorldPos = GetWorldPosition(pointerPos);
        if(!IsPositionInDrawingZone(currentWorldPos, out SplineContainer _)) // splineContainer는 버리기
        {
            EndCurrentLine();
            return;
        }
        UpdateLineVisuals(currentWorldPos);
        float distance = Vector3.Distance(currentWorldPos, lastCommittedPosition);
        if(distance < minDistance)
        {
            CommitDataPoint(currentWorldPos);
        }
    }
    ///<summary>
    /// 스크린 좌표를 유니티 월드 좌표로 변환
    ///</summary>
    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f)); // z값은 카메라로부터 10f 떨어진 지점, 즉 0f
        worldPosition.z = 0f; // 오차 방지 
        return worldPosition;
    }
    private void CreateNewLine(Vector3 startPositon)
    {
        IsPositionInDrawingZone(startPositon, out SplineContainer startedSpline);
        currentLineObject=GetLineFromPool();
        currentLine = currentLineObject.GetComponent<LineRenderer>();
        currentLineData = currentLineObject.GetComponent<LineData>();
        currentLineData.drawnOnSpline = startedSpline;
        currentEdgeCollider = currentLineObject.GetComponent<EdgeCollider2D>();
        currentEdgeCollider.Reset();
        currentLine.positionCount = 2;
        currentLine.SetPosition(0, startPositon); // 두 점을 같은 위치에 찍어 점처럼 보이게 함(실제로는 선)
        currentLine.SetPosition(1, startPositon);
        currentLineData.positions.Clear();
        currentLineData.positions.Add(startPositon);
        lastCommittedPosition = startPositon;
    }
    private void UpdateLineVisuals(Vector3 newPosition)
    {
        currentLine.positionCount++;
        currentLine.SetPosition(currentLine.positionCount-1, newPosition);
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < currentLine.positionCount; i++)
        {
            points.Add(currentLine.GetPosition(i));
        }
        currentEdgeCollider.SetPoints(points);
    }
    ///<summary>
    ///데이터 인식에 사용할 LineData리스트에 새 확정 점 추가
    ///</summary>
    private void CommitDataPoint(Vector3 newPosition)
    {
        currentLineData.positions.Add(newPosition);
        lastCommittedPosition = newPosition;
    }
    /// <summary>
    /// 미리 선을 생성해서 풀에 넣어둠
    /// </summary>
    private void InitializePool()
    {
        linePool = new List<GameObject>();
        for(int i=0;i<initialPoolSize;i++)
        {
            GameObject lineObj = Instantiate(linePrefab, this.transform);
            lineObj.SetActive(false);
            linePool.Add(lineObj);
        }
    }
    /// <summary>
    /// 풀에서 비활성화된 선을 찾아 반환
    /// </summary>
    /// <returns></returns>
    private GameObject GetLineFromPool()
    {
        foreach(GameObject lineObj in linePool)
        {
            if(!lineObj.activeInHierarchy)
            {
                lineObj.SetActive(true);
                return lineObj;
            }
        }
        GameObject newLineObj = Instantiate(linePrefab, this.transform); // 풀에 넣어둔 것 다 쓰면 긴급 보충
        linePool.Add(newLineObj);
        return newLineObj;
    }
    ///<summary>
    ///현재 그리던 선을 종료 및 정리(손 떼기 및 영역 이탈 시)
    ///</summary>
    private void EndCurrentLine()
    {
        if (currentLine != null)
        {
            currentLine.Simplify(simplificationTolerance);
        }
        currentLine = null;
        currentLineData = null;
        currentLineObject = null;
        isDrawing = false;
    }
    ///<summary>
    ///특정 좌표가 여러 경로 중 하나라도 허용 범위 내에  있는지 판단
    ///</summary>
    public bool IsPositionInDrawingZone(Vector3 position, out SplineContainer foundSpline)
    {
        foundSpline = null;
        if(allTrackPaths == null || allTrackPaths.Count == 0)
        {
            return true;
        }
        foreach (SplineContainer trackPath in allTrackPaths)
        {
            if (trackPath == null || trackPath.Spline.Count == null)
                continue;
            Vector3 localPosition = trackPath.transform.InverseTransformPoint(position);
            foreach (Spline spline in trackPath.Splines)
            {
                if (spline.Count < 2) // 점 2개 미만이면 선이 아님
                    continue;
                float3 nearestPoint;
                float t;
                SplineUtility.GetNearestPoint(spline, (float3)localPosition, out nearestPoint, out t);
                float distance = Vector3.Distance(localPosition, nearestPoint); // 현재 좌표와 경로의 최단 거리
                if (distance <= maxTraceDistance)
                {
                    foundSpline = trackPath;
                    return true;
                }
            }
        }
        return false;
    }
}
