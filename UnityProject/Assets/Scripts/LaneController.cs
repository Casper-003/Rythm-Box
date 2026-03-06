using UnityEngine;
using System.Collections.Generic;

public class LaneController : MonoBehaviour
{
    public int laneId; // 0, 1, 2, 3
    public KeyCode debugKey; // 键盘调试用

    // 这一条轨道上所有正在飞行的方块
    private List<Note> activeNotes = new List<Note>();

    // 判定区间 (秒)：比如误差在 0.15秒内都算击中
    private double perfectWindow = 0.15;

    // 当方块生成时，把它加入列表
    public void AddNote(Note note)
    {
        activeNotes.Add(note);
    }

    // 当方块飞过头销毁时，从列表移除
    public void RemoveNote(Note note)
    {
        if (activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
        }
    }

    // ★ 核心交互：玩家按下了这个轨道的键
    public void HandleInput()
    {
        // 如果轨道上没方块，直接忽略
        if (activeNotes.Count == 0) return;

        // 这里的逻辑是：检查列表里【第一个】也就是最靠近底部的方块
        // 注意：因为列表可能包含还没飞到的方块，我们需要找到时间最小的那个

        // 获取当前音乐时间
        double songTime = GameManager.Instance.GetMusicTime();

        // 找列表里最老的那个方块 (也就是应该最早被消除的)
        Note targetNote = activeNotes[0];

        // 计算误差：绝对值(方块的目标时间 - 当前音乐时间)
        double diff = System.Math.Abs(targetNote.targetTime - songTime);

        // 如果误差在允许范围内
        if (diff < perfectWindow)
        {
            Debug.Log($"🔥 轨道 {laneId} 击中! 误差: {diff:F3}s");

            // 1. 播放特效 (这里简单用变色代替，后面教你加粒子)
            HitVisualEffect();

            // 2. 销毁方块
            Destroy(targetNote.gameObject);

            // 3. 从列表移除 (防止重复击打)
            activeNotes.RemoveAt(0);
        }
        else
        {
            Debug.Log($"❌ 太早或太晚了! 误差: {diff:F3}s");
        }
    }

    void HitVisualEffect()
    {
        // 简单的视觉反馈：让判定框闪一下绿色
        GetComponent<Renderer>().material.color = Color.green;
        Invoke("ResetColor", 0.1f);
    }

    void ResetColor()
    {
        GetComponent<Renderer>().material.color = Color.white; // 恢复白色
    }
}