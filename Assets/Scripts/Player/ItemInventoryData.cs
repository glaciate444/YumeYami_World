using UnityEngine;

// 枠の色・役割に対応するカテゴリ
public enum ItemCategory{
    Weapon,     // Zキー：武器強化（赤/ピンク枠）
    SubAction,  // Xキー：動作系（緑枠）
    Special,    // Cキー：SP消費技（青枠）
    Passive     // 常時発動スキル（黄/紫枠）
}
// 緑枠（SubAction）で実行する具体的なアクションの種類
public enum SubActionType{
    None,      // なし
    Dash,      // ダッシュ
    Guard,     // ガード（予定）
    HipDrop    // ヒップドロップ（予定）
}

[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "GameData/ItemInventoryData")]
public class ItemInventoryData : ScriptableObject{
    [Header("基本情報")]
    public string itemName;
    [TextArea(2, 3)]
    public string description;
    public Sprite icon;          // UIに表示するアイコン
    public ItemCategory category;// このアイテムがどの枠に属するか

    // ▼ ここを新規追加（インスペクターで選べるようにする）
    [Tooltip("緑枠の場合のみ、どのアクションを発動するか選択します")]
    public SubActionType subActionType = SubActionType.None;

    public int itemId;           // セーブ・ロード用の固有ID

    [Header("各種パラメータ（用途に合わせて使用）")]
    public int attackPower;      // 武器の攻撃力など
    public int spCost;           // Cキー技などのSP消費量

    [Header("アクション系パラメータ（ダッシュなど）")]
    public float actionSpeed;    // ダッシュ速度など
    public float actionDuration; // 持続時間

    [Header("アクション用プレハブ")]
    [Tooltip("Cキーで発射する弾のプレハブなどを登録します")]
    public GameObject actionPrefab;
}