using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class DetailViewer : MonoBehaviour
{
    [System.Serializable]
    public struct StageTrackMapping
    {
        public string stageID;
        public GameObject trackPrefab;
    }
    [Header("프리팹 소환 설정")]
    [Tooltip("스테이지 이름과 프리팹 연결")]
    public List<StageTrackMapping> trackMappings;
    [Header("유저 선 관련 설정")]
    public Material lineMaterial;
    public float lineWidth = 0.2f;
    [Header("카메라 이동 설정")]
    public float panSpeed = 1.0f;
    public SpriteRenderer backgroundSprite;
    private void Start()
    {
        SpawnTrack();
        DrawSavedLines();
    }
    private void SpawnTrack()
    {
        string targetID = GalleryDataKeeper.targetStageID;
        foreach (var mapping in trackMappings)
        {
            if (mapping.stageID == targetID)
            {
                Instantiate(mapping.trackPrefab, new Vector3(-17.6f, 27.7f, 0f), Quaternion.identity);
                Debug.Log("트랙 소환 성공: " + targetID);
                return;
            }
            Debug.LogWarning(targetID + "에 해당하는 트랙 프리팹이 리스트에 없음");
        }
    }
    private void DrawSavedLines()
    {
        string path = GalleryDataKeeper.targetJsonPath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogWarning("선 데이터 찾기 실패");
            return;
        }
        string json = File.ReadAllText(path);
        ClearRecord record = JsonUtility.FromJson<ClearRecord>(json);
        foreach (SavedLineData lineData in record.allLines)
        {
            GameObject lineObj = new GameObject("ReplayLine");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            Color parsedColor=Color.black;
            if (!string.IsNullOrEmpty(lineData.colorHex))
            {
                ColorUtility.TryParseHtmlString(lineData.colorHex, out parsedColor);
            }
            lr.material = lineMaterial;
            lr.startColor = parsedColor;
            lr.endColor = parsedColor;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = true;
            lr.sortingOrder = 2;
            lr.positionCount = lineData.points.Count;
            for (int i = 0; i < lineData.points.Count; i++)
            {
                lr.SetPosition(i, new Vector3(lineData.points[i].x, lineData.points[i].y, -1f));
            }
        }
        Debug.Log("그렸던 선 생성 완료");
    }
    private void Update()
    {
        if(Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.primaryTouch;
            if(touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector2 deltaPos = touch.delta.ReadValue();
                MoveCamera(deltaPos);
            }
        }
        else if(Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 deltaPos = Mouse.current.delta.ReadValue();
            MoveCamera(deltaPos);
        }
    }
    void MoveCamera(Vector2 deltaScreen)
    {
        float worldPerPixel = (Camera.main.orthographicSize * 2f) / Screen.height;
        Vector3 delta = new Vector3(-deltaScreen.x, -deltaScreen.y, 0) * worldPerPixel * panSpeed;
        Vector3 targetPos=Camera.main.transform.position+delta;
        if (backgroundSprite != null)
        {
            float camHeight = Camera.main.orthographicSize;
            float camWidth = camHeight * Camera.main.aspect;
            Bounds bgBounds = backgroundSprite.bounds;
            float minX = bgBounds.min.x + camWidth;
            float maxX = bgBounds.max.x - camWidth;
            float minY = bgBounds.min.y + camHeight;
            float maxY = bgBounds.max.y - camHeight;
            if (minX > maxX) minX = maxX = bgBounds.center.x;
            if (minY > maxY) minY = maxY = bgBounds.center.y;
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }
        Camera.main.transform.position=targetPos;
    }
    public void GoBackToGallery()
    {
        SceneManager.LoadScene("Gallery");
    }
}
