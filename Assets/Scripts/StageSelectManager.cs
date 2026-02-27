using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public class StageSelectManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("스테이지 데이터")]
    public List<StageData> allStageData;
    private int currentStageIndex = 0; // 현재 화면 가운데에 있는 곡의 번호
    [Header("UI 연결")]
    public RectTransform[] cards;
    public Image[] cardImages;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI composerText;
    [Header("오디오 설정")]
    public AudioSource previewAudioSource;
    private Vector2[] targetPos = new Vector2[5];
    private Vector3[] targetScale = new Vector3[5];
    private float[] targetAlpha = { 0f, 1f, 1f, 1f, 0f };
    private int[] currentPositions = { 0, 1, 2, 3, 4 };
    [Header("스와이프 설정")]
    public float swipeThreshold = 50f; // 넘어가기 위해 필요한 드래그 정도
    public float moveSpeed = 10f;
    private Vector2 startDragPos;
    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            targetPos[i] = cards[i].anchoredPosition;
            targetScale[i] = cards[i].localScale;
            Color c = cardImages[i].color;
            c.a=targetAlpha[i];
            cardImages[i].color = c;
        }
        UpdateCardData();
        UpdateCardHierarchy();
    }
    void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            int targetIndex = currentPositions[i];
            cards[i].anchoredPosition = Vector2.Lerp(cards[i].anchoredPosition, targetPos[targetIndex], Time.deltaTime * moveSpeed);
            cards[i].localScale = Vector3.Lerp(cards[i].localScale, targetScale[targetIndex], Time.deltaTime * moveSpeed);
            Color c = cardImages[i].color;
            c.a = Mathf.Lerp(c.a, targetAlpha[targetIndex], Time.deltaTime * moveSpeed);
            cardImages[i].color = c;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        startDragPos = eventData.position;
    }
    public void OnDrag(PointerEventData eventData)
    {

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        float dragDistance = eventData.position.x - startDragPos.x;
        if (dragDistance < -swipeThreshold)
        {
            NextStage();
        }
        else if (dragDistance > swipeThreshold)
        {
            PrevStage();
        }
    }
    public void NextStage()
    {
        if (allStageData.Count == 0)
            return;
        currentStageIndex = (currentStageIndex+1)%allStageData.Count;
        for (int i = 0; i < 5; i++)
        {
            currentPositions[i]--; // 자리 번호를 하나씩 뺌
            if (currentPositions[i] < 0)
                currentPositions[i] = 4;
        }
        UpdateCardData();
        UpdateCardHierarchy();
    }
    public void PrevStage()
    {
        if (allStageData.Count == 0)
            return;
        currentStageIndex--;
        if (currentStageIndex < 0)
            currentStageIndex = allStageData.Count-1;
        for (int i = 0; i < 5; i++)
        {
            currentPositions[i]++; // 자리 번호를 하나씩 늘림
            if (currentPositions[i] > 4)
                currentPositions[i] = 0;
        }
        UpdateCardData();
        UpdateCardHierarchy();
    }
    private void UpdateCardHierarchy()
    {
        for (int i = 0; i < 5; i++)
        {
            if (currentPositions[i] == 0 || currentPositions[i] == 4)
                cards[i].SetAsFirstSibling();
        }
        for (int i = 0; i < 5; i++)
        {
            if (currentPositions[i] == 1 || currentPositions[i] == 3)
                cards[i].SetAsLastSibling();
        }
        for(int i=0;i<5;i++)
        {
            if (currentPositions[i] == 2)
                cards[i].SetAsLastSibling();
        }
    }
    private void UpdateCardData()
    {
        if (allStageData.Count == 0)
            return;
        StageData centerData = allStageData[currentStageIndex];
        if (titleText != null)
            titleText.text = allStageData[currentStageIndex].songTitle;
        if (composerText != null)
            composerText.text = allStageData[currentStageIndex].composer;
        if (previewAudioSource != null && centerData.musicClip != null)
        {
            if (previewAudioSource.clip != centerData.musicClip)
            {
                previewAudioSource.clip = centerData.musicClip;
                previewAudioSource.time = centerData.previewStartTime;
                previewAudioSource.Play();
            }
        }
        int count = allStageData.Count;
        int leftSpareIdx = (currentStageIndex - 2 + count * 2) % count;
        int leftIdx = (currentStageIndex - 1 + count) % count;
        int centerIdx = currentStageIndex;
        int rightIdx = (currentStageIndex + 1) % count;
        int rightSpareIdx = (currentStageIndex + 2) % count;
        for(int i=0;i<5;i++)
        {
            int pos = currentPositions[i];
            if (pos == 0)
                cardImages[i].sprite = allStageData[leftSpareIdx].coverImage;
            else if (pos == 1)
                cardImages[i].sprite = allStageData[leftIdx].coverImage;
            else if(pos == 2)
                cardImages[i].sprite = allStageData[centerIdx].coverImage;
            else if(pos == 3)
                cardImages[i].sprite = allStageData[rightIdx].coverImage;
            else if (pos == 4)
                cardImages[i].sprite = allStageData[rightSpareIdx].coverImage;

        }
    }
}
