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
    public GameObject mainPanel;

    [Header("オプション画面")]
    public GameObject optionsPanel;

    private int currentIndex = 0;
    private bool isOptionsOpen = false;

    // 【追加】入力の貫通を防ぐためのクールタイム（待ち時間）
    private float inputCooldown = 0f;

    void Start(){
        CloseOptions();
        UpdateCursorPosition();
    }

    void Update(){
        // ▼ クールタイム中は一切のキーボード入力を無視してリターンする ▼
        if (inputCooldown > 0f){
            inputCooldown -= Time.deltaTime;
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (isOptionsOpen){
            if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame){
                CloseOptions();
            }
            return;
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
        if (menuPositions.Length > 0 && cursorImage != null && menuPositions[currentIndex] != null){
            Vector2 newPos = cursorImage.anchoredPosition;
            newPos.y = menuPositions[currentIndex].anchoredPosition.y;
            cursorImage.anchoredPosition = newPos;
        }
    }

    private void ExecuteMenu(){
        switch (currentIndex){
            case 0:
                SceneManager.LoadScene("MapSelectScene");
                break;
            case 1:
                Debug.Log("ロード機能は後ほど実装します！");
                break;
            case 2:
                OpenOptions();
                break;
        }
    }

    public void OpenOptions(){
        isOptionsOpen = true;
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);

        inputCooldown = 0.2f; // 開いた直後も0.2秒間入力を無視する
    }

    public void CloseOptions(){
        isOptionsOpen = false;
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
        UpdateCursorPosition();

        inputCooldown = 0.2f; // 閉じた直後も0.2秒間入力を無視する（これで貫通を完全ガード！）
    }
}