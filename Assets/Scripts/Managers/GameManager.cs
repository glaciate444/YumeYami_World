/* ===================================================
 * スクリプト名 : GameManager.cs
 * Version : Ver0.04
 * 用途 : シーンを切り替えても絶対に消滅しない、ゲームの総司令塔
 * 拡張 : マップ上の現在位置（ノード番号）を記憶する機能を追加
 * =================================================== */
using UnityEngine;

public class GameManager : MonoBehaviour{
    public static GameManager Instance;

    [Header("プレイヤーのデータ（セーブ対象）")]
    public int currentMaxHp = 12;
    public int currentMaxSp = 6;
    public int unlockedStageLevel = 1; 
    public int totalCoins = 0;

    // ▼【新規追加】最後にいたマップのノード番号（LevelDataのstageNumberと対応）
    public int currentMapNodeNumber = 1; 

    [Header("ステージで集めたコインの一時保存用（セーブ非対象）")]
    public int stageCoins = 0;

    [Header("残基システム")]
    public int currentLives = 3;       
    public int currentLifePieces = 0;  

    public AudioClip oneUpSE;

    // ▼ 変数宣言の場所に追加
    [Header("ワールド進行データ")]
    public int unlockedWorldLevel = 1;   // レベル2が解放されたら2になる
    public int currentWorldNodeNumber = 1; // マップの現在位置記憶用

    [Header("マップ遷移用")]
    public string returnMapSceneName = ""; // ← これを追加！

    void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }else{
            Destroy(gameObject);
        }
    }

    public void AddLifePiece(int amount) {
        currentLifePieces += amount;
        Debug.Log($"1UPアイテムゲット！ 現在: {currentLifePieces} / 100");
        
        if (currentLifePieces >= 100) {
            currentLifePieces -= 100;
            currentLives++;
            
            if (SoundManager.instance != null && oneUpSE != null) {
                SoundManager.instance.PlaySE(oneUpSE);
            }
            Debug.Log($"1UPしました！ 残基: {currentLives}");
        }
        
        LifeHUD hud = FindFirstObjectByType<LifeHUD>();
        if (hud != null){
            hud.UpdateHUD();
        }
    }

    // ==========================================
    // セーブ・ロード・リセット機能
    // ==========================================
    public void SaveGame() {
        PlayerPrefs.SetInt("MaxHp", currentMaxHp);
        PlayerPrefs.SetInt("MaxSp", currentMaxSp);
        PlayerPrefs.SetInt("UnlockedStageLevel", unlockedStageLevel);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetInt("CurrentLives", currentLives);
        PlayerPrefs.SetInt("CurrentLifePieces", currentLifePieces);
        
        // ▼【追加】現在位置をセーブ
        PlayerPrefs.SetInt("CurrentMapNodeNumber", currentMapNodeNumber);

        // ▼ SaveGame() の中に追加
        PlayerPrefs.SetInt("UnlockedWorldLevel", unlockedWorldLevel);
        PlayerPrefs.SetInt("CurrentWorldNodeNumber", currentWorldNodeNumber);

        PlayerPrefs.Save(); 
    }

    public void LoadGame() {
        if (PlayerPrefs.HasKey("MaxHp")) currentMaxHp = PlayerPrefs.GetInt("MaxHp");
        if (PlayerPrefs.HasKey("MaxSp")) currentMaxSp = PlayerPrefs.GetInt("MaxSp");
        if (PlayerPrefs.HasKey("UnlockedStageLevel")) unlockedStageLevel = PlayerPrefs.GetInt("UnlockedStageLevel");
        if (PlayerPrefs.HasKey("TotalCoins")) totalCoins = PlayerPrefs.GetInt("TotalCoins");
        if (PlayerPrefs.HasKey("CurrentLives")) currentLives = PlayerPrefs.GetInt("CurrentLives");
        if (PlayerPrefs.HasKey("CurrentLifePieces")) currentLifePieces = PlayerPrefs.GetInt("CurrentLifePieces");
        
        // ▼【追加】現在位置をロード
        if (PlayerPrefs.HasKey("CurrentMapNodeNumber")) currentMapNodeNumber = PlayerPrefs.GetInt("CurrentMapNodeNumber");

        // ▼ LoadGame() の中に追加
        if (PlayerPrefs.HasKey("UnlockedWorldLevel")) unlockedWorldLevel = PlayerPrefs.GetInt("UnlockedWorldLevel");
        if (PlayerPrefs.HasKey("CurrentWorldNodeNumber")) currentWorldNodeNumber = PlayerPrefs.GetInt("CurrentWorldNodeNumber");
    }

    public void ResetData() {
        PlayerPrefs.DeleteAll();
        currentMaxHp = 12;
        currentMaxSp = 6;
        unlockedStageLevel = 1;
        totalCoins = 0;
        currentLives = 3;
        currentLifePieces = 0;

        // ▼ ResetData() の中に追加
        unlockedWorldLevel = 1;
        currentWorldNodeNumber = 1;

        // ▼【追加】現在位置も初期化
        currentMapNodeNumber = 1;
    }
}