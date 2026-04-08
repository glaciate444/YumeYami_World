using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI操作に必要

public class PlayerHealth : MonoBehaviour, IDamageable{
    [Header("HP設定")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("UI連携")]
    public Slider healthSlider;

    [Header("ノックバック設定")]
    public float knockbackForce = 10f;
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
        currentHealth = maxHealth;

        // 初期UIの更新
        if (healthSlider != null){
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // IDamageableインターフェースの実装
    public void TakeDamage(int damage, Vector2 knockbackDirection){
        if (isInvincible) return; // 無敵中ならダメージを受けない

        currentHealth -= damage;
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
        Vector2 force = new Vector2(direction.x, 0.5f).normalized * knockbackForce;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        playerController.isKnockback = false;
    }

    void UpdateUI(){
        if (healthSlider != null){
            healthSlider.value = currentHealth;
        }
    }

    private IEnumerator DamageEffect(){
        isInvincible = true;

        // ダメージ時の点滅演出
        for (int i = 0; i < 5; i++){
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

    private void Die(){
        // 現在のシーンの名前を取得して再読み込み
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);

        // 特定のチェックポイントからリスポーンさせたい場合は、
        // シーン遷移ではなく、transform.position = checkpoint.position にします。
    }
}
