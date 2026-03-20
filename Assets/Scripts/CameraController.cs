using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    [Header("카메라 뷰 설정")]
    [Tooltip("카메라가 이동할 목표 지점들")]
    public Transform[] viewTargets;
    [Header("이동 및 줌 설정")]
    [Tooltip("이동하는데 걸리는 시간")]
    public float smoothTime = 0.4f;
    [Tooltip("줌(카메라 사이즈) 변경 속도")]
    public float zoomSpeed = 1.5f;
    private Vector3 velocity = Vector3.zero;
    private int currentIndex = 0;
    private Camera myCam;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        myCam = GetComponent<Camera>();
    }
    private void Start()
    {
        if (viewTargets != null && viewTargets.Length > 0)
        {
            Transform firstView = viewTargets[0];
            transform.position = new Vector3(firstView.position.x, firstView.position.y, -10f);
            myCam.orthographicSize = firstView.localScale.x;
        }
    }
    private void LateUpdate()
    {
        // 다른 물체 전부 update 후에 카메라 update
        if (viewTargets == null || viewTargets.Length == 0)
            return;
        Transform target = viewTargets[currentIndex];
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        float targetSize = target.localScale.x;
        myCam.orthographicSize = Mathf.Lerp(myCam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed); ;
    }
    public void MoveToView(int index)
    {
        if (index >= 0 && index < viewTargets.Length)
        {
            currentIndex = index;
        }
        else
        {
            Debug.LogWarning("[CameraController] 잘못된 뷰 인덱스입니다!");
        }
    }
}
