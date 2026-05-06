/* ===================================================
 * スクリプト名 : TitleManager.cs
 * Version : Ver0.04
 * Since : 2026/04/27
 * Update : 2026/05/06
 * 用途 : タイトル画面の演出、状態遷移、メニュー選択を担当します。
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public enum TitleState {
    PressAnyKey, 
    MainMenu,    
    Options,     
    Credits      
}

public class TitleManager : MonoBehaviour{
    [Header("状態管理")]
    public TitleState currentState = TitleState.PressAnyKey;

    [Header("UIパネル設定")]
    public GameObject pressAnyKeyPanel; 
    public GameObject mainPanel;        
    public GameObject optionsPanel;     
    public GameObject creditsPanel;     

    [Header("メインメニュー設定")]
    public RectTransform cursorImage;
    [Tooltip("上から順に: 0:NewGame, 1:Data1, 2:Data2, 3:Data3, 4:Data4, 5:Option, 6:Credit")]
    public RectTransform[] menuPositions; 
    
    [Tooltip("カーソルをボタンの左側どれくらい離れた位置に置くか（数値で調整してください）")]
    public float cursorOffsetX = 150f;

    private int currentIndex = 0;
    private float inputCooldown = 0f; 

    // ▼【追加】上下左右に動かした時の「移動先」の地図（ナビゲーション）
    // 配列の中身： { 上を押した時の行先, 下, 左, 右 }
    private readonly int[,] navigation = new int[7, 4] {
        { 5, 1, 0, 0 }, // 0: NewGame (上->Option, 下->Data1, 左右->移動なし)
        { 0, 3, 2, 2 }, // 1: Data1   (上->NewGame, 下->Data3, 左右->Data2)
        { 0, 4, 1, 1 }, // 2: Data2   (上->NewGame, 下->Data4, 左右->Data1)
        { 1, 5, 4, 4 }, // 3: Data3   (上->Data1, 下->Option, 左右->Data4)
        { 2, 6, 3, 3 }, // 4: Data4   (上->Data2, 下->Credit, 左右->Data3)
        { 3, 0, 6, 6 }, // 5: Option  (上->Data3, 下->NewGame(ループ), 左右->Credit)
        { 4, 0, 5, 5 }  // 6: Credit  (上->Data4, 下->NewGame(ループ), 左右->Option)
    };

    void Start(){
        ChangeState(TitleState.PressAnyKey);
    }

    void Update(){
        if (inputCooldown > 0f){
            inputCooldown -= Time.deltaTime;
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        switch (currentState){
            case TitleState.PressAnyKey:
                if (keyboard.anyKey.wasPressedThisFrame){
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;

            case TitleState.MainMenu:
                HandleMainMenuInput(keyboard);
                break;

            case TitleState.Options:
                if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame){
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;

            case TitleState.Credits:
                if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame || 
                    keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;
        }
    }

    private void HandleMainMenuInput(Keyboard keyboard){
        // ▼【変更】地図（navigation配列）を見て、上下左右の移動先を決定する
        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame){
            currentIndex = navigation[currentIndex, 0];
            UpdateCursorPosition();
        }else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame){
            currentIndex = navigation[currentIndex, 1];
            UpdateCursorPosition();
        }else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame){
            currentIndex = navigation[currentIndex, 2];
            UpdateCursorPosition();
        }else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame){
            currentIndex = navigation[currentIndex, 3];
            UpdateCursorPosition();
        }

        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame){
            ExecuteMenu();
        }
    }

    private void UpdateCursorPosition(){
        if (menuPositions.Length > 0 && cursorImage != null && menuPositions[currentIndex] != null){
            // ▼【変更】Y座標だけでなく、X座標もボタンに合わせて動かす
            Vector2 newPos = menuPositions[currentIndex].anchoredPosition;
            
            // X座標をボタンの位置から左（マイナス方向）にズラす
            newPos.x -= cursorOffsetX; 
            
            cursorImage.anchoredPosition = newPos;
        }
    }

    private void ExecuteMenu(){
        inputCooldown = 0.2f;
        switch (currentIndex){
            case 0: 
                Debug.Log("ニューゲームを開始します！");
                SceneManager.LoadScene("MapSelectScene");
                break;
            case 1: case 2: case 3: case 4: 
                Debug.Log($"セーブデータ {currentIndex} をロードします！");
                break;
            case 5: 
                ChangeState(TitleState.Options);
                break;
            case 6: 
                ChangeState(TitleState.Credits);
                break;
        }
    }

    private void ChangeState(TitleState newState){
        currentState = newState;

        if (pressAnyKeyPanel) pressAnyKeyPanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);

        switch (currentState){
            case TitleState.PressAnyKey:
                if (pressAnyKeyPanel) pressAnyKeyPanel.SetActive(true);
                break;
            case TitleState.MainMenu:
                if (mainPanel) mainPanel.SetActive(true);
                UpdateCursorPosition(); 
                break;
            case TitleState.Options:
                if (optionsPanel) optionsPanel.SetActive(true);
                break;
            case TitleState.Credits:
                if (creditsPanel) creditsPanel.SetActive(true);
                break;
        }
    }
}