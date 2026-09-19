/* ===================================================
 * スクリプト名 : PlayerHealth.cs
 * 用途 : プレイヤーのHP管理
 * 拡張 : HUDManagerを使用した安全なUI更新（タグ検索廃止）
 * =================================================== */
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable{
    [Header("HP設定")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("ノックバック設定")]
    public float knockbackDuration = 0.2f;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private SpriteRenderer sr;
    
    private bool isInvincible; 
    public float invincibilityDuration = 1.0f;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start(){
        if (GameManager.Instance != null){
            maxHealth = GameManager.Instance.currentMaxHp;
        }
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        if (isInvincible) return; 

        int finalDamage = 0;
        if (damage > 0){
            finalDamage = damage - playerController.passiveDefenseBonus;
            if (finalDamage < 1) finalDamage = 1; 
        }

        currentHealth -= finalDamage;
        UpdateUI();

        StartCoroutine(KnockbackRoutine(knockbackDirection));

        if (currentHealth <= 0){
            Die();
        }else{
            StartCoroutine(DamageEffect()); 
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 direction){
        playerController.isKnockback = true;
        rb.linearVelocity = Vector2.zero;

        float impact = direction.magnitude;
        Vector2 dir = direction.normalized;
        Vector2 force = new Vector2(dir.x, 0.5f).normalized * impact;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);
        playerController.isKnockback = false;
    }

    void UpdateUI(){
        // ▼ HUDManagerへ数値を送るだけ！
        if (HUDManager.Instance != null){
            HUDManager.Instance.UpdateHP(currentHealth, maxHealth);
        }
    }

    private IEnumerator DamageEffect(){
        isInvincible = true;
        float totalInvincibleTime = invincibilityDuration + playerController.passiveInvincibleBonus;
        int blinkCount = Mathf.RoundToInt(totalInvincibleTime / 0.2f);

        for (int i = 0; i < blinkCount; i++){
            sr.color = new Color(1, 1, 1, 0); 
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1); 
            yield return new WaitForSeconds(0.1f);
        }
        isInvincible = false;
    }

    public void Heal(int index){
        if (maxHealth >= currentHealth){
            currentHealth += index;
            if(maxHealth <= currentHealth) currentHealth = maxHealth;
            UpdateUI();
        }
    }

    public void InstantDie(){
        currentHealth = 0;
        UpdateUI();
        Die();
    }

    private void Die(){
        if (GameManager.Instance != null){
            GameManager.Instance.currentLives--;
            if (GameManager.Instance.currentLives < 0){
                SceneManager.LoadScene("GameOverScene"); 
                return; 
            }
        }
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}