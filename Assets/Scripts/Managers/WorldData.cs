using UnityEngine;

[CreateAssetMenu(fileName = "NewWorldData", menuName = "GameData/WorldData")]
public class WorldData : ScriptableObject {
    [Header("ワールド基本情報")]
    public int worldNumber;        // 例：1, 2, 3
    public string worldName;       // 例："レベル1の島"
    public string sceneName;       // ロードするScene名（例："MapSelectScene_Level1"）
    
    [Tooltip("このワールドに行くために必要な解放レベル")]
    public int requiredWorldLevel = 1;
}