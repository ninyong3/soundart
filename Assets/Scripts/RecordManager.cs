using UnityEngine;
using System.Collections.Generic;
using System.IO;
[System.Serializable]
public class SavedPointData { public float x, y; }
[System.Serializable]
public class SavedLineData { public List<SavedPointData> points = new List<SavedPointData>(); }
[System.Serializable]
public class ClearRecord
{
    public string stageID;
    public string playTime;
    public List<SavedLineData> allLines = new List<SavedLineData>();
}
public class RecordManager : MonoBehaviour
{
    public void SavePlayData(string stageID, List<LineRenderer> playedLines, int missCount)
    {
        if (missCount > 0)
            return;
        ClearRecord newRecord =new ClearRecord();
        newRecord.stageID = stageID;
        newRecord.playTime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        foreach(LineRenderer line in playedLines)
        {
            SavedLineData lineData = new SavedLineData();
            Vector3[] positions = new Vector3[line.positionCount];
            line.GetPositions(positions);
            foreach(Vector3 pos in positions)
            {
                SavedPointData pt = new SavedPointData { x = pos.x, y = pos.y };
                lineData.points.Add(pt);
            }
            newRecord.allLines.Add(lineData);
        }
        string jsonData = JsonUtility.ToJson(newRecord, true);
        string fileName = stageID + "_" + newRecord.playTime + ".json";
        string savePath=Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(savePath, jsonData);
        Debug.Log("플레이 기록 저장: " + savePath);
    }
}
