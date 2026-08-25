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
    [Tooltip("システム管理用のID（11, 21など。フラグ管理に使います）")]
    public int stageNumber;

    // プレイヤーに見せる用の番号
    [Tooltip("トランジション画面で表示するCourse No.（1-1なら1、2-4なら10など）")]
    public int displayCourseNumber = 1;

    public string levelName;
    public string sceneName;

    [Header("解放条件（フラグ式）")]
    public List<int> requiredClearedStageNumbers = new List<int>();
    public List<string> requiredEventFlags = new List<string>();

    [Header("UI表示用")]
    public Sprite thumbnail;
    [TextArea(2, 3)]
    public string description;

    [Header("収集アイテム設定")]
    [Tooltip("このステージに存在するメダルの総数")]
    public int maxMedals = 3;
}