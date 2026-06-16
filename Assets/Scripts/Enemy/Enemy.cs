/* ===================================================
* スクリプト名 : Enemy.cs
* Version : Ver0.05
* Since : 2026/04/09
* Update : 2026/06/16
* 用途 : 敵のステータス管理（アニメーション対応版）
* 拡張 : 無敵（攻撃が効かない）フラグの追加
* =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable {
    public int hp = 3;
    public float knockbackTime = 0.2f;

    [Header("無敵設定")]
    [Tooltip("チェックを入れると、プレイヤーからの攻撃を一切受け付けなくなります")]
    public bool isInvincible = false;

    [Header("ドロップ設定")]
    public GameObject itemPrefab; 
    [Range(0, 100)] public int dropChance = 50; 
    
    [Header("演出")]
    public GameObject explosionEffectPrefab; 

    private Rigidbody2D rb;
    private EnemyMovement movementScript; 
    // ▼【追加】アニメーターを制御するための変数
    private Animator anim;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<EnemyMovement>(); 
        // ▼【追加】アタッチされているAnimatorを取得
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        // 無敵の敵なら、ダメージ処理を行わずにここで終了する
        if (isInvincible){
            // ※もし「キンッ！」と弾かれるような音を鳴らしたい場合は、ここに効果音の処理を書きます
            return;
        }

        hp -= damage;
        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(knockbackDirection.x, 0f);
        rb.AddForce(force, ForceMode2D.Impulse);

        // ▼【追加】ダメージを受けた瞬間に、Animatorへ合図を送る
        if (anim != null) {
            anim.SetTrigger("Damage");
        }

        if (hp <= 0){
            Die();
        }else{
            StartCoroutine(DamageRoutine());
        }
    }

    private void Die(){
        if (explosionEffectPrefab != null){
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        if (Random.Range(0, 100) < dropChance && itemPrefab != null){
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private IEnumerator DamageRoutine(){
        // 動きを一時停止
        if (movementScript != null) movementScript.PauseMovement(true);

        // ノックバック時間だけ待つ
        yield return new WaitForSeconds(knockbackTime);

        // 動きを再開
        if (movementScript != null) movementScript.PauseMovement(false);
    }
}