/* ===================================================
 * スクリプト名 : TreasureMiniGame.cs
 * 用途 : 8つの宝箱から2つを選んで開けるミニゲーム (完全版)
 * 拡張 : HUDの自動更新、残り回数表示、ポップアップ演出の追加
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using TMPro; // HUDの文字を書き換えるために追加

public class TreasureMiniGame : MonoBehaviour {
    [System.Serializable]
    public class RewardData {
        public bool isCoin; 
        public int amount;  
    }

    [Header("宝箱の設定")]
    public Button[] chestButtons;       
    public Sprite openChestSprite;      
    public List<RewardData> rewards;    

    [Header("UIナビゲーション設定")]
    public RectTransform cursorImage;
    public RectTransform[] menuPositions; 
    public Vector2 cursorOffset = new Vector2(0f, 0f);

    [Header("遷移先")]
    public string resultSceneName = "ResultScene"; 

    // ▼【新規追加】HUDとポップアップ用の設定
    [Header("HUD連携")]
    public TextMeshProUGUI hudCoinText;     // コインの数字
    public TextMeshProUGUI hudHeartText;    // ハートの数字
    public TextMeshProUGUI hudLivesText;    // 残基の数字
    public TextMeshProUGUI remainingOpensText; // 残り開けられる回数（画面右上の「2」など）

    [Header("ポップアップ演出")]
    public GameObject rewardPopupPrefab;    // Step1で作るプレハブ
    public Sprite coinIconSprite;           // コインのアイコン画像
    public Sprite heartIconSprite;          // ハートのアイコン画像

    private int openedCount = 0;
    private const int MAX_OPENS = 2;
    private bool isGameOver = false; 

    private int currentIndex = 0;
    private float inputCooldown = 0f;

    private readonly int[,] navigation = new int[9, 4] {
        { 0, 4, 0, 1 }, { 1, 5, 0, 2 }, { 2, 6, 1, 3 }, { 3, 7, 2, 3 }, 
        { 0, 8, 4, 5 }, { 1, 8, 4, 6 }, { 2, 8, 5, 7 }, { 3, 8, 6, 7 }, 
        { 5, 8, 8, 8 }  
    };

    void Start() {
        // ▼【追加】設定の数が足りない時に、日本語で警告を出す安全装置
        if (rewards.Count < chestButtons.Length) {
            Debug.LogError($"【設定エラー】Rewards（中身）の数が足りません！宝箱が {chestButtons.Length} 個あるのに対し、Rewardsが {rewards.Count} 個しかありません。");
            return; // エラー回避のためここで止める
        }
        if (menuPositions.Length < 9) {
            Debug.LogError($"【設定エラー】Menu Positions の数が足りません！宝箱8個＋スキップ1個の合計 9個 設定してください。");
            return; 
        }
        // ▲ 安全装置ここまで

        ShuffleRewards();

        for (int i = 0; i < chestButtons.Length; i++) {
            if (chestButtons[i] == null) {
                Debug.LogError($"【設定エラー】Chest Buttons の Element {i} が空っぽ(None)です！");
                continue; 
            }
            int index = i; 
            chestButtons[i].onClick.AddListener(() => OnClickChest(index));
        }

        UpdateCursorPosition();
        UpdateHUD(); 
    }

    private void ShuffleRewards() {
        for (int i = 0; i < rewards.Count; i++) {
            RewardData temp = rewards[i];
            int randomIndex = Random.Range(i, rewards.Count);
            rewards[i] = rewards[randomIndex];
            rewards[randomIndex] = temp;
        }
    }

    void Update() {
        if (isGameOver) return;

        if (inputCooldown > 0f) {
            inputCooldown -= Time.deltaTime;
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool moved = false;
        
        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) {
            currentIndex = navigation[currentIndex, 0];
            moved = true;
        } else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) {
            currentIndex = navigation[currentIndex, 1];
            moved = true;
        } else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) {
            currentIndex = navigation[currentIndex, 2];
            moved = true;
        } else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) {
            currentIndex = navigation[currentIndex, 3];
            moved = true;
        }

        if (moved) {
            UpdateCursorPosition();
            inputCooldown = 0.15f; 
        }

        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) {
            ExecuteMenu();
        }
    }

    private void UpdateCursorPosition() {
        if (menuPositions.Length > 0 && cursorImage != null && menuPositions[currentIndex] != null) {
            cursorImage.position = menuPositions[currentIndex].position;
            cursorImage.anchoredPosition += cursorOffset; 
        }
    }

    private void ExecuteMenu() {
        inputCooldown = 0.2f;

        if (currentIndex == 8) {
            OnClickSkip();
        } else {
            if (chestButtons[currentIndex].interactable) {
                OnClickChest(currentIndex);
            }
        }
    }

    public void OnClickChest(int chestIndex) {
        if (openedCount >= MAX_OPENS || isGameOver) return;

        openedCount++;
        Button clickedChest = chestButtons[chestIndex];

        if (openChestSprite != null) {
            clickedChest.image.sprite = openChestSprite;
        }
        clickedChest.interactable = false; 

        ApplyReward(rewards[chestIndex], clickedChest); // 宝箱の場所を渡す

        if (openedCount >= MAX_OPENS) {
            isGameOver = true; 
            StartCoroutine(WaitAndGoToResult());
        }
    }

    private void ApplyReward(RewardData reward, Button chestBtn) {
        // ▼【修正】ポップアップはGameManagerが無くても出せるように、チェックの前に引っ越します！
        ShowPopup(chestBtn, reward);

        // ▼【修正】GameManagerが必要な計算だけを、この「ifの中」に隔離します
        if (GameManager.Instance != null) {
            if (reward.isCoin) {
                GameManager.Instance.stageCoins += reward.amount;
                if (GameManager.Instance.stageCoins < 0) GameManager.Instance.stageCoins = 0;
            } else {
                GameManager.Instance.AddLifePiece(reward.amount);
            }
        } else {
            // GameManagerが無い時は、コンソールにログだけ残す（テスト用）
            Debug.LogWarning($"【テストモード】GameManagerが無いため、数値の内部保存はスキップされます。(中身: {(reward.isCoin ? "コイン" : "LifePiece")} / 量: {reward.amount})");
        }

        // ▼【修正】GameManagerが無くても、残り開けられる回数（右上）などを更新するために必ず呼ぶ
        UpdateHUD();
    }

    // ▼ HUDの文字を書き換える処理
    private void UpdateHUD() {
        if (GameManager.Instance != null) {
            if (hudCoinText != null) hudCoinText.text = GameManager.Instance.stageCoins.ToString("D3");
            if (hudHeartText != null) hudHeartText.text = GameManager.Instance.currentLifePieces.ToString("D2");
            if (hudLivesText != null) hudLivesText.text = GameManager.Instance.currentLives.ToString("D2");
        }

        if (remainingOpensText != null) {
            // MAX(2) - 開けた数 = 残り回数
            remainingOpensText.text = (MAX_OPENS - openedCount).ToString();
        }
    }

    // ▼ ポップアッププレハブを生成する処理
    private void ShowPopup(Button chest, RewardData reward) {
        if (rewardPopupPrefab == null) return;

        // 1. 親となる Canvas を探す
        Canvas parentCanvas = GetComponentInParent<Canvas>();

        // 2. 宝箱の中ではなく、Canvas の直下に生成する（Grid Layoutの干渉も防げます）
        GameObject popupObj = Instantiate(rewardPopupPrefab, parentCanvas.transform);

        // 3. 生成場所を、叩いた宝箱と全く同じ位置（ワールド座標）に合わせる
        popupObj.transform.position = chest.transform.position;

        // 4. 【超重要】ヒエラルキーの「一番下」に移動させ、全てのUIの「一番手前」に表示させる！
        popupObj.transform.SetAsLastSibling();

        RewardPopup popup = popupObj.GetComponent<RewardPopup>();
        if (popup != null) {
            Sprite icon = reward.isCoin ? coinIconSprite : heartIconSprite;
            popup.Setup(icon, reward.amount);
        }
    }

    public void OnClickSkip() {
        if (isGameOver) return;
        isGameOver = true;
        GoToResult();
    }

    private IEnumerator WaitAndGoToResult() {
        yield return new WaitForSeconds(2.0f);
        GoToResult();
    }

    private void GoToResult() {
        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadScene(resultSceneName, TransitionType.Fade);
        } else {
            SceneManager.LoadScene(resultSceneName);
        }
    }
}