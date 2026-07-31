using UnityEngine;

// これを書くことで、右クリックメニューからこのデータを作成できるようになります
[CreateAssetMenu(fileName = "NewLevelData", menuName = "GameData/LevelData")]
public class LevelData : ScriptableObject{
    [Header("ステージ基本情報")]
    public int stageNumber;        // ステージ番号
    public string levelName;       // 画面に表示する名前（例："始まりの森"）
    public string sceneName;       // 実際にロードするScene名（例："Stage_01"）

    [Tooltip("このステージを遊ぶために必要な進行度（1なら最初から遊べる）")]
    public int requiredUnlockLevel = 1;

    [Header("UI表示用")]
    public Sprite thumbnail;       // マップ選択画面で表示する画像
    [TextArea(2, 3)]
    public string description;     // ステージの説明文（必要なら）
}