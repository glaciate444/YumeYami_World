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

    // ▼▼▼ ここから新規追加 ▼▼▼
    [Header("ステータスUI参照")]
    public TextMeshProUGUI hpCurrentText; // HPの現在値
    public TextMeshProUGUI hpMaxText;     // HPの最大値
    public TextMeshProUGUI spCurrentText; // SPの現在値
    public TextMeshProUGUI spMaxText;     // SPの最大値
    public TextMeshProUGUI apNormalText;  // 普通の攻撃力(Zキー)
    public TextMeshProUGUI apStompText;   // 踏みつけ攻撃力
    public TextMeshProUGUI jpText;        // ジャンプ力
    public TextMeshProUGUI dpText;        // 移動速度
    // ▲▲▲ ここまで新規追加 ▲▲▲

    private bool isPaused = false;

    void Awake(){
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void TogglePause(){
        isPaused = !isPaused;

        if (menuPanel != null){
            menuPanel.SetActive(isPaused);
        }

        // ▼【追加】ポーズ画面が開かれた瞬間にプレイヤーのデータを取得・更新する
        if (isPaused){
            UpdatePersonalData();
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    // ▼▼▼ ここから新規メソッド追加 ▼▼▼
    /// <summary>
    /// プレイヤーから各スクリプトを取得し、UIのテキストを最新の数値に書き換えます
    /// </summary>
 // ▼▼▼ 修正版の UpdatePersonalData メソッド ▼▼▼
    private void UpdatePersonalData(){
        // タグ検索ではなく、シーン内に確実に存在する PlayerController を直接探し出します。
        PlayerController controller = FindFirstObjectByType<PlayerController>();

        // 万が一見つからなかった場合はここで処理をストップ
        if (controller == null){
            Debug.LogError("PlayerControllerが見つかりませんでした。");
            return;
        }

        // 見つけた controller を起点にして、同じオブジェクト（または子要素）に付いているスクリプトを取得します。
        // これにより、タグの付け間違いや階層ズレによるエラーが完全に起きなくなります。
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

        // 【AP（普通 / 踏みつけ）】
        if (apNormalText != null)
            apNormalText.text = (attack != null) ? attack.attackPower.ToString() : "0";

        if (apStompText != null)
            apStompText.text = (stomp != null) ? stomp.stompDamage.ToString() : "0";

        // 【JP / DP】PlayerController から取得
        if (jpText != null) jpText.text = controller.jumpForce.ToString("F0");
        if (dpText != null) dpText.text = controller.moveSpeed.ToString("F0");
    }
    // ▲▲▲ 修正ここまで ▲▲▲

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}