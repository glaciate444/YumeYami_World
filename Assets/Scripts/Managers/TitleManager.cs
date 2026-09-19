/* ===================================================
 * スクリプト名 : TitleManager.cs
 * 用途 : タイトル画面の演出、状態遷移、メニュー選択
 * 拡張 : PlayerControls完全対応（コントローラー＆キーボード両対応版）
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public enum TitleState{
    PressAnyKey,
    MainMenu,
    FileMenu,
    Options,
    Credits
}

public class TitleManager : MonoBehaviour{
    [Header("状態管理")]
    public TitleState currentState = TitleState.PressAnyKey;

    [Header("UIパネル設定")]
    public GameObject pressAnyKeyPanel;
    public GameObject mainPanel;
    public GameObject fileMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("メインメニュー設定")]
    public RectTransform cursorImage;
    [Tooltip("上から順に: 0:Data1, 1:Data2, 2:Data3, 3:Data4, 4:Option, 5:Credit")]
    public RectTransform[] menuPositions;
    public float cursorOffsetX = 150f;

    [Header("ファイルテキスト設定")]
    public TMP_Text[] fileTexts;

    [Header("サブメニュー設定")]
    public RectTransform subMenuCursor;
    [Tooltip("0:ゲームスタート, 1:ファイルを消す")]
    public RectTransform[] subMenuPositions;
    public float subMenuCursorOffsetX = 80f;

    private int currentIndex = 0;
    private int subMenuIndex = 0;
    private int selectedSlot = 1;

    // ▼ 新規追加：Input Actionのクラスとクールダウン
    private PlayerControls input;
    private float inputCooldown = 0f;

    private readonly int[,] navigation = new int[6, 4] {
        { 4, 2, 1, 1 },
        { 5, 3, 0, 0 },
        { 0, 4, 3, 3 },
        { 1, 5, 2, 2 },
        { 2, 0, 5, 5 },
        { 3, 1, 4, 4 }
    };

    void Awake(){
        input = new PlayerControls();
    }

    void OnEnable(){
        input.Enable();
    }

    void OnDisable(){
        input.Disable();
    }

    void Start(){
        ChangeState(TitleState.PressAnyKey);
    }

    void Update(){
        if (inputCooldown > 0f){
            inputCooldown -= Time.unscaledDeltaTime;
        }

        // ▼ PlayerControls からの入力取得
        Vector2 moveDir = input.Player.Move.ReadValue<Vector2>();
        bool isUp = false, isDown = false, isLeft = false, isRight = false;

        // カーソル移動のクールダウン管理
        if (inputCooldown <= 0f && moveDir.sqrMagnitude > 0.1f){
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)){
                if (moveDir.x > 0.5f) isRight = true;
                else if (moveDir.x < -0.5f) isLeft = true;
            }else{
                if (moveDir.y > 0.5f) isUp = true;
                else if (moveDir.y < -0.5f) isDown = true;
            }
        }

        bool isSubmit = input.Player.Attack.WasPressedThisFrame() || input.Player.Jump.WasPressedThisFrame();
        bool isCancel = input.Player.Dash.WasPressedThisFrame() || input.Player.Pause.WasPressedThisFrame();

        // PressAnyKey用の「何らかのアクションが入力されたか」判定
        bool isAnyAction = isUp || isDown || isLeft || isRight || isSubmit || isCancel ||
                           (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame);

        switch (currentState){
            case TitleState.PressAnyKey:
                if (isAnyAction){
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;

            case TitleState.MainMenu:
                HandleMainMenuInput(isUp, isDown, isLeft, isRight, isSubmit);
                break;

            case TitleState.FileMenu:
                HandleFileMenuInput(isUp, isDown, isSubmit, isCancel);
                break;

            case TitleState.Options:
                if (isCancel){
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;

            case TitleState.Credits:
                if (isCancel || isSubmit){
                    ChangeState(TitleState.MainMenu);
                    inputCooldown = 0.2f;
                }
                break;
        }
    }

    private void HandleMainMenuInput(bool isUp, bool isDown, bool isLeft, bool isRight, bool isSubmit){
        bool moved = false;
        if (isUp){
            currentIndex = navigation[currentIndex, 0];
            moved = true;
        }else if (isDown){
            currentIndex = navigation[currentIndex, 1];
            moved = true;
        }else if (isLeft){
            currentIndex = navigation[currentIndex, 2];
            moved = true;
        }else if (isRight){
            currentIndex = navigation[currentIndex, 3];
            moved = true;
        }

        if (moved){
            UpdateCursorPosition();
            inputCooldown = 0.2f;
        }

        if (isSubmit){
            ExecuteMainMenu();
        }
    }

    private void HandleFileMenuInput(bool isUp, bool isDown, bool isSubmit, bool isCancel){
        if (isUp || isDown){
            subMenuIndex = (subMenuIndex == 0) ? 1 : 0;
            UpdateSubMenuCursorPosition();
            inputCooldown = 0.2f;
        }

        if (isSubmit){
            if (subMenuIndex == 0){
                GameManager.Instance.currentSaveSlot = selectedSlot;
                GameManager.Instance.LoadGame();
                SceneTransitionManager.Instance.LoadScene(SceneNames.WorldMap);
            }else{
                GameManager.Instance.DeleteSaveData(selectedSlot);
                ChangeState(TitleState.MainMenu);
            }
        }

        if (isCancel){
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

    private void UpdateFileTexts(){
        for (int i = 0; i < 4; i++){
            if (fileTexts != null && i < fileTexts.Length && fileTexts[i] != null){
                int slot = i + 1;
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
        if (currentIndex >= 0 && currentIndex <= 3){
            selectedSlot = currentIndex + 1;
            if (GameManager.HasSaveData(selectedSlot)){
                ChangeState(TitleState.FileMenu);
            }else{
                GameManager.Instance.currentSaveSlot = selectedSlot;
                GameManager.Instance.ResetData();
                GameManager.Instance.SaveGame();
                SceneTransitionManager.Instance.LoadScene(SceneNames.Opening);
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
                UpdateFileTexts();
                UpdateCursorPosition();
                break;
            case TitleState.FileMenu:
                if (mainPanel) mainPanel.SetActive(true);
                if (fileMenuPanel) fileMenuPanel.SetActive(true);
                subMenuIndex = 0;
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