using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable{ // ←ここがポイント
    public int hp = 3;
    public float knockbackForce = 5f;
    private Rigidbody2D rb;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }

    // IDamageableのルールに従って、ダメージ処理を実装する
    public void TakeDamage(int damage, Vector2 knockbackDirection){
        hp -= damage;

        // ノックバック処理（少し上に浮かせるためにY軸にも力を加える）
        rb.linearVelocity = Vector2.zero; // 今の動きをリセット
        Vector2 force = new Vector2(knockbackDirection.x, 0.5f).normalized * knockbackForce;
        rb.AddForce(force, ForceMode2D.Impulse);

        if (hp <= 0){
            Die();
        }else{
            // ダメージを受けた時のリアクション（赤く点滅させるなど）
            StartCoroutine(DamageEffect());
        }
    }

    private void Die(){
        // 死亡エフェクトなどを出す場合はここ
        Destroy(gameObject);
    }

    private IEnumerator DamageEffect(){
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.white; // 一瞬白くする
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.red;   // 元に戻す
    }
}