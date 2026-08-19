/* ===================================================
 * スクリプト名 : GameManager.cs
 * 用途 : シーンを切り替えても絶対に消滅しない、ゲームの総司令塔
 * 拡張 : セーブデータ多重スロット（ファイル1〜4）対応版
 * =================================================== */
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    // ▼▼▼ 新規追加：セーブスロット管理 ▼▼▼
    [Header("セーブデータ管理")]
    public int currentSaveSlot = 1; // 現在選んでいるファイル番号（1〜4）
    // ▲▲▲ 新規追加ここまで ▲▲▲

    [Header("プレイヤーのデータ（セーブ対象）")]
    public int currentMaxHp = 12;
    public int currentMaxSp = 6;
    public int unlockedStageLevel = 1;
    public int totalCoins = 0;

    public int currentMapNodeNumber = 1;

    [Header("ステージで集めたコインの一時保存用（セーブ非対象）")]
    public int stageCoins = 0;

    [Header("残基システム")]
    public int currentLives = 3;
    public int currentLifePieces = 0;

    public AudioClip oneUpSE;

    [Header("ワールド進行データ")]
    public int unlockedWorldLevel = 1;
    public int currentWorldNodeNumber = 1;

    [Header("マップ遷移用")]
    public string returnMapSceneName = "";

    [Header("インベントリデータ（セーブ対象）")]
    public System.Collections.Generic.List<int> ownedItemIds = new System.Collections.Generic.List<int>();

    [Header("ショップでの購入回数（セーブ対象）")]
    public int hpUpPurchaseCount = 0;
    public int spUpPurchaseCount = 0;

    void Awake(){
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 注意：Awakeでの LoadGame() は外しています（タイトル画面でスロットを選んでからロードするため）
        }else{
            Destroy(gameObject);
        }
    }

    public void AddLifePiece(int amount){
        currentLifePieces += amount;
        if (currentLifePieces >= 100){
            currentLifePieces -= 100;
            currentLives++;
            if (SoundManager.instance != null && oneUpSE != null){
                SoundManager.instance.PlaySE(oneUpSE);
            }
        }
        LifeHUD hud = FindFirstObjectByType<LifeHUD>();
        if (hud != null) hud.UpdateHUD();
    }

    // ==========================================
    // セーブ・ロード・リセット機能（多重スロット対応）
    // ==========================================
    public void SaveGame(){
        string s = "_" + currentSaveSlot; // 例: "_1"

        PlayerPrefs.SetInt("IsSaved" + s, 1); // データが存在する証拠のフラグ

        PlayerPrefs.SetInt("MaxHp" + s, currentMaxHp);
        PlayerPrefs.SetInt("MaxSp" + s, currentMaxSp);
        PlayerPrefs.SetInt("UnlockedStageLevel" + s, unlockedStageLevel);
        PlayerPrefs.SetInt("TotalCoins" + s, totalCoins);
        PlayerPrefs.SetInt("CurrentLives" + s, currentLives);
        PlayerPrefs.SetInt("CurrentLifePieces" + s, currentLifePieces);
        PlayerPrefs.SetInt("CurrentMapNodeNumber" + s, currentMapNodeNumber);
        PlayerPrefs.SetInt("UnlockedWorldLevel" + s, unlockedWorldLevel);
        PlayerPrefs.SetInt("CurrentWorldNodeNumber" + s, currentWorldNodeNumber);

        string itemIdsStr = string.Join(",", ownedItemIds);
        PlayerPrefs.SetString("OwnedItemIds" + s, itemIdsStr);

        PlayerPrefs.SetInt("HpUpPurchaseCount" + s, hpUpPurchaseCount);
        PlayerPrefs.SetInt("SpUpPurchaseCount" + s, spUpPurchaseCount);

        PlayerPrefs.Save();
    }

    public void LoadGame(){
        string s = "_" + currentSaveSlot;

        if (PlayerPrefs.HasKey("MaxHp" + s)) currentMaxHp = PlayerPrefs.GetInt("MaxHp" + s);
        if (PlayerPrefs.HasKey("MaxSp" + s)) currentMaxSp = PlayerPrefs.GetInt("MaxSp" + s);
        if (PlayerPrefs.HasKey("UnlockedStageLevel" + s)) unlockedStageLevel = PlayerPrefs.GetInt("UnlockedStageLevel" + s);
        if (PlayerPrefs.HasKey("TotalCoins" + s)) totalCoins = PlayerPrefs.GetInt("TotalCoins" + s);
        if (PlayerPrefs.HasKey("CurrentLives" + s)) currentLives = PlayerPrefs.GetInt("CurrentLives" + s);
        if (PlayerPrefs.HasKey("CurrentLifePieces" + s)) currentLifePieces = PlayerPrefs.GetInt("CurrentLifePieces" + s);
        if (PlayerPrefs.HasKey("CurrentMapNodeNumber" + s)) currentMapNodeNumber = PlayerPrefs.GetInt("CurrentMapNodeNumber" + s);
        if (PlayerPrefs.HasKey("UnlockedWorldLevel" + s)) unlockedWorldLevel = PlayerPrefs.GetInt("UnlockedWorldLevel" + s);
        if (PlayerPrefs.HasKey("CurrentWorldNodeNumber" + s)) currentWorldNodeNumber = PlayerPrefs.GetInt("CurrentWorldNodeNumber" + s);

        if (PlayerPrefs.HasKey("HpUpPurchaseCount" + s)) hpUpPurchaseCount = PlayerPrefs.GetInt("HpUpPurchaseCount" + s);
        if (PlayerPrefs.HasKey("SpUpPurchaseCount" + s)) spUpPurchaseCount = PlayerPrefs.GetInt("SpUpPurchaseCount" + s);

        if (PlayerPrefs.HasKey("OwnedItemIds" + s)){
            string idsStr = PlayerPrefs.GetString("OwnedItemIds" + s);
            ownedItemIds.Clear();
            if (!string.IsNullOrEmpty(idsStr)){
                string[] idArray = idsStr.Split(',');
                foreach (string idStr in idArray)
                {
                    if (int.TryParse(idStr, out int id))
                    {
                        ownedItemIds.Add(id);
                    }
                }
            }
        }
    }

    public void ResetData(){
        // ★重要：PlayerPrefs.DeleteAll() は使わない！（他のファイルも消えるため）
        // あくまで「現在ゲーム内で動いているメモリ上の数値を初期値に戻す」だけにする
        currentMaxHp = 12;
        currentMaxSp = 6;
        unlockedStageLevel = 1;
        totalCoins = 0;
        currentLives = 3;
        currentLifePieces = 0;
        unlockedWorldLevel = 1;
        currentWorldNodeNumber = 1;
        currentMapNodeNumber = 1;
        hpUpPurchaseCount = 0;
        spUpPurchaseCount = 0;
        ownedItemIds.Clear();
    }

    // ▼▼▼ 新規追加：指定したスロットのセーブデータを消去する（ファイルを消す用） ▼▼▼
    public void DeleteSaveData(int slot)
    {
        string s = "_" + slot;
        PlayerPrefs.DeleteKey("IsSaved" + s);
        PlayerPrefs.DeleteKey("MaxHp" + s);
        PlayerPrefs.DeleteKey("MaxSp" + s);
        PlayerPrefs.DeleteKey("UnlockedStageLevel" + s);
        PlayerPrefs.DeleteKey("TotalCoins" + s);
        PlayerPrefs.DeleteKey("CurrentLives" + s);
        PlayerPrefs.DeleteKey("CurrentLifePieces" + s);
        PlayerPrefs.DeleteKey("CurrentMapNodeNumber" + s);
        PlayerPrefs.DeleteKey("UnlockedWorldLevel" + s);
        PlayerPrefs.DeleteKey("CurrentWorldNodeNumber" + s);
        PlayerPrefs.DeleteKey("OwnedItemIds" + s);
        PlayerPrefs.DeleteKey("HpUpPurchaseCount" + s);
        PlayerPrefs.DeleteKey("SpUpPurchaseCount" + s);
        PlayerPrefs.Save();
    }

    // ▼▼▼ 新規追加：そのスロットにデータがあるか確認する（タイトル画面用） ▼▼▼
    public static bool HasSaveData(int slot)
    {
        return PlayerPrefs.HasKey("IsSaved_" + slot);
    }
}