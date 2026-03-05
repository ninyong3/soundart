using UnityEngine;
using TMPro;
using System.Collections;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public enum HitAccuracy {Perfect, Good, Miss}
    [Header("UI 연결")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI accuracyText;
    [Header("설정")]
    public float accuracyDisplayTime = 1.0f;
    private int score = 0;
    private int combo = 0;
    private Coroutine hideTextCoroutine;
    private void Awake()
    {
        if(Instance == null)
            Instance= this;
        if (accuracyText != null)
            accuracyText.text = "";
        UpdateUI();
    }
    public void AddHit(HitAccuracy accuracy)
    {
        combo++;
        if (accuracy == HitAccuracy.Perfect)
        {
            score += 300 * combo;
            ShowAccuracyText("Perfect!");
        }
        else if (accuracy == HitAccuracy.Good)
        {
            score += 100 * combo;
            ShowAccuracyText("Good!");
        }
        UpdateUI();
    }
    public void ResetCombo()
    {
        combo = 0;
        ShowAccuracyText("Miss!");
        UpdateUI();
    }
    public void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        if (comboText != null)
            comboText.text = combo + " Combo";
    }
    private void ShowAccuracyText(string text)
    {
        if (accuracyText == null)
            return;
        accuracyText.text=text;
        if(hideTextCoroutine != null)
            StopCoroutine(hideTextCoroutine);
        hideTextCoroutine = StartCoroutine(HideTextAfterDelay());
    }
    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(accuracyDisplayTime);
        accuracyText.text = "";
    }
    public int GetScore()
    {
        return score;
    }
    public void ResetScoreManager()
    {
        score = 0;
        combo = 0;
        if (accuracyText != null)
            accuracyText.text = "";
        UpdateUI();
    }
}
