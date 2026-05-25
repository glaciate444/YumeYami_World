/* ===================================================
 * スクリプト名 : GameManager.cs
 * Version : Ver0.02
 * Update : 2026/05/25
 * 用途 : シーンを切り替えても絶対に消滅しない、ゲームの総司令塔
 * 拡張 : コインの保持、および PlayerPrefs によるセーブ・ロード機能の追加
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
    // セーブ・ロード機能 (PlayerPrefs を使用)
    // ==========================================

    public void SaveGame() {
        // PlayerPrefs.SetInt("キーの名前", 保存したい数値);
        PlayerPrefs.SetInt("MaxHp", currentMaxHp);
        PlayerPrefs.SetInt("MaxSp", currentMaxSp);
        PlayerPrefs.SetInt("UnlockedStageLevel", unlockedStageLevel);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        PlayerPrefs.Save(); // 書き込みを確定させる
        Debug.Log($"セーブ完了！ コイン: {totalCoins}, 解放ステージ: {unlockedStageLevel}");
    }

    public void LoadGame() {
        // HasKey で「そのデータが過去にセーブされているか」を確認してから読み込む
        if (PlayerPrefs.HasKey("MaxHp")) {
            currentMaxHp = PlayerPrefs.GetInt("MaxHp");
        }
        if (PlayerPrefs.HasKey("MaxSp")) {
            currentMaxSp = PlayerPrefs.GetInt("MaxSp");
        }
        if (PlayerPrefs.HasKey("UnlockedStageLevel")) {
            unlockedStageLevel = PlayerPrefs.GetInt("UnlockedStageLevel");
        }
        if (PlayerPrefs.HasKey("TotalCoins")) {
            totalCoins = PlayerPrefs.GetInt("TotalCoins");
        }

        Debug.Log("ロード完了！");
    }

    // テスト用にデータを初期化するメソッド（タイトル画面の「初めから」などで使います）
    public void ResetData() {
        PlayerPrefs.DeleteAll();
        
        // 初期値に戻す
        currentMaxHp = 12;
        currentMaxSp = 6;
        unlockedStageLevel = 1;
        totalCoins = 0;
        
        Debug.Log("セーブデータを全て削除し、初期化しました。");
    }
}