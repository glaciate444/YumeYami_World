/* ===================================================
 * スクリプト名 : GameOverManager.cs
 * 用途 : ゲームオーバー画面での選択肢の処理
 * 拡張 : シーン遷移をSceneNames定数に固定
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class GameOverManager : MonoBehaviour {
    // ▼ 変数 mapSceneName と titleSceneName を削除してスッキリ！

    [Header("UIナビゲーション設定")]
    public RectTransform cursorImage;       
    public RectTransform[] menuPositions;   
    public float cursorOffsetX = 150f;      

    private int currentIndex = 0;           
    private float inputCooldown = 0.2f;     
    private PlayerControls input;

    void Awake() => input = new PlayerControls();
    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();
    void Start() => UpdateCursorPosition();

    void Update() {
        if (inputCooldown > 0f) inputCooldown -= Time.unscaledDeltaTime;

        Vector2 moveDir = input.Player.Move.ReadValue<Vector2>();
        
        if (inputCooldown <= 0f && moveDir.sqrMagnitude > 0.1f) {
            currentIndex = (currentIndex == 0) ? 1 : 0;
            UpdateCursorPosition();
            inputCooldown = 0.2f;
        }

        bool isSubmit = input.Player.Attack.WasPressedThisFrame() || input.Player.Jump.WasPressedThisFrame();
        
        if (isSubmit) ExecuteMenu();
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
        if (currentIndex == 0) OnClickContinue();
        else OnClickToTitle();
    }

    public void OnClickContinue() {
        if (GameManager.Instance != null) {
            GameManager.Instance.currentLives = 3; 
            GameManager.Instance.SaveGame();
        }
        // ▼ 修正：定数を使用
        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadScene(SceneNames.MapSelect, TransitionType.Fade);
        } else {
            SceneManager.LoadScene(SceneNames.MapSelect);
        }
    }

    public void OnClickToTitle() {
        // ▼ 修正：定数を使用
        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadScene(SceneNames.Title, TransitionType.Fade);
        } else {
            SceneManager.LoadScene(SceneNames.Title);
        }
    }
}