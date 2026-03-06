using UnityEngine;

public class Note : MonoBehaviour
{
    public float targetTime;
    public float speed = 10f;
    public LaneController myLane;

    void Start()
    {
        if (myLane != null) myLane.AddNote(this);
    }

    void Update()
    {
        if (GameManager.Instance == null || myLane == null) return;

        float musicTime = GameManager.Instance.GetMusicTime();

        // ★★★ 核心修改：横向移动公式 ★★★
        // 目标 X (终点)
        float targetX = myLane.transform.position.x;

        // 当前 X = 终点X + (时间差 * 速度)
        // 时间差 > 0 时，音符在终点右侧 (Plus X)
        float currentX = targetX + (targetTime - musicTime) * speed;

        // 赋值：X在变，Y保持轨道高度，Z固定为0
        transform.position = new Vector3(
            currentX,
            myLane.transform.position.y,
            0
        );

        // 飞过头判定：如果 X 小于 -10 (屏幕最左边外面)，就销毁
        if (currentX < -10.0f)
        {
            if (myLane != null) myLane.RemoveNote(this);
            Destroy(gameObject);
        }
    }
}