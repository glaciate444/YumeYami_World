/* ===================================================
 * スクリプト名 : GameOverManager.cs
 * 用途 : ゲームオーバー画面での選択肢の処理 (Input System対応版)
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // ← 新しいInputSystemを使用

public class GameOverManager : MonoBehaviour {

    [Header("遷移先設定")]
    public string mapSceneName = "MapSelectScene"; 
    public string titleSceneName = "TitleScene";   

    [Header("UIナビゲーション設定")]
    public RectTransform cursorImage;       // 選択カーソル（指マークやアイコンなど）
    [Tooltip("0:コンティニュー, 1:タイトルへ の順番でボタンをセット")]
    public RectTransform[] menuPositions;   
    public float cursorOffsetX = 150f;      // カーソルを左にどれくらいズラすか

    private int currentIndex = 0;           // 0 = Continue, 1 = Title
    private float inputCooldown = 0.2f;     // 画面切り替え直後のボタン誤爆防止

    void Start() {
        UpdateCursorPosition();
    }

    void Update() {
        if (inputCooldown > 0f) {
            inputCooldown -= Time.deltaTime;
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ▼ 上下キー（またはW・Sキー）で選択を切り替え
        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame ||
            keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) {
            
            // 選択肢が2つだけなので、上を押しても下を押しても 0 と 1 が入れ替わるようにする
            currentIndex = (currentIndex == 0) ? 1 : 0;
            UpdateCursorPosition();
        }

        // ▼ 決定キー（Z, Enter, Space）
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) {
            ExecuteMenu();
        }
    }

    private void UpdateCursorPosition() {
        // カーソル画像とボタンが正しくセットされていれば、指定位置に移動させる
        if (menuPositions.Length > 0 && cursorImage != null && menuPositions[currentIndex] != null) {
            Vector2 newPos = menuPositions[currentIndex].anchoredPosition;
            newPos.x -= cursorOffsetX; // 左に少しズラす
            cursorImage.anchoredPosition = newPos;
        }
    }

    private void ExecuteMenu() {
        inputCooldown = 999f; // 連打防止のためにキー入力をロックする

        if (currentIndex == 0) {
            OnClickContinue();
        } else {
            OnClickToTitle();
        }
    }

    // ==========================================
    // ▼ 実際の処理内容（マウスクリックからも呼べます）
    // ==========================================
    public void OnClickContinue() {
        if (GameManager.Instance != null) {
            GameManager.Instance.currentLives = 3; // 残基復活
            GameManager.Instance.SaveGame();
        }

        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadScene(mapSceneName, TransitionType.Fade);
        } else {
            SceneManager.LoadScene(mapSceneName);
        }
    }

    public void OnClickToTitle() {
        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadScene(titleSceneName, TransitionType.Fade);
        } else {
            SceneManager.LoadScene(titleSceneName);
        }
    }
}