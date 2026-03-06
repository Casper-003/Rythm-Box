import librosa
import numpy as np
import soundfile as sf
import os
from flask import Flask, request, jsonify
# 引入新库用于提取封面
from mutagen import File
from mutagen.id3 import ID3, APIC

app = Flask(__name__)

def extract_album_art(file_path):
    """
    尝试从音频文件中提取封面图片。
    如果成功，保存为 temp_cover.jpg 并返回路径。
    如果失败，返回 None。
    """
    try:
        audio = File(file_path)
        artwork = None

        # 1. 针对 MP3 (ID3 标签)
        if audio.tags:
            for tag in audio.tags.values():
                if isinstance(tag, APIC):
                    artwork = tag.data
                    break
        
        # 2. 针对 FLAC (Picture Block) - 简单的兼容性尝试
        if not artwork and hasattr(audio, 'pictures'):
            if audio.pictures:
                artwork = audio.pictures[0].data

        if artwork:
            # 保存到同级目录
            cover_path = os.path.dirname(file_path) + "/temp_cover.jpg"
            with open(cover_path, 'wb') as img:
                img.write(artwork)
            print(f"🖼️ 提取封面成功: {cover_path}")
            return cover_path
    except Exception as e:
        print(f"⚠️ 提取封面失败 (这很正常，不是每首歌都有封面): {e}")
    
    return None

def intelligent_chart_generator(file_path):
    print(f"🎵 正在分析: {file_path}")
    
    # 1. 加载音频
    y, sr = librosa.load(file_path, duration=180) 

    # --- 格式转换 ---
    unity_audio_path = file_path
    if file_path.lower().endswith('.flac'):
        wav_path = file_path.rsplit('.', 1)[0] + "_converted.wav"
        if not os.path.exists(wav_path):
            sf.write(wav_path, y, sr)
        unity_audio_path = wav_path

    # --- ★★★ 新增：提取封面 ★★★ ---
    cover_path = extract_album_art(file_path)

    # 2. 提取重音
    onset_env = librosa.onset.onset_strength(y=y, sr=sr)
    onset_frames = librosa.onset.onset_detect(onset_envelope=onset_env, sr=sr, backtrack=True)
    onset_times = librosa.frames_to_time(onset_frames, sr=sr)
    
    # 3. 计算 BPM 和网格
    tempo, beat_frames = librosa.beat.beat_track(y=y, sr=sr)
    if isinstance(tempo, np.ndarray): tempo = tempo.item()
    print(f"🎹 BPM: {tempo:.2f}")

    beat_times = librosa.frames_to_time(beat_frames, sr=sr)
    grid_times = []
    for i in range(len(beat_times) - 1):
        start = beat_times[i]
        end = beat_times[i+1]
        grid_times.append(start)
        grid_times.append((start + end) / 2) 
    
    grid_times = np.array(grid_times)

    # 4. 分轨准备
    centroids = librosa.feature.spectral_centroid(y=y, sr=sr)[0]
    times_all = librosa.times_like(centroids, sr=sr)
    temp_notes = []
    collected_brightness = []
    last_grid_time = -1 

    for actual_time in onset_times:
        closest_idx = np.argmin(np.abs(grid_times - actual_time))
        snapped_time = grid_times[closest_idx]

        if abs(snapped_time - actual_time) > 0.15: continue
        if snapped_time == last_grid_time: continue
        last_grid_time = snapped_time
        
        frame_idx = np.argmin(np.abs(times_all - actual_time))
        brightness = centroids[frame_idx]
        
        temp_notes.append({"time": snapped_time, "brightness": brightness})
        collected_brightness.append(brightness)

    # 5. 动态分轨
    dynamic_threshold = np.median(collected_brightness) if collected_brightness else 2000
    
    final_notes = []
    for note in temp_notes:
        lane = 0 if note["brightness"] > dynamic_threshold else 1
        final_notes.append({"time": float(note["time"]), "lane": lane})

    return {
        "notes": final_notes,
        "audio_path": unity_audio_path,
        "cover_path": cover_path # ★ 返回封面路径给 Unity
    }

@app.route('/analyze', methods=['POST'])
def analyze():
    path = request.json.get('path')
    if not path: return jsonify({"error": "No path"}), 400
    try:
        data = intelligent_chart_generator(path)
        return jsonify(data)
    except Exception as e:
        print(f"❌ Error: {e}")
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    print("🚀 带封面提取功能的服务器已启动...")
    app.run(port=5000)