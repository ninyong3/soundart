using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public class ColorPickerManager : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI 연결")]
    public RectTransform circleRect;
    public RectTransform cursor;
    public TMP_InputField hexInput;
    [Header("팝업창 위치 설정")]
    public RectTransform pickerWindow;
    public Vector2 popupOffset = new Vector2(150f, 150f); // 오른쪽 위로 얼마나 띄울지 x, y
    [Header("무지개 이미지를 띄울 곳")]
    public Image colorCircleImage;
    public Image[] allPresets;
    private Image currentPreset;
    private Color currentColor=Color.white;
    private void Start()
    {
        hexInput.onEndEdit.AddListener(OnHexInputChanged);
        CreateColorCircleTexture();
    }
    public void OpenPicker(Image presetImage)
    {
        if(gameObject.activeSelf && currentPreset == presetImage)
        {
            gameObject.SetActive(false);
            return;
        }
        currentPreset=presetImage;
        gameObject.SetActive(true);
        pickerWindow.position =  presetImage.rectTransform.position;
        pickerWindow.localPosition += new Vector3(popupOffset.x, popupOffset.y, 0f);
        SyncPickerWithColor(presetImage.color);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateColorFromCircle(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        UpdateColorFromCircle(eventData);
    }
    private void UpdateColorFromCircle(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(circleRect, eventData.position, eventData.pressEventCamera, out Vector2 localPos);
        float radius = circleRect.rect.width / 2f; // 각도
        float distance = Mathf.Clamp(localPos.magnitude, 0, radius); // 중심으로부터의 거리, 원 밖으로 안 나가게
        float angle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360f;
        float hue = angle / 360f;
        float saturation = distance / radius;
        currentColor = Color.HSVToRGB(hue, saturation, 1f);
        cursor.localPosition = localPos.normalized * distance;
        ApplyColor();
        hexInput.text = ColorUtility.ToHtmlStringRGB(currentColor);
    }
    private void OnHexInputChanged(string hex)
    {
        if (!hex.StartsWith("#")) // 유저가 앞에 #을 안 붙였을 시 추가
            hex = "#"+hex;
        if(ColorUtility.TryParseHtmlString(hex, out Color newColor))
        {
            currentColor=newColor;
            ApplyColor();
        }
        else
        {
            Debug.Log("코드값이 이상함");
        }
    }
    private void ApplyColor()
    {
        if (currentPreset != null)
        {
            currentPreset.color = currentColor;
            int index = System.Array.IndexOf(allPresets, currentPreset);
            if(index >= 0)
            {
                GameManager.Instance.SaveColor(index, currentColor);
            }
        }
    }
    private void CreateColorCircleTexture()
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        for(int y=0;y<size;y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(center, pos);
                if (distance > radius) // 원 바깥쪽은 투명하게
                {
                    texture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    Vector2 dir = pos - center;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    if (angle < 0)
                        angle += 360f;
                    float hue = angle / 360f;
                    float saturation = distance / radius;
                    texture.SetPixel(x, y, Color.HSVToRGB(hue, saturation, 1f));
                }
            }
        }
        texture.Apply();
        colorCircleImage.sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    private void SyncPickerWithColor(Color color)
    {
        currentColor= color;
        hexInput.text = "#"+ColorUtility.ToHtmlStringRGB(color).ToUpper(); // 텍스트 업데이트(대문자로 통일)
        Color.RGBToHSV(color, out float h, out float s, out float v);
        float radius = circleRect.rect.width / 2f;
        float angleRad = h * Mathf.PI * 2f;
        float distance = s * radius;
        float x = Mathf.Cos(angleRad)*distance;
        float y = Mathf.Sin(angleRad)*distance;
        cursor.localPosition = new Vector2(x, y);
    }
}
