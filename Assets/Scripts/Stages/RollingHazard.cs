/* ===================================================
 * スクリプト名 : RollingHazard.cs
 * 用途 : 物理演算で転がり、プレイヤーを狙う障害物
 * 更新 : 木箱などのオブジェクトにも特大ダメージ(9999)を与えるよう修正
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class RollingHazard : MonoBehaviour{
    [Header("ダメージ設定")]
    public int damage = 2;

    [Header("初速設定")]
    public float initialForce = 3f;
    public float initialTorque = 5f;

    [Header("消滅設定")]
    public float lifeTime = 10f; 

    [Header("効果音設定")]
    public AudioClip impactSE; 
    public float impactThreshold = 2.0f; 

    private Rigidbody2D rb;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null){
            float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
            rb.AddForce(new Vector2(direction * initialForce, 0f), ForceMode2D.Impulse);
            rb.AddTorque(-direction * initialTorque, ForceMode2D.Impulse);
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other){
        IDamageable target = other.GetComponent<IDamageable>();
        
        if (target != null){
            // 1. プレイヤーだった場合（設定された通常のダメージ）
            if (other.CompareTag("Player")){
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                knockbackDir.y = Mathf.Max(knockbackDir.y, 0.5f); 
                target.TakeDamage(damage, knockbackDir);
            }
            // 2. 敵（Enemy）だった場合（HP問わず即死させる！）
            else if (other.GetComponent<Enemy>() != null) {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                target.TakeDamage(9999, dir);
            }
            // 3. ▼【修正】木箱など、その他の場合（大玉専用の 9999 ダメージを与えて粉砕する！）
            else {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                target.TakeDamage(9999, dir);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other){
        if (other.relativeVelocity.magnitude > impactThreshold){
            if (SoundManager.instance != null && impactSE != null){
                SoundManager.instance.PlaySE(impactSE);
            }
        }
    }
}