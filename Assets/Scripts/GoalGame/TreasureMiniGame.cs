/* ===================================================
 * スクリプト名 : TreasureMiniGame.cs
 * 用途 : 8つの宝箱から2つを選んで開けるミニゲーム (Input System対応版)
 * 修正 : ShuffleRewardsの追加と、GameManagerのAddLifePiece連携に対応
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

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
    [Tooltip("0〜7: 宝箱のボタン, 8: スキップボタン の順にセットします(合計9個)")]
    public RectTransform[] menuPositions; 
    [Tooltip("カーソルをボタンからどれくらいズラすか")]
    public Vector2 cursorOffset = new Vector2(0f, 50f);

    [Header("遷移先")]
    public string resultSceneName = "ResultScene"; 

    private int openedCount = 0;
    private const int MAX_OPENS = 2;
    private bool isGameOver = false; 

    private int currentIndex = 0;
    private float inputCooldown = 0f;

    private readonly int[,] navigation = new int[9, 4] {
        { 0, 4, 0, 1 }, 
        { 1, 5, 0, 2 }, 
        { 2, 6, 1, 3 }, 
        { 3, 7, 2, 3 }, 
        { 0, 8, 4, 5 }, 
        { 1, 8, 4, 6 }, 
        { 2, 8, 5, 7 }, 
        { 3, 8, 6, 7 }, 
        { 5, 8, 8, 8 }  
    };

    void Start() {
        // ▼【修正】ここで下のシャッフルメソッドを呼び出します
        ShuffleRewards();

        for (int i = 0; i < chestButtons.Length; i++) {
            int index = i; 
            chestButtons[i].onClick.AddListener(() => OnClickChest(index));
        }

        UpdateCursorPosition();
    }

    // ▼【追加・修正】前回抜け落ちていたシャッフル用のメソッド
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
            
            // 1. 【超重要】「親が違うUI」でも位置を合わせるため、ワールド座標(.position)を直接コピーする
            cursorImage.position = menuPositions[currentIndex].position;
            
            // 2. その後、インスペクターで設定したズレ（オフセット）をローカルで足す
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

        ApplyReward(rewards[chestIndex]);

        if (openedCount >= MAX_OPENS) {
            isGameOver = true; 
            StartCoroutine(WaitAndGoToResult());
        }
    }

    private void ApplyReward(RewardData reward) {
        if (GameManager.Instance == null) return;

        if (reward.isCoin) {
            // ▼【修正】トータルではなく、ゴールから引き継いだステージコインを増減させる
            GameManager.Instance.stageCoins += reward.amount;
            
            // コインがマイナスになったら0にする
            if (GameManager.Instance.stageCoins < 0) GameManager.Instance.stageCoins = 0;
            
            Debug.Log($"ステージコインが {reward.amount} 枚変化しました！ 現在: {GameManager.Instance.stageCoins}枚");
        } else {
            // 残基パーツの獲得はそのままGameManagerに加算
            GameManager.Instance.AddLifePiece(reward.amount);
            Debug.Log($"LifePieceが {reward.amount} 個変化しました！");
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