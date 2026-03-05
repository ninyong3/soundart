using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.Rendering.Universal;
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
    public Camera captureCamera;

    public void SavePlayData(string stageID, List<LineRenderer> playedLines, int missCount)
    { 
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
        StartCoroutine(TakeScreenshotRoutine(stageID, newRecord.playTime));
    }
    private IEnumerator TakeScreenshotRoutine(string stageId, string playTime)
    {
        yield return new WaitForEndOfFrame();
        if(captureCamera == null)
        {
            yield break;
        }
        RenderTexture originalTexture = captureCamera.targetTexture;
        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        captureCamera.targetTexture = rt;
        captureCamera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        screenShot.Apply();
        captureCamera.targetTexture = originalTexture;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] bytes =screenShot.EncodeToPNG();
        string imageFileName=stageId + "_" + playTime+".png";
        string imageSavePath = Path.Combine(Application.persistentDataPath, imageFileName);
        File.WriteAllBytes(imageSavePath, bytes);
        Debug.Log("플레이 섬네일 저장 위치: "+imageSavePath);
    }
}
