using UnityEngine;

[CreateAssetMenu(fileName = "NewWorldData", menuName = "GameData/WorldData")]
public class WorldData : ScriptableObject{
    [Header("ワールド基本情報")]
    public int worldNumber;        // 例：1, 2, 3
    public string worldName;       // 例："レベル1の島"
    public string sceneName;       // ロードするScene名（例："MapSelectScene_Level1"）

    // ▼▼▼ ここを新規追加 ▼▼▼
    [Header("ストーリー演出設定")]
    [Tooltip("このワールドに入る時の導入シーン名（空欄ならストーリーなしで直接マップへ）")]
    public string storySceneName;  // 例："Level1_OpeningScene"
                                   // ▲▲▲ 新規追加ここまで ▲▲▲

    [Tooltip("このワールドに行くために必要な解放レベル")]
    public int requiredWorldLevel = 1;
}