/* ===================================================
 * スクリプト名 : GameOverManager.cs
 * 用途 : ゲームオーバー画面での選択肢の処理
 * 拡張 : PlayerControls完全対応
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class GameOverManager : MonoBehaviour {

    [Header("遷移先設定")]
    public string mapSceneName = "MapSelectScene"; 
    public string titleSceneName = "TitleScene";   

    [Header("UIナビゲーション設定")]
    public RectTransform cursorImage;       
    public RectTransform[] menuPositions;   
    public float cursorOffsetX = 150f;      

    private int currentIndex = 0;           
    private float inputCooldown = 0.2f;     

    // ▼ 新規追加
    private PlayerControls input;

    void Awake() {
        input = new PlayerControls();
    }

    void OnEnable() {
        input.Enable();
    }

    void OnDisable() {
        input.Disable();
    }

    void Start() {
        UpdateCursorPosition();
    }

    void Update() {
        if (inputCooldown > 0f) {
            inputCooldown -= Time.unscaledDeltaTime;
        }

        // ▼ PlayerControls対応
        Vector2 moveDir = input.Player.Move.ReadValue<Vector2>();
        
        // どの方向でもいいので入力を検知したら切り替える
        if (inputCooldown <= 0f && moveDir.sqrMagnitude > 0.1f) {
            currentIndex = (currentIndex == 0) ? 1 : 0;
            UpdateCursorPosition();
            inputCooldown = 0.2f;
        }

        bool isSubmit = input.Player.Attack.WasPressedThisFrame() || input.Player.Jump.WasPressedThisFrame();
        
        if (isSubmit) {
            ExecuteMenu();
        }
    }

    private void UpdateCursorPosition() {
        if (menuPositions.Length > 0 && cursorImage != null && menuPositions[currentIndex] != null) {
            Vector2 newPos = menuPositions[currentIndex].anchoredPosition;
            newPos.x -= cursorOffsetX; 
            cursorImage.anchoredPosition = newPos;
        }
    }

    private void ExecuteMenu() {
        inputCooldown = 999f; 
        if (currentIndex == 0) {
            OnClickContinue();
        } else {
            OnClickToTitle();
        }
    }

    public void OnClickContinue() {
        if (GameManager.Instance != null) {
            GameManager.Instance.currentLives = 3; 
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