/* ===================================================
 * スクリプト名 : PlayerHealth.cs
 * Version : Ver0.03
 * Since : 2026/04/01
 * Update : 2026/08/07
 * 用途 : プレイヤーのHP管理
 * 更新 : 装備によるダメージ軽減
 * =================================================== */
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI操作に必要
using TMPro;

public class PlayerHealth : MonoBehaviour, IDamageable{
    [Header("HP設定")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("UI連携")]
    public Slider healthSlider;
    public TMP_Text healthText;

    [Header("ノックバック設定")]
    public float knockbackDuration = 0.2f;
    private Rigidbody2D rb;
    private PlayerController playerController;

    private SpriteRenderer sr;
    private bool isInvincible; // 無敵時間フラグ
    public float invincibilityDuration = 1.0f;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();

        if (healthSlider == null){
            GameObject sliderObj = GameObject.FindWithTag("HPSlider");
            if (sliderObj != null) healthSlider = sliderObj.GetComponent<Slider>();
        }
    }

    void Start(){
        // GameManager が存在する場合、成長後の最大HPを読み込む
        if (GameManager.Instance != null){
            maxHealth = GameManager.Instance.currentMaxHp;
        }

        // HPを満タンにする（※アクションゲームなのでステージ開始時は満タンにする仕様です）
        currentHealth = maxHealth;

        // UIを最新の最大値で更新する
        if (healthSlider != null){
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (healthText != null){
            healthText.text = currentHealth.ToString();
        }
    }


    // IDamageableインターフェースの実装
    // IDamageableインターフェースの実装
    public void TakeDamage(int damage, Vector2 knockbackDirection){
        if (isInvincible) return; // 無敵中ならダメージを受けない

        int finalDamage = 0;

        // ▼▼▼ 修正：元々のダメージが1以上の攻撃にだけ「防御軽減」と「最低1ダメージ保証」を適用する ▼▼▼
        if (damage > 0){
            finalDamage = damage - playerController.passiveDefenseBonus;
            if (finalDamage < 1) finalDamage = 1; // 軽減しすぎても最低1ダメージは保証する
        }
        // ▲▲▲ 修正ここまで ▲▲▲

        currentHealth -= finalDamage;
        UpdateUI();

        // ノックバック開始
        StartCoroutine(KnockbackRoutine(knockbackDirection));

        if (currentHealth <= 0){
            Die();
        }else{
            StartCoroutine(DamageEffect()); // 点滅演出
        }
    }
    private IEnumerator KnockbackRoutine(Vector2 direction){
        playerController.isKnockback = true;

        // 現在の速度をリセットして、斜め上に弾き飛ばす
        rb.linearVelocity = Vector2.zero;

        // ▼【変更】送られてきたベクトルから、衝撃の強さ（長さ）と方向を取り出す
        float impact = direction.magnitude;
        Vector2 dir = direction.normalized;

        Vector2 force = new Vector2(dir.x, 0.5f).normalized * impact;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        playerController.isKnockback = false;
    }

    void UpdateUI(){
        if (healthSlider != null){
            healthSlider.value = currentHealth;
        }
        if (healthText != null){
            healthText.text = currentHealth.ToString();
        }
    }

    private IEnumerator DamageEffect(){
        isInvincible = true;

        // サファイアの無敵延長効果を加味した長さに変更
        // 基本の無敵時間 + パッシブの延長時間
        float totalInvincibleTime = invincibilityDuration + playerController.passiveInvincibleBonus;

        // 0.2秒（消えて戻る1セット）を何回繰り返せば合計時間に達するか計算
        int blinkCount = Mathf.RoundToInt(totalInvincibleTime / 0.2f);

        // 計算した回数だけ点滅を繰り返す
        for (int i = 0; i < blinkCount; i++){
            sr.color = new Color(1, 1, 1, 0); // 透明
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1); // 不透明
            yield return new WaitForSeconds(0.1f);
        }

        isInvincible = false;
    }
    public void Heal(int index){
        if (maxHealth >= currentHealth){
            currentHealth += index;
            if(maxHealth <= currentHealth){
                currentHealth = maxHealth;
            }
            UpdateUI();
        }
    }
    // ==========================================
    // ▼ ここから追加：落下時などの即死処理（無敵貫通）
    // ==========================================
    public void InstantDie(){
        // HPを強制的に0にしてUIを更新
        currentHealth = 0;
        UpdateUI();

        // 通常の死亡処理（残基減少・シーンリロード等）を呼ぶ
        Die();
    }
    private void Die(){
        // ▼ 1. GameManager が存在する場合（本番環境）
        if (GameManager.Instance != null){
            GameManager.Instance.currentLives--;
            Debug.Log($"ミス！ 残り残基: {GameManager.Instance.currentLives}");

            // ▼ もし残基が0未満（ゲームオーバー）になった場合
            if (GameManager.Instance.currentLives < 0){
                Debug.Log("ゲームオーバー！");

                // 【ここを修正！】ゲームオーバー画面へ強制移動する
                SceneManager.LoadScene("GameOverScene"); 
                return; // 必須：ここで処理を終わらせて、下の「現在のシーン再読み込み」をキャンセルする
            }
        }
        else {
            Debug.Log("【テストモード】GameManagerがないため、無限残基扱いで復活します。");
        }

        // ▼ 3. 残基がまだある場合、現在のシーンを再読み込みして復活（リトライ）
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
