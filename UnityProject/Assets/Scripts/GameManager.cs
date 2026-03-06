using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 引入 UI 库
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NoteData { public float time; public int lane; }

[System.Serializable]
public class ChartData
{
    public List<NoteData> notes;
    public string audio_path;
    public string cover_path; // 接收封面路径
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("配置")]
    public string songPath;
    public GameObject notePrefab;
    public LaneController[] laneControllers;
    public AudioSource audioSource;

    [Header("UI 绑定")]
    public RawImage bgImage; // ★ 把场景里的 BackgroundImage 拖进来

    [Header("手感调整")]
    public float spawnAheadTime = 4.0f;
    public float noteSpeed = 5.0f;

    private bool isPlaying = false;
    private List<NoteData> chart;
    private int nextNoteIndex = 0;

    void Awake()
    {
        Instance = this;
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // ★★★ 关键修改 ★★★

        // 只有在【打包出的游戏】里 (即非编辑器环境)，才自动去读旁边的 music.mp3
#if !UNITY_EDITOR
            songPath = Application.dataPath + "/../music.mp3";
#endif

        // 在编辑器里 (#if UNITY_EDITOR)，上面那句不会执行。
        // 代码会直接使用你在 Inspector 面板里填写的 "E:/musictest/1.mp3"

        StartCoroutine(AskPythonForChart());
    }

    IEnumerator AskPythonForChart()
    {
        Debug.Log("正在呼叫 Python AI...");
        string json = "{\"path\": \"" + songPath + "\"}";
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        var request = new UnityWebRequest("http://127.0.0.1:5000/analyze", "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ChartData data = JsonUtility.FromJson<ChartData>(request.downloadHandler.text);
            chart = data.notes;
            chart.Sort((a, b) => a.time.CompareTo(b.time));

            // 播放音乐
            string playPath = !string.IsNullOrEmpty(data.audio_path) ? data.audio_path : songPath;
            StartCoroutine(LoadAndPlayMusic(playPath));

            // ★ 加载并模糊背景
            if (!string.IsNullOrEmpty(data.cover_path))
            {
                StartCoroutine(LoadAndBlurBackground(data.cover_path));
            }
        }
        else
        {
            Debug.LogError("API 错误: " + request.error);
        }
    }

    IEnumerator LoadAndPlayMusic(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                yield return new WaitForSeconds(0.5f);
                audioSource.Play();
                isPlaying = true;
            }
        }
    }

    // ★★★ 极速模糊黑科技 ★★★
    IEnumerator LoadAndBlurBackground(string path)
    {
        // 使用 file:// 协议加载本地图片
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 1. 获取下载的纹理
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                // 2. 将纹理赋值给 RawImage 组件
                bgImage.texture = texture;

                // 3. 设置颜色为半透明灰色，起到压暗的效果
                bgImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                // 注意：模糊效果现在是由挂在 BackgroundImage 上的材质球 (BlurMat) 完成的，
                // 代码里不需要再做任何模糊操作了。
            }
            else
            {
                Debug.LogError("加载背景图片失败: " + www.error);
            }
        }
    }

    void Update()
    {
        if (!isPlaying) return;
        while (nextNoteIndex < chart.Count && (chart[nextNoteIndex].time - spawnAheadTime) <= audioSource.time)
        {
            SpawnNote(chart[nextNoteIndex]);
            nextNoteIndex++;
        }

        // ... (输入逻辑保持不变，为了篇幅我省略了输入部分，请保留你原来的 Input 代码！) ...
        // 请保留你原来的 Update 里的 Gamepad 和 Keyboard 逻辑！
        // 这里只是演示背景加载
        HandleInput(); // 假设你把输入封装到了这个函数
    }

    // 把原来的 Update 输入代码复制回来放在这里
    void HandleInput()
    {
        if (Gamepad.current != null)
        {
            var gp = Gamepad.current;
            if (gp.dpad.up.wasPressedThisFrame || gp.dpad.down.wasPressedThisFrame || gp.dpad.left.wasPressedThisFrame || gp.dpad.right.wasPressedThisFrame || gp.leftShoulder.wasPressedThisFrame || gp.leftTrigger.wasPressedThisFrame) laneControllers[0].HandleInput();
            if (gp.buttonNorth.wasPressedThisFrame || gp.buttonSouth.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame || gp.buttonWest.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame || gp.rightTrigger.wasPressedThisFrame) laneControllers[1].HandleInput();
        }
        if (Keyboard.current.wKey.wasPressedThisFrame) laneControllers[0].HandleInput();
        if (Keyboard.current.downArrowKey.wasPressedThisFrame) laneControllers[1].HandleInput();
    }

    void SpawnNote(NoteData data)
    {
        // ... (保持不变)
        int laneIndex = Mathf.Clamp(data.lane, 0, 1);
        LaneController targetLane = laneControllers[laneIndex];
        float spawnX = targetLane.transform.position.x + (spawnAheadTime * noteSpeed);
        Vector3 spawnPos = new Vector3(spawnX, targetLane.transform.position.y, 0);
        GameObject obj = Instantiate(notePrefab, spawnPos, Quaternion.identity);
        Note noteScript = obj.GetComponent<Note>();
        noteScript.targetTime = data.time + 0.1f;
        noteScript.speed = noteSpeed;
        noteScript.myLane = targetLane;
    }

    public float GetMusicTime() { return audioSource.time; }
}