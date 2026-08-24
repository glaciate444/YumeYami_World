/* ===================================================
 * スクリプト名 : LevelData.cs
 * 用途 : ステージのデータ定義
 * 拡張 : 単一レベル制から、フラグ条件（リスト）制へ移行
 * =================================================== */
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "GameData/LevelData")]
public class LevelData : ScriptableObject {
    [Header("ステージ基本情報")]
    public int stageNumber;
    public string levelName;
    public string sceneName;

    // ▼ 旧仕様（廃止）
    // public int requiredUnlockLevel = 1;

    // ▼▼▼ 新仕様（RPG的フラグ管理用） ▼▼▼
    [Header("解放条件（フラグ式）")]
    [Tooltip("ここに入れたステージ番号を【すべて】クリアしていれば解放されます。最初から遊べるなら空(0件)にします。")]
    public List<int> requiredClearedStageNumbers = new List<int>();

    [Tooltip("ここに入れたイベントフラグが【すべて】立っていれば解放されます。（例: Defeated_Boss1）")]
    public List<string> requiredEventFlags = new List<string>();
    // ▲▲▲ 新仕様ここまで ▲▲▲

    [Header("UI表示用")]
    public Sprite thumbnail;
    [TextArea(2, 3)]
    public string description;

    [Header("収集アイテム設定")]
    [Tooltip("このステージに存在するメダルの総数（レベル1は3、レベル2は4など）")]
    public int maxMedals = 5;
}