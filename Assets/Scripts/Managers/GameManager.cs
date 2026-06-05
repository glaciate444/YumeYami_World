/* ===================================================
 * スクリプト名 : GameManager.cs
 * Version : Ver0.04
 * 用途 : シーンを切り替えても絶対に消滅しない、ゲームの総司令塔
 * 拡張 : LifeHUDと連携して、残基UIを自動更新する機能を追加
 * =================================================== */
using UnityEngine;

public class GameManager : MonoBehaviour{
    public static GameManager Instance;

    [Header("プレイヤーのデータ（セーブ対象）")]
    public int currentMaxHp = 12;
    public int currentMaxSp = 6;
    public int unlockedStageLevel = 1; 
    public int totalCoins = 0;

    [Header("残基システム")]
    public int currentLives = 3;       
    public int currentLifePieces = 0;  

    public AudioClip oneUpSE; 

    void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }else{
            Destroy(gameObject);
        }
    }

    // ==========================================
    // 1UPアイテムを取得した時の専用処理
    // ==========================================
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
        
        // ▼【追加】画面内の LifeHUD を探して、UIを最新状態に更新させる ▼
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
        PlayerPrefs.Save(); 
    }

    public void LoadGame() {
        if (PlayerPrefs.HasKey("MaxHp")) currentMaxHp = PlayerPrefs.GetInt("MaxHp");
        if (PlayerPrefs.HasKey("MaxSp")) currentMaxSp = PlayerPrefs.GetInt("MaxSp");
        if (PlayerPrefs.HasKey("UnlockedStageLevel")) unlockedStageLevel = PlayerPrefs.GetInt("UnlockedStageLevel");
        if (PlayerPrefs.HasKey("TotalCoins")) totalCoins = PlayerPrefs.GetInt("TotalCoins");
        if (PlayerPrefs.HasKey("CurrentLives")) currentLives = PlayerPrefs.GetInt("CurrentLives");
        if (PlayerPrefs.HasKey("CurrentLifePieces")) currentLifePieces = PlayerPrefs.GetInt("CurrentLifePieces");
    }

    public void ResetData() {
        PlayerPrefs.DeleteAll();
        currentMaxHp = 12;
        currentMaxSp = 6;
        unlockedStageLevel = 1;
        totalCoins = 0;
        currentLives = 3;
        currentLifePieces = 0;
    }
}