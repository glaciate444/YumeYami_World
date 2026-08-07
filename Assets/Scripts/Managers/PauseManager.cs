/* ===================================================
 * スクリプト名 : PauseManager.cs
 * Version : Ver0.02
 * Since : 2026/07/13
 * Update : 2026/07/14
 * 用途 : ポーズ画面
 * 更新 : パーソナルデータ追加
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // ▼【追加】TextMeshProを扱うために必要

public class PauseManager : MonoBehaviour {
    public static PauseManager Instance;

    [Header("UI参照")]
    public GameObject menuPanel;

    [Header("コース退出確認用")]
    public GameObject exitDialogPanel;

    [Header("ステータスUI参照")]
    public TextMeshProUGUI hpCurrentText; // HPの現在値
    public TextMeshProUGUI hpMaxText;     // HPの最大値
    public TextMeshProUGUI spCurrentText; // SPの現在値
    public TextMeshProUGUI spMaxText;     // SPの最大値
    public TextMeshProUGUI apNormalText;  // 普通の攻撃力(Zキー)
    public TextMeshProUGUI apStompText;   // 踏みつけ攻撃力
    public TextMeshProUGUI jpText;        // ジャンプ力
    public TextMeshProUGUI dpText;        // 移動速度

    private bool isPaused = false;

    void Awake(){
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (menuPanel != null) menuPanel.SetActive(false);
        // ▼【追加】開始時はダイアログを確実に消しておく
        if (exitDialogPanel != null) exitDialogPanel.SetActive(false);
    }

    public void TogglePause(){
        isPaused = !isPaused;

        if (menuPanel != null){
            menuPanel.SetActive(isPaused);
        }

        // ▼ポーズを閉じた時は、ダイアログも強制的に閉じる
        if (!isPaused && exitDialogPanel != null){
            exitDialogPanel.SetActive(false);
        }

        // ▼ポーズ画面が開かれた瞬間にプレイヤーのデータを取得・更新する
        if (isPaused){
            UpdatePersonalData();
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    // ===============================================
    // ▼▼▼ ここからコース退出用の新規メソッド追加 ▼▼▼
    // ===============================================

    /// <summary>
    /// 「STAGE EXIT」ボタンを押したときに呼ばれる（ダイアログを開く）
    /// </summary>
    public void OpenExitDialog(){
        if (exitDialogPanel != null) exitDialogPanel.SetActive(true);
    }

    /// <summary>
    /// ダイアログで「いいえ」を押したときに呼ばれる（ダイアログを閉じる）
    /// </summary>
    public void CloseExitDialog(){
        if (exitDialogPanel != null) exitDialogPanel.SetActive(false);
    }

    /// <summary>
    /// ダイアログで「はい」を押したときに呼ばれる（マップへ戻る）
    /// </summary>
    public void ConfirmExitCourse(){
        // 1. 時間停止（ポーズ）を解除する
        Time.timeScale = 1f;
        isPaused = false;

        // 2. GameManagerから「元いたマップのシーン名」を取得してロードする
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.returnMapSceneName)){
            // SceneTransitionManagerを使って、フェードアウトしながらマップへ戻る
            if (SceneTransitionManager.Instance != null){
                SceneTransitionManager.Instance.LoadScene(GameManager.Instance.returnMapSceneName, TransitionType.Fade);
            }else{
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameManager.Instance.returnMapSceneName);
            }
        }else{
            // ※GameManagerが無いテストプレイ時などの保険
            Debug.LogWarning("マップのシーン名が記録されていません。仮のマップへ遷移するか、処理を中断します。");
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene"); // ← テスト用マップ名を入れることも可能です
        }
    }

    // ▼ 修正1：外から呼べるように「public」に変更しました
    public void UpdatePersonalData()
    {
        PlayerController controller = FindFirstObjectByType<PlayerController>();

        if (controller == null){
            Debug.LogError("PlayerControllerが見つかりませんでした。");
            return;
        }

        PlayerHealth health = controller.GetComponent<PlayerHealth>();
        PlayerShoot shoot = controller.GetComponent<PlayerShoot>();
        PlayerAttack attack = controller.GetComponentInChildren<PlayerAttack>(true);
        PlayerStomp stomp = controller.GetComponentInChildren<PlayerStomp>(true);

        // 【HP】PlayerHealth から取得
        if (health != null){
            if (hpCurrentText != null) hpCurrentText.text = health.currentHealth.ToString("D2");
            if (hpMaxText != null) hpMaxText.text = health.maxHealth.ToString("D2");
        }else{
            Debug.LogWarning("PlayerHealthが見つかりません");
        }

        // 【SP】PlayerShoot から取得
        if (shoot != null){
            if (spCurrentText != null) spCurrentText.text = shoot.currentSp.ToString("D2");
            if (spMaxText != null) spMaxText.text = shoot.maxSp.ToString("D2");
        }else{
            Debug.LogWarning("PlayerShootが見つかりません");
        }

        // ▼▼▼ 修正2：【AP（普通 / 踏みつけ）】にパッシブボーナスを加算して表示 ▼▼▼
        if (apNormalText != null){
            int normalAp = (attack != null) ? attack.attackPower : 0;
            // 武器を装備していれば足す
            if (attack != null && attack.currentWeaponEquip != null)
            {
                normalAp += attack.currentWeaponEquip.attackPower;
            }
            // パッシブ（ルビー）の分を足す
            normalAp += controller.passiveAttackBonus;

            apNormalText.text = normalAp.ToString();
        }

        if (apStompText != null){
            int stompAp = (stomp != null) ? stomp.stompDamage : 0;
            // パッシブ（ルビー）の分を足す
            stompAp += controller.passiveAttackBonus;

            apStompText.text = stompAp.ToString();
        }
        // ▲▲▲ 修正ここまで ▲▲▲

        // 【JP / DP】PlayerController から取得
        // （ここは ApplyPassiveEffects で直接ジャンプ力自体が書き換わるため、そのまま表示でOKです）
        if (jpText != null) jpText.text = controller.jumpForce.ToString("F0");
        if (dpText != null) dpText.text = controller.moveSpeed.ToString("F0");
    }

    private void OnDestroy(){
        Time.timeScale = 1f;
    }
}