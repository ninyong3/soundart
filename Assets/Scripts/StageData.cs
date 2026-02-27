using UnityEngine;
[CreateAssetMenu(fileName = "New Stage", menuName = "RhythmGame/StageData")]
public class StageData: ScriptableObject
{
    [Header("기본 정보")]
    public string songTitle;
    public string composer;
    [Header("리소스")]
    public Sprite coverImage;
    public AudioClip musicClip; // 미리듣기용 배경음악
    public float previewStartTime;
    [Header("게임 설정")]
    public string gameSceneName;
    public float bpm;
    public int difficultyLevel;
}
