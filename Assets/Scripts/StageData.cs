using UnityEngine;
[CreateAssetMenu(fileName = "New Stage", menuName = "RhythmGame/StageData")]
public class StageData: ScriptableObject
{
    [Header("기본 정보")]
    public string songTitle;
    public string composer;
    public string songLength;
    public string difficulty;
    public string rank;
    public string bestScore;
    public string stageID;
    [Header("리소스")]
    public Sprite coverImage;
    public AudioClip musicClip; // 미리듣기용 배경음악
    public float previewStartTime;
    [Header("게임 설정")]
    public string gameSceneName;
    public float bpm;
    public void LoadSavedData()
    {
        int savedScore = PlayerPrefs.GetInt(stageID + "_BestScore", 0);
        string savedGrade = PlayerPrefs.GetString(stageID + "_BestGrade", "-");
        this.bestScore=savedScore.ToString();
        this.rank = savedGrade;
    }
}
