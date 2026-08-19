/* ===================================================
 * スクリプト名 : TitleManager.cs
 * 用途 : タイトル画面の演出、状態遷移、メニュー選択
 * 拡張 : セーブデータ多重スロット対応＆サブメニュー化
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro; // テキスト操作用

// ▼ 追加：サブメニュー用の状態（FileMenu）を追加
public enum TitleState {
    PressAnyKey,
    MainMenu,
    FileMenu,    // ファイルを選択した後の「スタート/消す」メニュー
    Options,
    Credits
}

public class TitleManager : MonoBehaviour {
    [Header("状態管理")]
    public TitleState currentState = TitleState.PressAnyKey;

    [Header("UIパネル設定")]
    public GameObject pressAnyKeyPanel;
    public GameObject mainPanel;
    public GameObject fileMenuPanel;    // ▼ 追加：ファイル選択後のサブメニュー枠
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("メインメニュー設定")]
    public RectTransform cursorImage;
    [Tooltip("上から順に: 0:Data1, 1:Data2, 2:Data3, 3:Data4, 4:Option, 5:Credit")]
    public RectTransform[] menuPositions;

    [Tooltip("カーソルをボタンの左側どれくらい離れた位置に置くか")]
    public float cursorOffsetX = 150f;

    [Header("ファイルテキスト設定")]
    [Tooltip("ファイル1〜4のテキスト（データ有無の表示切り替え用）")]
    public TMP_Text[] fileTexts;

    [Header("サブメニュー設定")]
    public RectTransform subMenuCursor;
    [Tooltip("0:ゲームスタート, 1:ファイルを消す")]
    public RectTransform[] subMenuPositions;
    public float subMenuCursorOffsetX = 80f;

    private int currentIndex = 0;
    private int subMenuIndex = 0;     // サブメニューのカーソル位置
    private int selectedSlot = 1;     // 選んだファイル番号（1〜4）
    private float inputCooldown = 0f;

    // ▼ 修正：「ニューゲーム」を廃止し、6つのボタン用のナビゲーションに再構築
    // 配列の中身： { 上, 下, 左, 右 }
    private readonly int[,] navigation = new int[6, 4] {
        { 4, 2, 1, 1 }, // 0: Data1  (上->Option, 下->Data3, 左右->Data2)
        { 5, 3, 0, 0 }, // 1: Data2  (上->Credit, 下->Data4, 左右->Data1)
        { 0, 4, 3, 3 }, // 2: Data3  (上->Data1,  下->Option, 左右->Data4)
        { 1, 5, 2, 2 }, // 3: Data4  (上->Data2,  下->Credit, 左右->Data3)
        { 2, 0, 5, 5 }, // 4: Option (上->Data3,  下->Data1,  左右->Credit)
        { 3, 1, 4, 4 }  // 5: Credit (上->Data4,  下->Data2,  左右->Option)
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
                if (keyboard.anyKey.wasPressedThisFrame)
                {
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;

            case TitleState.MainMenu:
                HandleMainMenuInput(keyboard);
                break;

            case TitleState.FileMenu:
                HandleFileMenuInput(keyboard);
                break;

            case TitleState.Options:
                if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                {
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;

            case TitleState.Credits:
                if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame ||
                    keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
                {
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;
        }
    }

    private void HandleMainMenuInput(Keyboard keyboard){
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
            ExecuteMainMenu();
        }
    }

    private void HandleFileMenuInput(Keyboard keyboard){
        // サブメニュー（ゲームスタート / 消す）の上下移動
        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame ||
            keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame){
            subMenuIndex = (subMenuIndex == 0) ? 1 : 0; // 0と1を切り替える
            UpdateSubMenuCursorPosition();
        }

        // サブメニューの決定
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
            inputCooldown = 0.2f;
            if (subMenuIndex == 0){
                // 「ゲームスタート」を選んだ場合
                GameManager.Instance.currentSaveSlot = selectedSlot;
                GameManager.Instance.LoadGame();
                SceneTransitionManager.Instance.LoadScene("MapSelectScene_Level_1"); // ※続きから始まるシーン
            }else{
                // 「ファイルを消す」を選んだ場合
                GameManager.Instance.DeleteSaveData(selectedSlot);
                ChangeState(TitleState.MainMenu); // 消したらメインメニューに戻る
            }
        }

        // キャンセル（Xキー）でメインメニューに戻る
        if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame){
            ChangeState(TitleState.MainMenu);
            inputCooldown = 0.2f;
        }
    }

    private void UpdateCursorPosition(){
        if (menuPositions.Length > 0 && cursorImage != null && menuPositions[currentIndex] != null){
            Vector2 newPos = menuPositions[currentIndex].anchoredPosition;
            newPos.x -= cursorOffsetX;
            cursorImage.anchoredPosition = newPos;
        }
    }

    private void UpdateSubMenuCursorPosition(){
        if (subMenuPositions.Length > 0 && subMenuCursor != null && subMenuPositions[subMenuIndex] != null){
            Vector2 newPos = subMenuPositions[subMenuIndex].anchoredPosition;
            newPos.x -= subMenuCursorOffsetX;
            subMenuCursor.anchoredPosition = newPos;
        }
    }

    // データがあるかどうかを調べてUIテキストを書き換える
    private void UpdateFileTexts(){
        for (int i = 0; i < 4; i++){
            if (fileTexts != null && i < fileTexts.Length && fileTexts[i] != null){
                int slot = i + 1; // 配列0=ファイル1
                if (GameManager.HasSaveData(slot)){
                    fileTexts[i].text = $"ファイル {slot}\n(つづきから)";
                }else{
                    fileTexts[i].text = $"ファイル {slot}\n(あたらしくはじめる)";
                }
            }
        }
    }

    private void ExecuteMainMenu(){
        inputCooldown = 0.2f;

        // ファイル1〜4を選んだ場合
        if (currentIndex >= 0 && currentIndex <= 3){
            selectedSlot = currentIndex + 1; // ファイル番号（1〜4）

            if (GameManager.HasSaveData(selectedSlot)){
                // データがある場合はサブメニューを開く
                ChangeState(TitleState.FileMenu);
            }else{
                // データがない（空きスロット）場合は、即座にニューゲーム！
                GameManager.Instance.currentSaveSlot = selectedSlot;
                GameManager.Instance.ResetData();
                GameManager.Instance.SaveGame(); // 空のファイルを作成
                SceneTransitionManager.Instance.LoadScene("OpeningScene");
            }
        }else if (currentIndex == 4){
            ChangeState(TitleState.Options);
        }else if (currentIndex == 5){
            ChangeState(TitleState.Credits);
        }
    }

    private void ChangeState(TitleState newState){
        currentState = newState;

        if (pressAnyKeyPanel) pressAnyKeyPanel.SetActive(false);
        // サブメニューの時はメインパネルを裏に残しておく
        if (mainPanel && newState != TitleState.FileMenu) mainPanel.SetActive(false);
        if (fileMenuPanel) fileMenuPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (cursorImage) cursorImage.gameObject.SetActive(false);

        switch (currentState){
            case TitleState.PressAnyKey:
                if (pressAnyKeyPanel) pressAnyKeyPanel.SetActive(true);
                break;
            case TitleState.MainMenu:
                if (mainPanel) mainPanel.SetActive(true);
                if (cursorImage) cursorImage.gameObject.SetActive(true);
                UpdateFileTexts(); // メインメニューに戻るたびに「データあり/なし」を最新化
                UpdateCursorPosition();
                break;
            case TitleState.FileMenu:
                if (mainPanel) mainPanel.SetActive(true);
                if (fileMenuPanel) fileMenuPanel.SetActive(true);
                subMenuIndex = 0; // カーソル位置を「ゲームスタート」にリセット
                UpdateSubMenuCursorPosition();
                break;
            case TitleState.Options:
                if (optionsPanel) optionsPanel.SetActive(true);
                break;
            case TitleState.Credits:
                if (creditsPanel) creditsPanel.SetActive(true);
                break;
        }
    }

    public void CloseOptions(){
        ChangeState(TitleState.MainMenu);
        inputCooldown = 0.2f;
    }

    public void CloseCredits(){
        ChangeState(TitleState.MainMenu);
        inputCooldown = 0.2f;
    }
}