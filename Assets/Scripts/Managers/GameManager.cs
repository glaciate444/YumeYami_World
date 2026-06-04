/* ===================================================
 * スクリプト名 : GameManager.cs
 * Version : Ver0.03
 * Update : 2026/06/04
 * 用途 : シーンを切り替えても絶対に消滅しない、ゲームの総司令塔
 * 拡張 : 残基（Stock）と、1UPアイテム（LifePiece）のシステムを追加
 * =================================================== */
using UnityEngine;

public class GameManager : MonoBehaviour{
    // どこからでも GameManager.Instance でアクセスできるようにする（シングルトン）
    public static GameManager Instance;

    [Header("プレイヤーのデータ（セーブ対象）")]
    public int currentMaxHp = 12;
    public int currentMaxSp = 6;
    public int unlockedStageLevel = 1; // どこまでクリアしたか
    
    // ▼【追加】コインの所持数
    public int totalCoins = 0;

    // ▼【追加】残基と1UP用のアイテム数
    [Header("残基システム")]
    public int currentLives = 3;       // 残基（初期値3）
    public int currentLifePieces = 0;  // 1UPアイテムの所持数（0～99）

    // ▼【追加】1UPの効果音（インスペクターからセットできます）
    public AudioClip oneUpSE;

    void Awake(){
        // 自分が最初の1つ目なら、シーンを跨いでも消えないようにする
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ▼【追加】ゲーム起動時（GameManager誕生時）に一度だけデータをロードする
            LoadGame();
        }
        // すでにGameManagerが存在しているなら、自分（重複分）を削除する
        else{
            Destroy(gameObject);
        }
    }

    // ==========================================
    // 1UPアイテムを取得した時の専用処理
    // ==========================================
    public void AddLifePiece(int amount){
        currentLifePieces += amount;
        Debug.Log($"1UPアイテムゲット！ 現在: {currentLifePieces} / 100");
        
        // 100個以上になったら1UPする！
        if (currentLifePieces >= 100){
            // 100引いて、残りを繰り越す（例: 98個の時に5個取ったら、3個残る）
            currentLifePieces -= 100;
            currentLives++;
            
            // 1UPのファンファーレを鳴らす
            if (SoundManager.instance != null && oneUpSE != null) {
                SoundManager.instance.PlaySE(oneUpSE);
            }
            Debug.Log($"1UPしました！ 残基: {currentLives}");
        }
        
        // ※後ほど、ここでHUD（UI）を更新する処理を呼びます
    }

    // ==========================================
    // セーブ・ロード機能 (PlayerPrefs を使用)
    // ==========================================

    public void SaveGame() {
        // PlayerPrefs.SetInt("キーの名前", 保存したい数値);
        PlayerPrefs.SetInt("MaxHp", currentMaxHp);
        PlayerPrefs.SetInt("MaxSp", currentMaxSp);
        PlayerPrefs.SetInt("UnlockedStageLevel", unlockedStageLevel);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        // ▼【追加】
        PlayerPrefs.SetInt("CurrentLives", currentLives);
        PlayerPrefs.SetInt("CurrentLifePieces", currentLifePieces);

        PlayerPrefs.Save();

        Debug.Log($"セーブ完了！ コイン: {totalCoins}, 解放ステージ: {unlockedStageLevel}");
    }
    public void LoadGame(){
        if (PlayerPrefs.HasKey("MaxHp")) currentMaxHp = PlayerPrefs.GetInt("MaxHp");
        if (PlayerPrefs.HasKey("MaxSp")) currentMaxSp = PlayerPrefs.GetInt("MaxSp");
        if (PlayerPrefs.HasKey("UnlockedStageLevel")) unlockedStageLevel = PlayerPrefs.GetInt("UnlockedStageLevel");
        if (PlayerPrefs.HasKey("TotalCoins")) totalCoins = PlayerPrefs.GetInt("TotalCoins");

        // ▼【追加】
        if (PlayerPrefs.HasKey("CurrentLives")) currentLives = PlayerPrefs.GetInt("CurrentLives");
        if (PlayerPrefs.HasKey("CurrentLifePieces")) currentLifePieces = PlayerPrefs.GetInt("CurrentLifePieces");
    }

    // テスト用にデータを初期化するメソッド（タイトル画面の「初めから」などで使います）
    public void ResetData() {
        PlayerPrefs.DeleteAll();
        
        // 初期値に戻す
        currentMaxHp = 12;
        currentMaxSp = 6;
        unlockedStageLevel = 1;
        totalCoins = 0;

        // ▼【追加】初期値に戻す
        currentLives = 3;
        currentLifePieces = 0;
    }
}