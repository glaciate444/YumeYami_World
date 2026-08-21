/* ===================================================
 * スクリプト名 : WorldData.cs
 * 用途 : ワールド（大マップのマス）のデータ定義
 * 拡張 : 単一レベル制から、フラグ条件（リスト）制へ移行
 * =================================================== */
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWorldData", menuName = "GameData/WorldData")]
public class WorldData : ScriptableObject {
    [Header("ワールド基本情報")]
    public int worldNumber;
    public string worldName;
    public string sceneName;

    [Header("ストーリー演出設定")]
    [Tooltip("このワールドに入る時の導入シーン名（空欄ならストーリーなしで直接マップへ）")]
    public string storySceneName;

    // ▼ 旧仕様（廃止）
    // public int requiredWorldLevel = 1;

    // ▼▼▼ 新仕様（RPG的フラグ管理用） ▼▼▼
    [Header("解放条件（フラグ式）")]
    [Tooltip("ここに入れたイベントフラグが【すべて】立っていれば解放されます。（例: Unlocked_World_2）")]
    public List<string> requiredEventFlags = new List<string>();
    // ▲▲▲ 新仕様ここまで ▲▲▲
}