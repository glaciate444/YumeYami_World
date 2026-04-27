/* ===================================================
 * スクリプト名 : TitleManager.cs
 * Version : Ver0.02
 * Since : 2026/04/27
 * Update : 2026/04/27
 * 用途 : タイトル画面の演出や、ボタンを押した時の画面遷移だけを担当する使い捨てのスクリプトです。
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour{
    [Header("UI設定")]
    public RectTransform cursorImage;
    public RectTransform[] menuPositions;
    public GameObject mainPanel;      // 【追加】メインメニューのパネル

    [Header("オプション画面")]
    public GameObject optionsPanel;

    private int currentIndex = 0;
    private bool isOptionsOpen = false;

    void Start(){
        // 最初はオプションを閉じ、メインを表示しておく
        CloseOptions();
        UpdateCursorPosition();
    }

    void Update(){
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ▼ オプション画面が開いている時の処理 ▼
        if (isOptionsOpen){
            // Xキー または Escキーで閉じる
            if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame){
                CloseOptions(); // 【変更】閉じる処理をメソッド化
            }
            return; // ここで処理を止める
        }

        // ▼ 通常のメニュー操作 ▼
        if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame){
            currentIndex++;
            if (currentIndex >= menuPositions.Length) currentIndex = 0;
            UpdateCursorPosition();
        }else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame){
            currentIndex--;
            if (currentIndex < 0) currentIndex = menuPositions.Length - 1;
            UpdateCursorPosition();
        }

        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame){
            ExecuteMenu();
        }
    }

    private void UpdateCursorPosition(){
        if (menuPositions.Length > 0 && cursorImage != null){
            Vector2 newPos = cursorImage.anchoredPosition;
            newPos.y = menuPositions[currentIndex].anchoredPosition.y;
            cursorImage.anchoredPosition = newPos;
        }
    }

    private void ExecuteMenu(){
        switch (currentIndex){
            case 0: // Game Start
                SceneManager.LoadScene("MapSelectScene");
                break;
            case 1: // Load
                Debug.Log("ロード機能は後ほど実装します！");
                break;
            case 2: // Options
                OpenOptions(); // 【変更】開く処理をメソッド化
                break;
        }
    }

    // --- 【追加】開く・閉じる専用のメソッド（外部から呼べるように public にする） ---

    public void OpenOptions(){
        isOptionsOpen = true;
        mainPanel.SetActive(false);   // メインを隠す
        optionsPanel.SetActive(true); // オプションを表示
    }

    public void CloseOptions(){
        isOptionsOpen = false;
        optionsPanel.SetActive(false); // オプションを隠す
        mainPanel.SetActive(true);     // メインを再表示

        // メインに戻った時、カーソルの位置を再同期する
        UpdateCursorPosition();
    }
}