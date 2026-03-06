# Rythm-Box (Experimental Prototype)

一个基于 **Unity** 和 **Python** 开发的实验性音乐游戏原型。本项目尝试利用音频处理算法实现谱面的自动生成与同步。

目前项目处于初级开发阶段，主要用于验证 **Librosa** 音频分析与 **Unity** 游戏逻辑结合的可行性。

---

## 项目概览

本项目由两部分组成：
1. **Python Server**：基于 Flask 框架，使用 Librosa 库对音乐进行 BPM 检测和重音提取。
2. **Unity Client**：负责音符生成、玩家输入判定及视觉呈现。

### 已实现功能
* **自动谱面生成**：通过自适应分轨算法，尝试根据音频频率分布自动分配高低轨道。
* **节奏对齐 (Snapping)**：尝试将检测到的重音点吸附至最近的 1/8 拍网格上，以维持基本的节奏律动。
* **视觉呈现**：支持提取 MP3 内置封面并实现简单的背景模糊效果。

---

## 运行环境与依赖

### Python 端
* **Python 3.10+**
* 依赖库：`librosa`, `flask`, `mutagen`, `numpy`, `soundfile`
* 建议安装方式：`pip install -r requirements.txt`

### Unity 端
* **Unity 6 (6000.3.6f1)** 或更高版本。
* 硬件参考：本项目在基础轻薄本（如 i3-1005G1）设备上通过了初步运行测试。

---

## 如何使用

由于项目尚不完善，运行过程需要手动启动两个端：

1. **启动服务器**：
   在 `PythonServer/` 目录下运行 `python server.py`。请确保服务器运行在 `127.0.0.1:5000` 端口。
2. **准备音乐**：
   将一个名为 `music.mp3` 的音频文件放置在游戏的可执行文件同级目录下。
3. **运行游戏**：
   在 Unity 编辑器中打开项目，或运行构建后的 `.exe` 文件。

**注意**：如果开启了全局 VPN/代理，可能会导致 Unity 无法连接至本地服务器 (502 Bad Gateway)，请在运行时关闭代理或添加本地回环例外。

---

## 项目结构

```text
Rythm-Box/
├── UnityProject/          # Unity 工程源文件
│   ├── Assets/            # 核心脚本、材质与 Prefab
│   ├── Packages/          # 包管理配置
│   └── ProjectSettings/   # 输入与图层配置
├── PythonServer/          # 后端分析源码
│   ├── server.py          # AI 分析核心逻辑
│   └── requirements.txt   # Python 依赖清单
└── .gitignore             # 忽略规则配置
```

---

## 局限性与已知问题
* **分析精度**：自适应算法在复杂多变的曲风下可能表现不佳，生成的谱面仅具参考意义。
* **延迟处理**：目前尚未建立完善的音频延迟补偿机制，不同设备的同步表现可能存在差异。
* **操作方式**：目前默认支持控制器或键盘 `W` 和 `向下方向键`。

---

## 关于作者
* **GitHub**: [Casper-003](https://github.com/Casper-003)
* **Email**: casper-003@outlook.com
---

## 许可说明
本项目采用 **MIT License** 开源。仅供学习与交流使用。
